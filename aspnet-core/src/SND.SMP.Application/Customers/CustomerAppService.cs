using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.Customers.Dto;
using System.Runtime.CompilerServices;
using SND.SMP.Users;
using SND.SMP.Users.Dto;
using System.Formats.Asn1;
using SND.SMP.Roles;
using SND.SMP.Roles.Dto;
using Abp.Application.Services.Dto;
using SND.SMP.Wallets;
using SND.SMP.Authorization.Users;
using Microsoft.AspNetCore.Identity;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using Abp.EntityFrameworkCore.Repositories;

namespace SND.SMP.Customers
{
    public class CustomerAppService(IRepository<Customer, long> repository,
        IUserAppService userAppService,
        IRoleAppService roleAppService,
        UserManager userManager
        ) : AsyncCrudAppService<Customer, CustomerDto, long, PagedCustomerResultRequestDto>(repository)
    {
        private readonly IRoleAppService _roleAppService = roleAppService;
        private readonly IUserAppService _userAppService = userAppService;
        private readonly UserManager _userManager = userManager;

        protected override IQueryable<Customer> CreateFilteredQuery(PagedCustomerResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.Code.Contains(input.Keyword) ||
                    x.CompanyName.Contains(input.Keyword) ||
                    x.EmailAddress.Contains(input.Keyword) ||
                    x.AddressLine1.Contains(input.Keyword) ||
                    x.City.Contains(input.Keyword) ||
                    x.State.Contains(input.Keyword) ||
                    x.Country.Contains(input.Keyword) ||
                    x.PhoneNumber.Contains(input.Keyword) ||
                    x.RegistrationNo.Contains(input.Keyword));
        }

        public async Task<CompanyNameAndCode> GetCompanyNameAndCode(string email)
        {
            var customer = await Repository.FirstOrDefaultAsync(x => x.EmailAddress.Equals(email));

            return new CompanyNameAndCode()
            {
                Name = customer is null ? "" : customer.CompanyName,
                Code = customer is null ? "" : customer.Code,
                Id = customer is null ? 0 : customer.Id
            };
        }

        public override async Task<CustomerDto> UpdateAsync(CustomerDto input)
        {
            CheckUpdatePermission();

            var entity = await GetEntityByIdAsync(input.Id);

            MapToEntity(input, entity);
            await CurrentUnitOfWork.SaveChangesAsync();

            return MapToEntityDto(entity);
        }

        public override async Task<CustomerDto> CreateAsync(CustomerDto input)
        {
            CheckCreatePermission();

            var role = await _roleAppService.GetRoleByName("Customer");

            if (role is null)
            {
                List<string> GrantedPermissions =
                [
                    // GrantedPermissions.Add("Pages.Customer.Create");
                    // GrantedPermissions.Add("Pages.Customer.Delete");
                    // GrantedPermissions.Add("Pages.Customer.Edit");
                    "Pages.Customer",
                    "Pages.DispatchValidation"
                ];

                await _roleAppService.CreateAsync(new CreateRoleDto()
                {
                    Name = "Customer",
                    DisplayName = "Customer",
                    NormalizedName = "CUSTOMER",
                    Description = "",
                    GrantedPermissions = GrantedPermissions
                });
            }

            var entity = MapToEntity(input);

            await Repository.InsertAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();

            await _userAppService.CreateAsync(new CreateUserDto()
            {
                UserName = input.EmailAddress,
                Name = input.CompanyName,
                Surname = "",
                EmailAddress = input.EmailAddress,
                IsActive = true,
                RoleNames = ["CUSTOMER"],
                Password = input.Password
            });

            return MapToEntityDto(entity);
        }

        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var customer = await Repository.FirstOrDefaultAsync(x => x.Id.Equals(input));

            if (customer is not null) await _userAppService.GetAndDeleteUserByUsername(customer.EmailAddress);

            await base.DeleteAsync(input);
        }

        public async Task<List<Customer>> GetAllCustomers()
        {
            return await Repository.GetAllListAsync();
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        public async Task<bool> ChangePassword(ChangePasswordDto input)
        {
            await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

            var user = await _userManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            if (user == null)
            {
                throw new Exception("There is no current user!");
            }

            if (await _userManager.CheckPasswordAsync(user, input.CurrentPassword))
            {
                CheckErrors(await _userManager.ChangePasswordAsync(user, input.NewPassword));
            }
            else
            {
                CheckErrors(IdentityResult.Failed(new IdentityError
                {
                    Description = "Incorrect password."
                }));
            }

            var customer = await Repository.FirstOrDefaultAsync(x => x.EmailAddress.Equals(user.EmailAddress));

            if (customer is not null)
            {
                customer.Password = input.NewPassword;

                await Repository.UpdateAsync(customer);
                await Repository.GetDbContext().SaveChangesAsync();
            }

            return true;
        }
    }
}

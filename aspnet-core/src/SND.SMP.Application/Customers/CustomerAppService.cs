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

namespace SND.SMP.Customers
{
    public class CustomerAppService : AsyncCrudAppService<Customer, CustomerDto, long, PagedCustomerResultRequestDto>
    {
        private readonly IRoleAppService _roleAppService;
        private readonly IUserAppService _userAppService;
        private readonly IRepository<Wallet, string> _walletRepository;

        public CustomerAppService(IRepository<Customer, long> repository,
            IUserAppService userAppService,
            IRoleAppService roleAppService,
            IRepository<Wallet, string> walletRepository) : base(repository)
        {
            _userAppService = userAppService;
            _roleAppService = roleAppService;
            _walletRepository = walletRepository;
        }
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
                Code = customer is null ? "" : customer.Code
            };
        }

        public override async Task<CustomerDto> CreateAsync(CustomerDto input)
        {
            CheckCreatePermission();

            var role = await _roleAppService.GetRoleByName("Customer");

            if (role is null)
            {
                List<string> GrantedPermissions = new List<string>();
                GrantedPermissions.Add("Pages.Customer.Create");
                GrantedPermissions.Add("Pages.Customer.Delete");
                GrantedPermissions.Add("Pages.Customer.Edit");
                GrantedPermissions.Add("Pages.Customer");

                CreateRoleDto createRole = new CreateRoleDto()
                {
                    Name = "Customer",
                    DisplayName = "Customer",
                    NormalizedName = "CUSTOMER",
                    Description = "",
                    GrantedPermissions = GrantedPermissions
                };

                var createdRole = await _roleAppService.CreateAsync(createRole);
            }

            var entity = MapToEntity(input);

            await Repository.InsertAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();

            CreateUserDto userdto = new CreateUserDto()
            {
                UserName = input.EmailAddress,
                Name = input.CompanyName,
                Surname = "",
                EmailAddress = input.EmailAddress,
                IsActive = true,
                RoleNames = ["CUSTOMER"],
                Password = input.Password
            };

            var user = await _userAppService.CreateAsync(userdto);

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
    }
}

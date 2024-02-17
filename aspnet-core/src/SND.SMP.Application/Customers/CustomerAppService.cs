using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.Customers.Dto;
using System.Runtime.CompilerServices;
using SND.SMP.Users;
using SND.SMP.Users.Dto;
using System.Formats.Asn1;

namespace SND.SMP.Customers
{
    public class CustomerAppService : AsyncCrudAppService<Customer, CustomerDto, long, PagedCustomerResultRequestDto>
    {

        private readonly IUserAppService _userAppService;

        public CustomerAppService(IRepository<Customer, long> repository,
            IUserAppService userAppService) : base(repository)
        {
            _userAppService = userAppService;
        }
        protected override IQueryable<Customer> CreateFilteredQuery(PagedCustomerResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.Code.Contains(input.Keyword) ||
                    x.CompanyName.Contains(input.Keyword) ||
                    x.EmailAddress.Contains(input.Keyword) ||
                    x.Password.Contains(input.Keyword) ||
                    x.AddressLine1.Contains(input.Keyword) ||
                    x.AddressLine2.Contains(input.Keyword) ||
                    x.City.Contains(input.Keyword) ||
                    x.State.Contains(input.Keyword) ||
                    x.Country.Contains(input.Keyword) ||
                    x.PhoneNumber.Contains(input.Keyword) ||
                    x.RegistrationNo.Contains(input.Keyword) ||
                    x.EmailAddress2.Contains(input.Keyword) ||
                    x.EmailAddress3.Contains(input.Keyword)).AsQueryable();
        }

        public async Task<string> GetCompanyName(string email)
        {
            var customer = await Repository.FirstOrDefaultAsync(x => x.EmailAddress.Equals(email));

            return customer is null ? "" : customer.CompanyName;
        }

        public override async Task<CustomerDto> CreateAsync(CustomerDto input)
        {
            CheckCreatePermission();

            var entity = MapToEntity(input);

            await Repository.InsertAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();

            CreateUserDto userdto = new CreateUserDto()
            {
                UserName = input.EmailAddress,
                Name = input.CompanyName,
                Surname = "-",
                EmailAddress = input.EmailAddress,
                IsActive = true,
                RoleNames = [],
                Password = input.Password
            };

            var user = await _userAppService.CreateAsync(userdto);

            return MapToEntityDto(entity);
        }

        public async Task<List<Customer>> GetAllCustomers()
        {
            return await Repository.GetAllListAsync();
        }
    }
}

using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.CustomerPostals.Dto;

namespace SND.SMP.CustomerPostals
{
    public class CustomerPostalAppService : AsyncCrudAppService<CustomerPostal, CustomerPostalDto, long, PagedCustomerPostalResultRequestDto>
    {

        public CustomerPostalAppService(IRepository<CustomerPostal, long> repository) : base(repository)
        {
        }
        protected override IQueryable<CustomerPostal> CreateFilteredQuery(PagedCustomerPostalResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.Postal.Contains(input.Keyword)).AsQueryable();
        }
    }
}

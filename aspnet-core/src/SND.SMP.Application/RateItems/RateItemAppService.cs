using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.RateItems.Dto;

namespace SND.SMP.RateItems
{
    public class RateItemAppService : AsyncCrudAppService<RateItem, RateItemDto, long, PagedRateItemResultRequestDto>
    {

        public RateItemAppService(IRepository<RateItem, long> repository) : base(repository)
        {
        }
        protected override IQueryable<RateItem> CreateFilteredQuery(PagedRateItemResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.ServiceCode.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword) ||
                    x.CountryCode.Contains(input.Keyword) ||
                    x.PaymentMode.Contains(input.Keyword)).AsQueryable();
        }
    }
}

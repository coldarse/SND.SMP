using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.RateWeightBreaks.Dto;

namespace SND.SMP.RateWeightBreaks
{
    public class RateWeightBreakAppService : AsyncCrudAppService<RateWeightBreak, RateWeightBreakDto, int, PagedRateWeightBreakResultRequestDto>
    {

        public RateWeightBreakAppService(IRepository<RateWeightBreak, int> repository) : base(repository)
        {
        }
        protected override IQueryable<RateWeightBreak> CreateFilteredQuery(PagedRateWeightBreakResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.PostalOrgId.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword) ||
                    x.PaymentMode.Contains(input.Keyword)).AsQueryable();
        }
    }
}

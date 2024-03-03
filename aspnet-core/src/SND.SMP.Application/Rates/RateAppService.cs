using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.Rates.Dto;

namespace SND.SMP.Rates
{
    public class RateAppService : AsyncCrudAppService<Rate, RateDto, int, PagedRateResultRequestDto>
    {

        public RateAppService(IRepository<Rate, int> repository) : base(repository)
        {
        }
        protected override IQueryable<Rate> CreateFilteredQuery(PagedRateResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.CardName.Contains(input.Keyword)).AsQueryable();
        }
    }
}

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

        public async Task<List<Rate>> GetRates()
        {
            return await Repository.GetAllListAsync();
        }

        public async Task<List<RateDDL>> GetRateDDL()
        {
            var rates = await Repository.GetAllListAsync();

            List<RateDDL> rateDDL = new List<RateDDL>();
            foreach (var rate in rates.ToList())
            {
                rateDDL.Add(new RateDDL()
                {
                    Id = rate.Id,
                    CardName = rate.CardName
                });
            }

            return rateDDL;
        }
    }
}

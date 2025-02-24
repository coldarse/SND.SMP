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
using SND.SMP.Rates.Dto;

namespace SND.SMP.Rates
{
    public class RateAppService(IRepository<Rate, int> repository) : AsyncCrudAppService<Rate, RateDto, int, PagedRateResultRequestDto>(repository)
    {
        protected override IQueryable<Rate> CreateFilteredQuery(PagedRateResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.CardName.Contains(input.Keyword));
        }

        public async Task<List<Rate>> GetDERates()
        {
            return await Repository.GetAllListAsync(x => x.Service.Equals("DE"));
        }

        public async Task<List<RateDDL>> GetRateDDL()
        {
            var rates = await Repository.GetAllListAsync();

            List<RateDDL> rateDDL = [];
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

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
using SND.SMP.RateZones.Dto;

namespace SND.SMP.RateZones
{
    public class RateZoneAppService : AsyncCrudAppService<RateZone, RateZoneDto, long, PagedRateZoneResultRequestDto>
    {

        public RateZoneAppService(IRepository<RateZone, long> repository) : base(repository)
        {
        }
        protected override IQueryable<RateZone> CreateFilteredQuery(PagedRateZoneResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.Zone.Contains(input.Keyword) ||
                    x.State.Contains(input.Keyword) ||
                    x.City.Contains(input.Keyword));
        }
    }
}

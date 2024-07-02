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
using SND.SMP.Airports.Dto;

namespace SND.SMP.Airports
{
    public class AirportAppService(IRepository<Airport, int> repository) : AsyncCrudAppService<Airport, AirportDto, int, PagedAirportResultRequestDto>(repository)
    {
        protected override IQueryable<Airport> CreateFilteredQuery(PagedAirportResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.Name.Contains(input.Keyword) ||
                    x.Code.Contains(input.Keyword) ||
                    x.Country.Contains(input.Keyword));
        }

        public async Task<List<AirportDto>> GetAirportList()
        {
            var airports = await Repository.GetAllListAsync();

            return ObjectMapper.Map<List<AirportDto>>(airports);
        }
    }
}

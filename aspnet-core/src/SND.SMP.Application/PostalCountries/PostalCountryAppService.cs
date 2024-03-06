using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.PostalCountries.Dto;

namespace SND.SMP.PostalCountries
{
    public class PostalCountryAppService : AsyncCrudAppService<PostalCountry, PostalCountryDto, long, PagedPostalCountryResultRequestDto>
    {

        public PostalCountryAppService(IRepository<PostalCountry, long> repository) : base(repository)
        {
        }
        protected override IQueryable<PostalCountry> CreateFilteredQuery(PagedPostalCountryResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.PostalCode.Contains(input.Keyword) ||
                    x.CountryCode.Contains(input.Keyword)).AsQueryable();
        }
    }
}

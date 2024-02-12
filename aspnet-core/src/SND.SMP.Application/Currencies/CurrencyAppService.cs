using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.Currencies.Dto;

namespace SND.SMP.Currencies
{
    public class CurrencyAppService : AsyncCrudAppService<Currency, CurrencyDto, long, PagedCurrencyResultRequestDto>
    {

        public CurrencyAppService(IRepository<Currency, long> repository) : base(repository)
        {
        }
        protected override IQueryable<Currency> CreateFilteredQuery(PagedCurrencyResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.Abbr.Contains(input.Keyword) ||
                    x.Description.Contains(input.Keyword)).AsQueryable();
        }
    }
}

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
using SND.SMP.ItemMins.Dto;

namespace SND.SMP.ItemMins
{
    public class ItemMinAppService : AsyncCrudAppService<ItemMin, ItemMinDto, string, PagedItemMinResultRequestDto>
    {

        public ItemMinAppService(IRepository<ItemMin, string> repository) : base(repository)
        {
        }
        protected override IQueryable<ItemMin> CreateFilteredQuery(PagedItemMinResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.ExtID                         .Contains(input.Keyword) ||
                    x.CountryCode                   .Contains(input.Keyword) ||
                    x.RecpName                      .Contains(input.Keyword) ||
                    x.ItemDesc                      .Contains(input.Keyword) ||
                    x.Address                       .Contains(input.Keyword) ||
                    x.City                          .Contains(input.Keyword) ||
                    x.TelNo                         .Contains(input.Keyword));
        }
    }
}

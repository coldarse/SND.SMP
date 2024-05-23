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
using SND.SMP.ItemIdRunningNos.Dto;

namespace SND.SMP.ItemIdRunningNos
{
    public class ItemIdRunningNoAppService : AsyncCrudAppService<ItemIdRunningNo, ItemIdRunningNoDto, long, PagedItemIdRunningNoResultRequestDto>
    {

        public ItemIdRunningNoAppService(IRepository<ItemIdRunningNo, long> repository) : base(repository)
        {
        }
        protected override IQueryable<ItemIdRunningNo> CreateFilteredQuery(PagedItemIdRunningNoResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.Prefix.Contains(input.Keyword) ||
                    x.PrefixNo.Contains(input.Keyword) ||
                    x.Suffix.Contains(input.Keyword));
        }
    }
}

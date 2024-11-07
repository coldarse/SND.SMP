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
using SND.SMP.ItemTrackingEvents.Dto;

namespace SND.SMP.ItemTrackingEvents
{
    public class ItemTrackingEventAppService : AsyncCrudAppService<ItemTrackingEvent, ItemTrackingEventDto, long, PagedItemTrackingEventResultRequestDto>
    {

        public ItemTrackingEventAppService(IRepository<ItemTrackingEvent, long> repository) : base(repository)
        {
        }
        protected override IQueryable<ItemTrackingEvent> CreateFilteredQuery(PagedItemTrackingEventResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.TrackingNo.Contains(input.Keyword) ||
                    x.Status.Contains(input.Keyword) ||
                    x.Country.Contains(input.Keyword) ||
                    x.DispatchNo.Contains(input.Keyword));
        }
    }
}

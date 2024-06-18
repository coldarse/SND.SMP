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
using SND.SMP.ItemTrackings.Dto;

namespace SND.SMP.ItemTrackings
{
    public class ItemTrackingAppService(IRepository<ItemTracking, int> repository) : AsyncCrudAppService<ItemTracking, ItemTrackingDto, int, PagedItemTrackingResultRequestDto>(repository)
    {
        protected override IQueryable<ItemTracking> CreateFilteredQuery(PagedItemTrackingResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.TrackingNo.Contains(input.Keyword) ||
                    x.CustomerCode.Contains(input.Keyword) ||
                    x.DispatchNo.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword));
        }
    }
}

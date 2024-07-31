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
using SND.SMP.TrackingStatuses.Dto;

namespace SND.SMP.TrackingStatuses
{
    public class TrackingStatusAppService : AsyncCrudAppService<TrackingStatus, TrackingStatusDto, long, PagedTrackingStatusResultRequestDto>
    {

        public TrackingStatusAppService(IRepository<TrackingStatus, long> repository) : base(repository)
        {
        }
        protected override IQueryable<TrackingStatus> CreateFilteredQuery(PagedTrackingStatusResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.TrackingNo.Contains(input.Keyword) ||
                    x.DispatchNo.Contains(input.Keyword) ||
                    x.Status.Contains(input.Keyword));
        }
    }
}

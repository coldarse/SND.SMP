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
using SND.SMP.TrackingNoForUpdates.Dto;

namespace SND.SMP.TrackingNoForUpdates
{
    public class TrackingNoForUpdateAppService : AsyncCrudAppService<TrackingNoForUpdate, TrackingNoForUpdateDto, long, PagedTrackingNoForUpdateResultRequestDto>
    {

        public TrackingNoForUpdateAppService(IRepository<TrackingNoForUpdate, long> repository) : base(repository)
        {
        }
        protected override IQueryable<TrackingNoForUpdate> CreateFilteredQuery(PagedTrackingNoForUpdateResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.TrackingNo.Contains(input.Keyword) ||
                    x.DispatchNo.Contains(input.Keyword) ||
                    x.ProcessType.Contains(input.Keyword));
        }
    }
}

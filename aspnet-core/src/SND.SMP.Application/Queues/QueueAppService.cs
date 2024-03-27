using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.Queues.Dto;

namespace SND.SMP.Queues
{
    public class QueueAppService : AsyncCrudAppService<Queue, QueueDto, long, PagedQueueResultRequestDto>
    {

        public QueueAppService(IRepository<Queue, long> repository) : base(repository)
        {
        }
        protected override IQueryable<Queue> CreateFilteredQuery(PagedQueueResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.EventType.Contains(input.Keyword) ||
                    x.FilePath.Contains(input.Keyword) ||
                    x.Status.Contains(input.Keyword) ||
                    x.ErrorMsg.Contains(input.Keyword)).AsQueryable();
        }
    }
}

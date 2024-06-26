using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using Abp.Linq.Extensions;
using System.Linq;
using SND.SMP.Queues.Dto;
using System.Threading.Tasks;
using Abp.UI;
using Abp.EntityFrameworkCore.Repositories;

namespace SND.SMP.Queues
{
    public class QueueAppService(IRepository<Queue, long> repository) : AsyncCrudAppService<Queue, QueueDto, long, PagedQueueResultRequestDto>(repository)
    {
        protected override IQueryable<Queue> CreateFilteredQuery(PagedQueueResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.EventType.Contains(input.Keyword) ||
                    x.FilePath.Contains(input.Keyword) ||
                    x.Status.Contains(input.Keyword) ||
                    x.ErrorMsg.Contains(input.Keyword));
        }

        public async Task<bool> GetDispatchValidationUpdateStatusByFilePath(string filePath)
        {
            var queue = await Repository.FirstOrDefaultAsync(x => (x.FilePath == filePath) && (x.EventType == "Validate Dispatch"));

            if (queue is null) return false;

            queue.Status = "New";
            queue.TookInSec = 0;

            await Repository.UpdateAsync(queue);
            await Repository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

            return true;
        }
    }
}

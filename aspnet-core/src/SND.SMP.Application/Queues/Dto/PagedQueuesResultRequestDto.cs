using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.Queues
{

    public class PagedQueueResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string EventType { get; set; }
        public string FilePath { get; set; }
        public bool? DeleteFileOnSuccess { get; set; }
        public bool? DeleteFileOnFailed { get; set; }
        public DateTime? DateCreated { get; set; }
        public string Status { get; set; }
        public double? TookInSec { get; set; }
        public string ErrorMsg { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}

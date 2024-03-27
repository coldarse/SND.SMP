using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.Queues.Dto
{

    [AutoMap(typeof(Queue))]
    public class QueueDto : EntityDto<long>
    {
        public string EventType { get; set; }
        public string FilePath { get; set; }
        public bool DeleteFileOnSuccess { get; set; }
        public bool DeleteFileOnFailed { get; set; }
        public DateTime DateCreated { get; set; }
        public string Status { get; set; }
        public double TookInSec { get; set; }
        public string ErrorMsg { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}

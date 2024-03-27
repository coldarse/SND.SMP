using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.Queues
{

    public class Queue : Entity<long>
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

using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.TrackingStatuses
{

    public class TrackingStatus : Entity<long>
    {
        public string TrackingNo { get; set; }
        public int DispatchId { get; set; }
        public string DispatchNo { get; set; }
        public string Status { get; set; }
        public DateTime DateTime { get; set; }
    }
}

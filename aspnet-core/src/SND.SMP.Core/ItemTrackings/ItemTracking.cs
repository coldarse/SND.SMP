using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.ItemTrackings
{

    public class ItemTracking : Entity<int>
    {
        public string TrackingNo { get; set; }
        public int ApplicationId { get; set; }
        public int ReviewId { get; set; }
        public long CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUsed { get; set; }
        public int DispatchId { get; set; }
        public string DispatchNo { get; set; }
        public string ProductCode { get; set; }
        public bool IsExternal { get; set; }
    }
}

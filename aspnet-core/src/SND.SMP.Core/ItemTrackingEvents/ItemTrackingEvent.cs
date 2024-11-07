using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.ItemTrackingEvents
{

    public class ItemTrackingEvent : Entity<long>
    {
        public string TrackingNo { get; set; }
        public int Event { get; set; }
        public string Status { get; set; }
        public string Country { get; set; }
        public DateTime EventTime { get; set; }
        public string DispatchNo { get; set; }
    }
}

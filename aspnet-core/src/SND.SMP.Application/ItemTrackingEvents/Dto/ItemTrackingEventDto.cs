using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.ItemTrackingEvents.Dto
{

    [AutoMap(typeof(ItemTrackingEvent))]
    public class ItemTrackingEventDto : EntityDto<long>
    {
        public string TrackingNo { get; set; }
        public int? Event { get; set; }
        public string Status { get; set; }
        public string Country { get; set; }
        public DateTime? EventTime { get; set; }
        public string DispatchNo { get; set; }
    }
}

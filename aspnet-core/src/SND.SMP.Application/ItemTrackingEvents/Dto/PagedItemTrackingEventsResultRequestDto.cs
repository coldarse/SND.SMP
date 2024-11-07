using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.ItemTrackingEvents
{

    public class PagedItemTrackingEventResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string TrackingNo { get; set; }
        public int? Event { get; set; }
        public string Status { get; set; }
        public string Country { get; set; }
        public DateTime? EventTime { get; set; }
        public string DispatchNo { get; set; }
    }
}

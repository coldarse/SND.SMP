using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.ItemTrackings
{

    public class PagedItemTrackingResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string TrackingNo { get; set; }
        public int? ApplicationId { get; set; }
        public int? ReviewId { get; set; }
        public long? CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateUsed { get; set; }
        public int? DispatchId { get; set; }
        public string DispatchNo { get; set; }
        public string ProductCode { get; set; }
    }
}

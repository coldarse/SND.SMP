using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.TrackingStatuses
{

    public class PagedTrackingStatusResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string TrackingNo { get; set; }
        public int? DispatchId { get; set; }
        public string DispatchNo { get; set; }
        public string Status { get; set; }
        public DateTime? DateTime { get; set; }
    }
}

using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.TrackingNoForUpdates
{

    public class PagedTrackingNoForUpdateResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string TrackingNo { get; set; }
        public string DispatchNo { get; set; }
        public string ProcessType { get; set; }
    }
}

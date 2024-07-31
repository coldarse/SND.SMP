using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.TrackingStatuses.Dto
{

    [AutoMap(typeof(TrackingStatus))]
    public class TrackingStatusDto : EntityDto<long>
    {
        public string TrackingNo { get; set; }
        public int? DispatchId { get; set; }
        public string DispatchNo { get; set; }
        public string Status { get; set; }
        public DateTime? DateTime { get; set; }
    }
}

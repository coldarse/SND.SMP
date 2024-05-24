using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.ItemTrackings.Dto
{

    [AutoMap(typeof(ItemTracking))]
    public class ItemTrackingDto : EntityDto<int>
    {
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

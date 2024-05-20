using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.ItemTrackingApplications.Dto
{

    [AutoMap(typeof(ItemTrackingApplication))]
    public class ItemTrackingApplicationDto : EntityDto<int>
    {
        public long? CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string PostalCode { get; set; }
        public string PostalDesc { get; set; }
        public int? Total { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string Status { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}

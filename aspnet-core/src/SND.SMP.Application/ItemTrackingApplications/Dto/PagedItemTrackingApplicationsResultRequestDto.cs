using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.ItemTrackingApplications
{

    public class PagedItemTrackingApplicationResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public long? CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string PostalCode { get; set; }
        public string PostalDesc { get; set; }
        public int? Total { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string Status { get; set; }
        public DateTime? DateCreated { get; set; }
        public string Range { get; set; }
        public string Path { get; set; }
    }
}

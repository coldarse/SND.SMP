using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.ItemTrackingReviews
{

    public class PagedItemTrackingReviewResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public int? ApplicationId { get; set; }
        public long? CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public int? Total { get; set; }
        public string PostalCode { get; set; }
        public string PostalDesc { get; set; }
        public DateTime? DateCreated { get; set; }
        public string Status { get; set; }
        public int? TotalGiven { get; set; }
        public string Prefix { get; set; }
        public string PrefixNo { get; set; }
        public string Suffix { get; set; }
        public string ProductCode { get; set; }
    }
}

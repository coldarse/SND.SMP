using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.ItemTrackingReviews
{

    public class ItemTrackingReview : Entity<int>
    {
        public int ApplicationId { get; set; }
        public long CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public int Total { get; set; }
        public string PostalCode { get; set; }
        public string PostalDesc { get; set; }
        public DateTime DateCreated { get; set; }
        public string Status { get; set; }
        public int TotalGiven { get; set; }
        public string Prefix { get; set; }
        public string PrefixNo { get; set; }
        public string Suffix { get; set; }
        public string ProductCode { get; set; }
        public string Remark { get; set; }
    }

    public class PrefixSuffixCustomerCode
    {
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string CustomerCode { get; set; }
    }
}

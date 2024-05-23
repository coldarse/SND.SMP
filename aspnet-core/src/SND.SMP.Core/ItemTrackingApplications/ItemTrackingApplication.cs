using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.ItemTrackingApplications
{

    public class ItemTrackingApplication : Entity<int>
    {
        public long CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string PostalCode { get; set; }
        public string PostalDesc { get; set; }
        public int Total { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public string Range { get; set; }
    }
}

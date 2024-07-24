using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.ItemMins
{

    public class ItemMin : Entity<string>
    {
        public string ExtID { get; set; }
        [Key]
        public int? DispatchID { get; set; }
        public int? BagID { get; set; }
        public DateOnly? DispatchDate { get; set; }
        public int? Month { get; set; }
        public string CountryCode { get; set; }
        public decimal? Weight { get; set; }
        public decimal? ItemValue { get; set; }
        public string RecpName { get; set; }
        public string ItemDesc { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string TelNo { get; set; }
        public int? DeliveredInDays { get; set; }
        public bool? IsDelivered { get; set; }
        public int? Status { get; set; }
    }
}

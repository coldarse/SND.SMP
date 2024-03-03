using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.RateItems
{

    public class RateItem : Entity<long>
    {
        public int RateId { get; set; }
        public string ServiceCode { get; set; }
        public string ProductCode { get; set; }
        public string CountryCode { get; set; }
        public decimal Total { get; set; }
        public decimal Fee { get; set; }
        public long CurrencyId { get; set; }
        public string PaymentMode { get; set; }
    }
}

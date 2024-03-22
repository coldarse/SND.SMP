using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.RateWeightBreaks
{

    public class RateWeightBreak : Entity<int>
    {
        public int RateId { get; set; }
        public string PostalOrgId { get; set; }
        public decimal? WeightMin { get; set; }
        public decimal? WeightMax { get; set; }
        public string ProductCode { get; set; }
        public long CurrencyId { get; set; }
        public decimal? ItemRate { get; set; }
        public decimal? WeightRate { get; set; }
        public bool IsExceedRule { get; set; }
        public string PaymentMode { get; set; }
    }
}

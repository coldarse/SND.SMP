using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.RateWeightBreaks.Dto
{

    [AutoMap(typeof(RateWeightBreak))]
    public class RateWeightBreakDto : EntityDto<int>
    {
        public int RateId { get; set; }
        public string PostalOrgId { get; set; }
        public decimal WeightMin { get; set; }
        public decimal WeightMax { get; set; }
        public string ProductCode { get; set; }
        public long CurrencyId { get; set; }
        public decimal ItemRate { get; set; }
        public decimal WeightRate { get; set; }
        public bool IsExceedRule { get; set; }
        public string PaymentMode { get; set; }
    }
}

using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.RateItems.Dto
{

    [AutoMap(typeof(RateItem))]
    public class RateItemDto : EntityDto<long>
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

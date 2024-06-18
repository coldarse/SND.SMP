using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.DispatchUsedAmounts.Dto
{

    [AutoMap(typeof(DispatchUsedAmount))]
    public class DispatchUsedAmountDto : EntityDto<int>
    {
        public string CustomerCode { get; set; }
        public string Wallet { get; set; }
        public decimal Amount { get; set; }
        public string DispatchNo { get; set; }
        public DateTime? DateTime { get; set; }
        public string Description { get; set; }
    }
}

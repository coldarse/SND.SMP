using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.DispatchUsedAmounts
{

    public class PagedDispatchUsedAmountResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string CustomerCode { get; set; }
        public string Wallet { get; set; }
        public decimal Amount { get; set; }
        public string DispatchNo { get; set; }
        public DateTime? DateTime { get; set; }
        public string Description { get; set; }
    }
}

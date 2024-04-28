using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.Refunds
{

    public class PagedRefundResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public int? UserId { get; set; }
        public string ReferenceNo { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime? DateTime { get; set; }
        public decimal Weight { get; set; }
    }
}

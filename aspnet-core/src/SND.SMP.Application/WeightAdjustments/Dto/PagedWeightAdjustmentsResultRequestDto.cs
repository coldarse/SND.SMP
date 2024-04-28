using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.WeightAdjustments
{

    public class PagedWeightAdjustmentResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public int? UserId { get; set; }
        public string ReferenceNo { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime? DateTime { get; set; }
        public decimal Weight { get; set; }
        public int? InvoiceId { get; set; }
    }
}

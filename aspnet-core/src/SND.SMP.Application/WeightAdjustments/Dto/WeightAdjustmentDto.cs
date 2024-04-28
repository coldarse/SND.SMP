using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.WeightAdjustments.Dto
{

    [AutoMap(typeof(WeightAdjustment))]
    public class WeightAdjustmentDto : EntityDto<int>
    {
        public int? UserId { get; set; }
        public string ReferenceNo { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime? DateTime { get; set; }
        public decimal Weight { get; set; }
        public int? InvoiceId { get; set; }
    }
}

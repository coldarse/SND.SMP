using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.Refunds.Dto
{

    [AutoMap(typeof(Refund))]
    public class RefundDto : EntityDto<int>
    {
        public int? UserId { get; set; }
        public string ReferenceNo { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime? DateTime { get; set; }
        public decimal Weight { get; set; }
    }
}

using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.Invoices.Dto
{

    [AutoMap(typeof(Invoice))]
    public class InvoiceDto : EntityDto<int>
    {
        public DateTime? DateTime { get; set; }
        public string InvoiceNo { get; set; }
        public string Customer { get; set; }
    }
}

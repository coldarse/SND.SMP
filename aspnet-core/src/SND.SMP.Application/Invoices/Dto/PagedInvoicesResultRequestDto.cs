using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.Invoices
{

    public class PagedInvoiceResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public DateTime? DateTime { get; set; }
        public string InvoiceNo { get; set; }
        public string Customer { get; set; }
    }
}

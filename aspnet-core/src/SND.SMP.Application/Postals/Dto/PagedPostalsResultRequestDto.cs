using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.Postals
{

    public class PagedPostalResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string PostalCode { get; set; }
        public string PostalDesc { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceDesc { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public decimal? ItemTopUpValue { get; set; }
    }
}

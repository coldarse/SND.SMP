using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.CustomerPostals
{

    public class PagedCustomerPostalResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string Postal { get; set; }
        public int? Rate { get; set; }
        public long? AccountNo { get; set; }
    }
}

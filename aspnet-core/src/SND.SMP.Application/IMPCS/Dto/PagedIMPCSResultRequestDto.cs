using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.IMPCS
{

    public class PagedIMPCResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string Type { get; set; }
        public string CountryCode { get; set; }
        public string AirportCode { get; set; }
        public string IMPCCode { get; set; }
        public string LogisticCode { get; set; }
    }
}

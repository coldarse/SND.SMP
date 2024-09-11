using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.RateZones
{

    public class PagedRateZoneResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string Zone { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
    }
}

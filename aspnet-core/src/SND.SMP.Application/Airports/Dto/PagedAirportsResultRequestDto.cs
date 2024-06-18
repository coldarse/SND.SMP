using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.Airports
{

    public class PagedAirportResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Country { get; set; }
    }
}

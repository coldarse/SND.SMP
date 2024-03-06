using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.PostalCountries
{

    public class PagedPostalCountryResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
    }
}

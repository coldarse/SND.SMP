using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.Currencies
{

    public class PagedCurrencyResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string Abbr { get; set; }
        public string Description { get; set; }
    }
}

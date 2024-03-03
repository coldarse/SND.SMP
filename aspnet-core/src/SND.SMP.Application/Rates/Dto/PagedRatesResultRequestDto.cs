using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.Rates
{

    public class PagedRateResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string CardName { get; set; }
        public long? Count { get; set; }
    }
}

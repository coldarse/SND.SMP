using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.EWalletTypes
{

    public class PagedEWalletTypeResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string Type { get; set; }
    }
}

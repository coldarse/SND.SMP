using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.Wallets
{

    public class PagedWalletResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string? Customer { get; set; }
        public long? EWalletType { get; set; }
        public long? Currency { get; set; }
    }
}

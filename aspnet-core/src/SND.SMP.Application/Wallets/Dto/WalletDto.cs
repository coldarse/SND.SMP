using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.Wallets.Dto
{

    [AutoMap(typeof(Wallet))]
    public class WalletDto : EntityDto<custom>
    {
        public long Customer { get; set; }
        public long EWalletType { get; set; }
        public long Currency { get; set; }
    }
}

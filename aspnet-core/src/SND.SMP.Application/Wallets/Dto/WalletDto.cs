using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace SND.SMP.Wallets.Dto
{

    [AutoMap(typeof(Wallet))]
    public class WalletDto : EntityDto<string>
    {
        public string Customer { get; set; }
        public long EWalletType { get; set; }
        public long Currency { get; set; }
    }
}

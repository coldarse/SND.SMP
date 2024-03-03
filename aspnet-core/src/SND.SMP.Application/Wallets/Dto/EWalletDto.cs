using System.Collections.Generic;
using Abp.Application.Services.Dto;
using SND.SMP.Currencies;
using SND.SMP.EWalletTypes;

public class EWalletDto : EntityDto<string>
{
    public string Customer { get; set; }
    public long EWalletType { get; set; }
    public string EWalletTypeDesc { get; set; }
    public long Currency { get; set; }
    public string CurrencyDesc { get; set; }
    public List<EWalletType> EWalletTypeList { get; set; }
    public List<Currency> CurrencyList { get; set; }
    public decimal Balance { get; set; }
}
using System.Collections.Generic;
using SND.SMP.Currencies;
using SND.SMP.EWalletTypes;

public class EWalletDto
{
    public string Customer { get; set; }
    public long EWalletType { get; set; }
    public string EWalletTypeDesc { get; set; }
    public long Currency { get; set; }
    public string CurrencyDesc { get; set; }
    public List<EWalletType> EWalletTypeList { get; set; }
    public List<Currency> CurrencyList { get; set; }
}
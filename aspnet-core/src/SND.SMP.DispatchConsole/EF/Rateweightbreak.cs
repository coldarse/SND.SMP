using System;
using System.Collections.Generic;

namespace SND.SMP.DispatchConsole.EF;

public partial class Rateweightbreak
{
    public uint Id { get; set; }

    public uint? RateId { get; set; }

    public string PostalOrgId { get; set; }

    public decimal? WeightMin { get; set; }

    public decimal? WeightMax { get; set; }

    public string ProductCode { get; set; }

    public int CurrencyId { get; set; }

    public decimal? ItemRate { get; set; }

    public decimal? WeightRate { get; set; }

    public ulong? IsExceedRule { get; set; }

    public string PaymentMode { get; set; }

    public virtual Postalorg PostalOrg { get; set; }

    public virtual Rate Rate { get; set; }
}

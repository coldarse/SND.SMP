using System;
using System.Collections.Generic;

namespace SND.SMP.DispatchImporter.EF;

public partial class Rateitem
{
    public int Id { get; set; }

    public uint? RateId { get; set; }

    public string ServiceCode { get; set; }

    public string ProductCode { get; set; }

    public string CountryCode { get; set; }

    public decimal? Total { get; set; }

    public decimal? Fee { get; set; }

    public int CurrencyId { get; set; }

    public string PaymentMode { get; set; }

    public virtual Rate Rate { get; set; }
}

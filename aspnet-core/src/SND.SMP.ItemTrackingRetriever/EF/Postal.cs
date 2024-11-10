using System;
using System.Collections.Generic;

namespace SND.SMP.ItemTrackingRetriever.EF;

public partial class Postal
{
    public uint Id { get; set; }

    public string PostalCode { get; set; }

    public string PostalDescription { get; set; }

    public string ServiceCode { get; set; }

    public string ServiceDescription { get; set; }

    public string ProductCode { get; set; }

    public string ProductDescription { get; set; }

    public decimal? ItemTopupValue { get; set; }
}

using System;
using System.Collections.Generic;

namespace SND.SMP.ItemTrackingGenerator.EF;

public partial class Customercurrency
{
    public uint Id { get; set; }

    public string CustomerCode { get; set; }

    public string CurrencyId { get; set; }

    public decimal? Balance { get; set; }

    public virtual Currency Currency { get; set; }

    public virtual Customer CustomerCodeNavigation { get; set; }
}

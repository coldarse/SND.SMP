using System;
using System.Collections.Generic;

namespace SND.SMP.DispatchTrackingUpdater.EF;

public partial class Currency
{
    public string Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Customercurrency> Customercurrencies { get; set; } = new List<Customercurrency>();
}

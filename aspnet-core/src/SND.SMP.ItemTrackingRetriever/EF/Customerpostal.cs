using System;
using System.Collections.Generic;

namespace SND.SMP.ItemTrackingRetriever.EF;

public partial class Customerpostal
{
    public uint Id { get; set; }

    public string AccountNo { get; set; }

    public uint? Rate { get; set; }

    public string Postal { get; set; }

    public virtual Customer CustomerCodeNavigation { get; set; }

    public virtual Rate RateNav { get; set; }
}

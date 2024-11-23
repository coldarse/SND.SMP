using System;
using System.Collections.Generic;

namespace SND.SMP.ItemTrackingGenerator.EF;

public partial class Rate
{
    public uint Id { get; set; }

    public string CardName { get; set; }

    public virtual ICollection<Customerpostal> Customerpostals { get; set; } = new List<Customerpostal>();

    public virtual ICollection<Rateitem> Rateitems { get; set; } = new List<Rateitem>();

    public virtual ICollection<Rateweightbreak> Rateweightbreaks { get; set; } = new List<Rateweightbreak>();
}

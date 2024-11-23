using System;
using System.Collections.Generic;

namespace SND.SMP.ItemTrackingGenerator.EF;

public partial class Postalorg
{
    public string Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Rateweightbreak> Rateweightbreaks { get; set; } = new List<Rateweightbreak>();
}

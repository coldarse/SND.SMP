using System;
using System.Collections.Generic;

namespace SND.SMP.DispatchValidator.EF;

public partial class Bag
{
    public uint Id { get; set; }

    public string BagNo { get; set; }

    public uint? DispatchId { get; set; }

    public string CountryCode { get; set; }

    public decimal? WeightPre { get; set; }

    public decimal? WeightPost { get; set; }

    public int? ItemCountPre { get; set; }

    public int? ItemCountPost { get; set; }

    public decimal? WeightVariance { get; set; }

    public string Cn35no { get; set; }

    public decimal? UnderAmount { get; set; }

    public virtual Dispatch Dispatch { get; set; }

    public virtual ICollection<Itemmin> Itemmins { get; set; } = new List<Itemmin>();

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}

using System;
using System.Collections.Generic;

namespace SND.SMP.DispatchValidator.EF;

public partial class Itemmin
{
    public string Id { get; set; }

    public string ExtId { get; set; }

    public uint? DispatchId { get; set; }

    public uint? BagId { get; set; }

    public DateOnly? DispatchDate { get; set; }

    public int? Month { get; set; }

    public string CountryCode { get; set; }

    public decimal? Weight { get; set; }

    public decimal? ItemValue { get; set; }

    public string RecpName { get; set; }

    public string ItemDesc { get; set; }

    public string Address { get; set; }

    public string City { get; set; }

    public string TelNo { get; set; }

    public int? DeliveredInDays { get; set; }

    public ulong? IsDelivered { get; set; }

    public int? Status { get; set; }

    public virtual Bag Bag { get; set; }

    public virtual Dispatch Dispatch { get; set; }
}

using System;
using System.Collections.Generic;

namespace SND.SMP.ItemTrackingGenerator.EF;

public partial class Postalcountry
{
    public uint Id { get; set; }

    public string PostalCode { get; set; }

    public string CountryCode { get; set; }
}

using System;
using System.Collections.Generic;

namespace SND.SMP.DispatchConsole.EF;

public partial class Customerpostal
{
    public uint Id { get; set; }

    public string CustomerCode { get; set; }

    public uint? RateId { get; set; }

    public string PostalCode { get; set; }

    public virtual Customer CustomerCodeNavigation { get; set; }

    public virtual Rate Rate { get; set; }
}

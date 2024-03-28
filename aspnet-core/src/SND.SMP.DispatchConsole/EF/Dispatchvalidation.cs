using System;
using System.Collections.Generic;

namespace SND.SMP.DispatchConsole.EF;

public partial class Dispatchvalidation
{
    public string DispatchNo { get; set; }

    public string CustomerCode { get; set; }

    public string PostalCode { get; set; }

    public string ServiceCode { get; set; }

    public string ProductCode { get; set; }

    public DateTime? DateStarted { get; set; }

    public DateTime? DateCompleted { get; set; }

    public double? TookInSec { get; set; }

    public string FilePath { get; set; }

    public ulong? IsValid { get; set; }

    public string Status { get; set; }

    public int? ValidationProgress { get; set; }

    public ulong? IsFundLack { get; set; }
}

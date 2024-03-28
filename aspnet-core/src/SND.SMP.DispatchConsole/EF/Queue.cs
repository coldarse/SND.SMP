using System;
using System.Collections.Generic;

namespace SND.SMP.DispatchConsole.EF;

public partial class Queue
{
    public uint Id { get; set; }

    public string EventType { get; set; }

    public string FilePath { get; set; }

    public ulong? DeleteFileOnSuccess { get; set; }

    public ulong? DeleteFileOnFailed { get; set; }

    public DateTime? DateCreated { get; set; }

    public string Status { get; set; }

    public double? TookInSec { get; set; }

    public string ErrorMsg { get; set; }

    public DateTime? DateStart { get; set; }

    public DateTime? DateEnd { get; set; }
}

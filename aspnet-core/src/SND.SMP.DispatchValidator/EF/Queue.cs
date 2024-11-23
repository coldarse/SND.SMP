using System;
using System.Collections.Generic;

namespace SND.SMP.DispatchValidator.EF;

public partial class Queue
{
    public uint Id { get; set; }

    public string EventType { get; set; }

    public string FilePath { get; set; }

    public ulong? DeleteFileOnSuccess { get; set; }

    public ulong? DeleteFileOnFailed { get; set; }

    public DateTime? DateCreated { get; set; }

    public string Status { get; set; }

    public double? TookInSec { get; set; } = 0;

    public string ErrorMsg { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }
}

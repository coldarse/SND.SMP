using System;
using System.Collections.Generic;
using SND.SMP.Bags;

public class GetPostCheck
{
    public string? CompanyName { get; set; }
    public string? DispatchNo { get; set; }
    public string? FlightTrucking { get; set; }
    public string? ServiceCode { get; set; }
    public DateOnly? ETA { get; set; }
    public DateTime? ATA { get; set; }
    public string CompanyCode { get; set; }
    public int? PreCheckNoOfBag { get; set; } = 0;
    public int? PostCheckNoOfBag { get; set; } = 0;
    public decimal? PreCheckWeight { get; set; } = 0;
    public decimal? PostCheckWeight { get; set; } = 0;
    public List<Bag> Bags { get; set; }
}
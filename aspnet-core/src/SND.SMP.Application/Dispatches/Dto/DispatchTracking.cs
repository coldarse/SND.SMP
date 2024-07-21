using System;
using System.Collections.Generic;

public class DispatchTracking
{
    public List<DispatchInfo> Dispatches { get; set; }
    public List<string> Countries { get; set; }
}

public class DispatchInfo
{
    public string Dispatch { get; set; }
    public int DispatchId { get; set; }
    public string DispatchDate { get; set; }
    public string PostalCode { get; set; }
    public int Status { get; set; }
    public string Customer { get; set; }
    public bool Open { get; set; }
    public List<DispatchCountry> DispatchCountries { get; set; }
}

public class DispatchCountry
{
    public string CountryCode { get; set; }
    public List<DispatchBag> DispatchBags { get; set; }
    public int BagCount { get; set; }
    public bool Open { get; set; }
    public bool Select { get; set; }
    public Stage Stages { get; set; }
}

public class DispatchBag
{
    public int BagId { get; set; }
    public string BagNo { get; set; }
    public int ItemCount { get; set; }
    public bool Select { get; set; }
    public bool Custom { get; set; }
    public Stage Stages { get; set; }
}

public class Stage
{
    public string DispatchNo { get; set; }
    public string CountryCode { get; set; }
    public string BagNo { get; set; } = "";
    public string Stage1Desc { get; set; } = "";
    public DateTime? Stage1DateTime { get; set; }
    public string Stage2Desc { get; set; } = "";
    public DateTime? Stage2DateTime { get; set; }
    public string Stage3Desc { get; set; } = "";
    public DateTime? Stage3DateTime { get; set; }
    public string Stage4Desc { get; set; } = "";
    public DateTime? Stage4DateTime { get; set; }
    public string Stage5Desc { get; set; } = "";
    public DateTime? Stage5DateTime { get; set; }
    public string Stage6Desc { get; set; } = "";
    public DateTime? Stage6DateTime { get; set; }
    public string Airport { get; set; } = "";
    public DateTime? AirportDateTime { get; set; }
    public string Stage7Desc { get; set; } = "";
    public DateTime? Stage7DateTime { get; set; }
}
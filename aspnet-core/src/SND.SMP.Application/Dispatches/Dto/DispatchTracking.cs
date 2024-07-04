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
    public DateOnly DispatchDate { get; set; }
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
}

public class DispatchBag
{
    public int BagId { get; set; }
    public string BagNo { get; set; }
    public int ItemCount { get; set; }
    public bool Select { get; set; }
    public bool Custom { get; set; }

}
using System;
using System.Collections.Generic;

public class DispatchInfoDto
{
    public string CustomerName { get; set; }
    public string CustomerCode { get; set; }
    public string PostalCode { get; set; }
    public string PostalDesc { get; set; }
    public DateOnly? DispatchDate { get; set; }
    public string DispatchNo { get; set; }
    public string ServiceCode { get; set; }
    public string ServiceDesc { get; set; }
    public string ProductCode { get; set; }
    public string ProductDesc { get; set; }
    public int? TotalBags { get; set; }
    public decimal? TotalWeight { get; set; }
    public int TotalCountry { get; set; }
    public string Status { get; set; }
    public string Path { get; set; }
    public List<string> Countries { get; set; }
}
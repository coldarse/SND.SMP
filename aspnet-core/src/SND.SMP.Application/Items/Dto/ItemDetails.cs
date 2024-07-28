using System;
using System.Collections.Generic;

public class ItemDetails
{
    public string TrackingNo { get; set; }
    public string DispatchNo { get; set; }
    public string BagNo { get; set; }
    public string DispatchDate { get; set; }
    public string Postal { get; set; }
    public string Service { get; set; }
    public string Product { get; set; }
    public string Country { get; set; }
    public decimal Weight { get; set; }
    public decimal Value { get; set; }
    public string Description { get; set; }
    public string ReferenceNo { get; set; }
    public string Recipient { get; set; }
    public string ContactNo { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public int Status { get; set; }
}

public class ItemInfo
{
    public ItemDetails itemDetails { get; set; }
    public List<TrackingDetails> trackingDetails { get; set; }
}

public class TrackingDetails
{
    public string trackingNo { get; set; }
    public string location { get; set; }
    public string description { get; set; }
    public string dateTime { get; set; }
}
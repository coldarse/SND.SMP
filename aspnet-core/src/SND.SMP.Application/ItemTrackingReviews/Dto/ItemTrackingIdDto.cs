using System;
using System.Collections.Generic;
using System.Data;

public class ItemTrackingIdDto
{
    public string TrackingNo { get; set; }
    public string DateCreated { get; set; }
    public string DateUsed { get; set; }
    public string DispatchNo { get; set; }
}

public class ItemTrackingWithPath
{
    public string ExcelPath { get; set; }
    public int ApplicationId { get; set; }
    public int ReviewId { get; set; }
    public long CustomerId { get; set; }
    public string CustomerCode { get; set; }
    public DateTime DateCreated { get; set; }
    public string ProductCode { get; set; }
    public List<ItemTrackingIdDto> ItemIds { get; set; }
}

public class ItemIds
{
    public List<ItemTrackingWithPath> ItemWithPath { get; set; }
    public string Path { get; set; }
    public int Count { get; set; }
}

public class UnusedItemIds
{
    public List<ItemTrackingWithPath> ItemWithPath { get; set; }
    public List<string> UnusedList { get; set; }
}

public class ItemIdPath
{
    public string ItemId { get; set; }
    public string Path { get; set; }
    public string DispatchNo { get; set; }
}

public class ItemIdPaths
{
    public List<ItemTrackingWithPath> ItemWithPath { get; set; }
    public List<ItemIdPath> Paths { get; set; }
    public int Count { get; set; }
}

public class ItemIdPathWithDatatables
{
    public List<ItemIdPath> ItemIdPaths{ get; set; }
    public List<DataTable> DataTablesByPath {get;set;}
}

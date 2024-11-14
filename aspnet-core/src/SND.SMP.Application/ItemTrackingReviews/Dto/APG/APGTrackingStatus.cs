using System;
using System.Collections.Generic;

public class TrackingStatus
{
    public string Location { get; set; }
    public string Datetime { get; set; }
    public string Description { get; set; }
}

public class ItemTrackingNo
{
    public string TrackingNo { get; set; }
    public List<TrackingStatus> Status { get; set; }
}

public class ItemsCollection
{
    public List<ItemTrackingNo> Items { get; set; }
    public List<string> Errors { get; set; }
}
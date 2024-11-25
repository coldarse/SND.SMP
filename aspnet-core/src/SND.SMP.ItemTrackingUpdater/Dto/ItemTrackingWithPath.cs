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

public class ItemTrackingIdDto
{
    public string TrackingNo { get; set; }
    public string DateCreated { get; set; }
    public string DateUsed { get; set; }
    public string DispatchNo { get; set; }
}
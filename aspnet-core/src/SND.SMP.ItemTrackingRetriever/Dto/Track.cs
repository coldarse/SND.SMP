public class InSATrack
{
    public string ItemBarCode { get; set; }
}

public class OutSATrack
{
    public bool IsSuccess { get; set; }
    public string ResultMessage { get; set; }
    public List<SAItemsWithStatus> ItemsWithStatus { get; set; }
}

public class SAItemsWithStatus
{
    public string EventId { get; set; }
    public string EventNameAR { get; set; }
    public string EventNameEN { get; set; }
    public string CreationDate { get; set; }
    public int ItemDeliveryStatusEnum { get; set; }
    public string OriginOfficeID { get; set; }
    public string OriginOfficeNameAR { get; set; }
    public string OriginOfficeNameEN { get; set; }
    public string DestinationOfficeID { get; set; }
    public string DestinationOfficeNameAR { get; set; }
    public string DestinationOfficeNameEN { get; set; }
    public string ItemBarcode { get; set; }
    public int NoDeliveryReasonID { get; set; }
    public string NoDeliveryReasonNameAR { get; set; }
    public string NoDeliveryReasonNameEN { get; set; }
    public string EventCategoryCode { get; set; }
    public string EventCategoryName_Ar { get; set; }
    public string EventCategoryName_En { get; set; }
}

public class TrackOutput
{
    public DateTime? DateTime { get; set; }
    public string TrackingNo { get; set; }
    public string Status { get; set; }
    public string Country { get; set; }
    public string EventType { get; set; }
    public string Remark { get; set; }
    public DateTime? FirstDate { get; set; }
    public string DaysDelivered { get; set; }
    public string DateTime2 { get; set; }
    public int Stage { get; set; }
    public bool IsCLAPI { get; set; }
    public override bool Equals(object x)
    {
        return ((TrackOutput)x).Status == Status && ((TrackOutput)x).DateTime == DateTime;
    }
    public override int GetHashCode()
    {
        return Status.GetHashCode();
    }
}

public class InSPLTrack
{
    public string Airwaybill { get; set; }
    public string Reference { get; set; }
    public string CustomerCode { get; set; }
    public string BranchCode { get; set; }
}

public class OutSPLTrack
{
    public string Airwaybill { get; set; }
    public string EventCode { get; set; }
    public string Event { get; set; }
    public string EventName { get; set; }
    public string Supplier { get; set; }
    public string UserName { get; set; }
    public string Notes { get; set; }
    public DateTime ActionDate { get; set; }
    public string EventCountry { get; set; }
    public string EventCity { get; set; }
    public string EventSubCode { get; set; }
    public string EventSubName { get; set; }
}
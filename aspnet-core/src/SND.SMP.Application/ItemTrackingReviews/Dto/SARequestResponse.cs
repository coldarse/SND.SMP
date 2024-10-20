using System.Collections.Generic;

public class InPost
{
    public string CRMAccountId { get; set; } = "521017871";
    public int PickupType { get; set; } = 1;
    public int RequestTypeId { get; set; } = 1;
    public string CustomerName { get; set; }
    public string CustomerMobileNumber { get; set; }
    public string SenderName { get; set; } = "Signature Mail International";
    public string SenderMobileNumber { get; set; } = "0126281039";
    public List<InPostItem> Items { get; set; } = [];
}

public class InPostItem
{
    public string ReferenceId { get; set; }
    public int PaymentType { get; set; } = 1;
    public decimal ContentPrice { get; set; }
    public string ContentDescription { get; set; }
    public decimal Weight { get; set; }
    public string BoxLength { get; set; } = null;
    public string BoxWidth { get; set; } = null;
    public string BoxHeight { get; set; } = null;
    public decimal ContentPriceVAT { get; set; } = 0m;
    public decimal DeliveryCost { get; set; } = 0m;
    public decimal DeliveryCostVAT { get; set; } = 0m;
    public decimal TotalAmount { get; set; } = 0m;
    public decimal CustomerVAT { get; set; } = 0m;
    public decimal SaudiPostVAT { get; set; } = 0m;
    public InAddressDetail SenderAddressDetail { get; set; }
    public InAddressDetail ReceiverAddressDetail { get; set; }
}

public class InAddressDetail
{
    public int AddressTypeID { get; set; }
    public string LocationId { get; set; }
    public string FinalOfficeID { get; set; }
    public string AddressLine1 { get; set; } = "";
    public string AddressLine2 { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public string UnitNo { get; set; } = "";
    public string BuildingNo { get; set; } = "";
    public string AdditionalNo { get; set; } = "";
}

public class OutPost
{
    public List<OutPostItem> Items { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
}

public class OutPostItem
{
    public string ReferenceId { get; set; }
    public int ItemStatus { get; set; }
    public string Message { get; set; }
    public string Barcode { get; set; }
    public string ItemPiecesResponse { get; set; }
}
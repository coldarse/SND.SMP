using System.Collections.Generic;
using System.ComponentModel;

public class SAManifest
{
    public string From { get; set; } = "HKG";
    public string To { get; set; } = "RUH";
    public string Flight { get; set; }
    public string Date { get; set; }
    public string TotalBags { get; set; }
    public string TotalWeight { get; set; }
    public List<SAManifestItem> Items { get; set; }
}

public class SAManifestItem
{
    public string MAWB { get; set; }
    [DisplayName("Saudi Post Label Number")]
    public string TrackingNo { get; set; }
    public string Date { get; set; }
    public string Consignor { get; set; } = "Shanghai Xingren Kemao Co., Ltd";
    public string Orig { get; set; } = "HKG";
    public string Consignee { get; set; } = "ESNAD EXPRESS";
    public string Dest { get; set; } = "RUH";
    public string Pcs { get; set; } = "1";
    public string Contact { get; set; }
    [DisplayName("Weight (Kgs)")]
    public decimal Weight { get; set; }
    public string Contents { get; set; }
    public decimal Value { get; set; }
    public string Currency { get; set; } = "SAR";
    public string Remarks { get; set; }
}
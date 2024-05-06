using System.ComponentModel;

public class GQManifest
{
    public string Barcode { get; set; }
    public decimal? Weight { get; set; }
    public string Attachments_Type { get; set; }
    public string Sender_Fname { get; set; }
    public string Sender_Lname { get; set; }
    public string Sender_Address { get; set; }
    public string Sender_City { get; set; }
    public string Sender_Province { get; set; }
    public string Sender_Zip { get; set; }
    public string Destination_Fname { get; set; }
    public string Destination_Lname { get; set; }
    public string Destination_Address { get; set; }
    public string Destination_City { get; set; }
    public string Destination_Province { get; set; }
    public string Destination_Zip { get; set; }
    public string Attachment_Description { get; set; }
    [DisplayName("中文品名")]
    public string ChineseProductName { get; set; }
    public int? Attachment_Quantity { get; set; }
    public decimal? Attachment_Weight { get; set; }
    public decimal? Attachment_Price_USD { get; set; }
    public string Attachment_Hs_Code { get; set; }
    public int? Attachment_Width { get; set; }
    public int? Attachment_Height { get; set; }
    public int? Attachment_Length { get; set; }
    public int? Bag_Number { get; set; }
    public string Impc_To_Code { get; set; }
    public decimal? Bag_Tare_Weight { get; set; }
    public decimal? Bag_Weight { get; set; }
    public string Dispatch_Sent_Date { get; set; }
    public string Logistic_Code { get; set; }
    public string Destination_Phone { get; set; }
    public string IOSS { get; set; }
}
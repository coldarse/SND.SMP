using System.ComponentModel;

public class DOManifest
{
    public string MAWB { get; set; }
    [DisplayName("bag_number")]
    public string BagNo { get; set; }
    public string ETD { get; set; }
    public string ETA { get; set; }
    [DisplayName("order_number")]
    public string OrderNo { get; set; }
    [DisplayName("tracking_number")]
    public string TrackingNo { get; set; }
    public string Origin { get; set; }
    public string Destination { get; set; }
    [DisplayName("consignee account #")]
    public string ConsigneeAccNo { get; set; }
    public string Consignee { get; set; }
    [DisplayName("consignee_address1")]
    public string ConsigneeAddress1 { get; set; }
    [DisplayName("consignee_address2")]
    public string ConsigneeAddress2 { get; set; }
    [DisplayName("consignee_address3")]
    public string ConsigneeAddress3 { get; set; }
    [DisplayName("consignee_neighbourhood")]
    public string ConsigneeNeighbourhood { get; set; }
    [DisplayName("consignee_city")]
    public string ConsigneeCity { get; set; }
    [DisplayName("consignee_state")]
    public string ConsigneeState { get; set; }
    [DisplayName("consignee_zip")]
    public string ConsigneeZip { get; set; }
    [DisplayName("consignee_country")]
    public string ConsigneeCountry { get; set; }
    [DisplayName("consignee_email")]
    public string ConsigneeEmail { get; set; }
    [DisplayName("consignee_phone")]
    public string ConsigneePhone { get; set; }
    [DisplayName("consignee_mobile")]
    public string ConsigneeMobile { get; set; }
    [DisplayName("consignee_taxid")]
    public string ConsigneeTaxId { get; set; }
    public int Pieces { get; set; }
    public decimal Gweight { get; set; }
    public string Cweight { get; set; }
    [DisplayName("weight_type")]
    public string WeightType { get; set; }
    public int Height { get; set; }
    public int Length { get; set; }
    public int Width { get; set; }
    public string Commodity { get; set; }
    public decimal Value { get; set; }
    public string Freight { get; set; }
    public string Currency { get; set; }
    [DisplayName("service_type")]
    public string ServiceType { get; set; }
    [DisplayName("service_level")]
    public string ServiceLevel { get; set; }
    [DisplayName("shipper account #")]
    public string ShipperAccNo { get; set; }
    [DisplayName("shipper name")]
    public string ShipperName { get; set; }
    [DisplayName("shipper_address1")]
    public string ShipperAddress1 { get; set; }
    [DisplayName("shipper_address2")]
    public string ShipperAddress2 { get; set; }
    [DisplayName("shipper_city")]
    public string ShipperCity { get; set; }
    [DisplayName("shipper_state")]
    public string ShipperState { get; set; }
    [DisplayName("shipper_zip")]
    public string ShipperZip { get; set; }
    [DisplayName("shipper_country")]
    public string ShipperCountry { get; set; }
    [DisplayName("shipper_email")]
    public string ShipperEmail { get; set; }
    [DisplayName("shipper_phone")]
    public string ShipperPhone { get; set; }
}

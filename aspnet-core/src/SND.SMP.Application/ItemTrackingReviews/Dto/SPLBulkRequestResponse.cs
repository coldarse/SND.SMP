using System;
using System.Collections.Generic;

public class ConsigneeContact
{
    public string PersonName { get; set; }
    public string CompanyName { get; set; }
    public string PhoneNumber1 { get; set; }
    public string PhoneNumber2 { get; set; }
    public string CellPhone { get; set; }
    public string EmailAddress { get; set; }
    public string Type { get; set; }
    public string CivilId { get; set; }
}

public class ConsigneeAddress
{
    public string CountryCode { get; set; }
    public string City { get; set; }
    public string District { get; set; }
    public string Line1 { get; set; }
    public string Line2 { get; set; }
    public string Line3 { get; set; }
    public string PostCode { get; set; }
    public string Longitude { get; set; }
    public string Latitude { get; set; }
    public string LocationCode1 { get; set; }
    public string LocationCode2 { get; set; }
    public string LocationCode3 { get; set; }
    public string ShortAddress { get; set; }
}

public class Consignee
{
    public ConsigneeContact ConsigneeContact { get; set; }
    public ConsigneeAddress ConsigneeAddress { get; set; }
}

public class ShipperContact
{
    public string PersonName { get; set; }
    public string CompanyName { get; set; }
    public string PhoneNumber1 { get; set; }
    public string PhoneNumber2 { get; set; }
    public string CellPhone { get; set; }
    public string EmailAddress { get; set; }
    public string Type { get; set; }
}

public class ShipperAddress
{
    public string CountryCode { get; set; }
    public string City { get; set; }
    public string Line1 { get; set; }
    public string Line2 { get; set; }
    public string Line3 { get; set; }
    public string PostCode { get; set; }
    public string Longitude { get; set; }
    public string Latitude { get; set; }
    public string LocationCode1 { get; set; }
    public string LocationCode2 { get; set; }
    public string LocationCode3 { get; set; }
}

public class ShipperDetails
{
    public ShipperAddress ShipperAddress { get; set; }
    public ShipperContact ShipperContact { get; set; }
}

public class Weight
{
    public int Unit { get; set; }
    public decimal Value { get; set; }
}

public class CustomsValue
{
    public string CurrencyCode { get; set; }
    public decimal Value { get; set; }
}

public class ShipmentItem
{
    public int Quantity { get; set; }
    public Weight Weight { get; set; }
    public CustomsValue CustomsValue { get; set; }
    public string Comments { get; set; }
    public string Reference { get; set; }
    public string CommodityCode { get; set; }
    public string GoodsDescription { get; set; }
    public string CountryOfOrigin { get; set; }
    public string PackageType { get; set; }
    public bool ContainsDangerousGoods { get; set; }
}

public class ShipmentWeight
{
    public int Value { get; set; }
    public int WeightUnit { get; set; }
    public int Length { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int DimensionUnit { get; set; }
}

public class Reference
{
    public string ShipperReference1 { get; set; }
    public string ShipperNote1 { get; set; }
}

public class SPLRequest
{
    public string CustomerCode { get; set; }
    public string BranchCode { get; set; }
    public string AirwaybillNumber { get; set; }
    public DateTime ShippingDateTime { get; set; }
    public DateTime DueDate { get; set; }
    public string DescriptionOfGoods { get; set; }
    public string ForeignHAWB { get; set; }
    public string NumberOfPieces { get; set; }
    public int Cod { get; set; }
    public int CustomsDeclaredValue { get; set; }
    public string CustomsDeclaredValueCurrency { get; set; }
    public string CodCurrnecy { get; set; }
    public string ProductType { get; set; }
    public string DutyHandling { get; set; }
    public string SupplierCode { get; set; }
    public string LabelFormat { get; set; }
    public string LabelSize { get; set; }
    public Consignee Consignee { get; set; }
    public ShipperDetails Shipper { get; set; }
    public List<ShipmentItem> Items { get; set; }
    public ShipmentWeight ShipmentWeight { get; set; }
    public Reference Reference { get; set; }
    public bool IncludeLabel { get; set; }
    public bool IncludeOfficeDetails { get; set; }
}
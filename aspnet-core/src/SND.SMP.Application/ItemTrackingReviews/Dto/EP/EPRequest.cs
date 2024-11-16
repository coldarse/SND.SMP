using System;
using System.Collections.Generic;

public class EPRequest
{
    public Weight Weight { get; set; }
    public Sender Shipper { get; set; }
    public Consignee Consignee { get; set; }
    public Dimensions Dimensions { get; set; }
    public Account Account { get; set; }
    public string ProductCode { get; set; }
    public string ServiceType { get; set; }
    public string PrintType { get; set; }
    public bool SendMailToSender { get; set; }
    public bool SendMailToReceiver { get; set; }
    public bool IsInsured { get; set; }
    public List<object> CustomsDeclarations { get; set; }
    public DeclaredValue DeclaredValue { get; set; }
    public int NumberOfPieces { get; set; }
    public string ReferenceNumber1 { get; set; }
    public string ReferenceNumber2 { get; set; }
    public string ReferenceNumber3 { get; set; }
    public string ReferenceNumber4 { get; set; }
    public string SpecialNotes { get; set; }
    public string Remarks { get; set; }
    public string TransportMode { get; set; }
    public string DeliveredDuty { get; set; }
}

public class Weight
{
    public decimal Value { get; set; }
    public string Unit { get; set; }
}

public class Sender
{
    public Contact Contact { get; set; }
    public Address Address { get; set; }
    public string ReferenceNo1 { get; set; }
    public string ReferenceNo2 { get; set; }
}

public class Consignee
{
    public Contact Contact { get; set; }
    public Address Address { get; set; }
    public string ReferenceNo1 { get; set; }
    public string ReferenceNo2 { get; set; }
}

public class Contact
{
    public string Name { get; set; }
    public string MobileNumber { get; set; }
    public string PhoneNumber { get; set; }
    public string EmailAddress { get; set; }
    public string CompanyName { get; set; }
}

public class Address
{
    public string Line1 { get; set; }
    public string RegionCode { get; set; }
    public string City { get; set; }
    public string CityCode { get; set; }
    public string CountryCode { get; set; }
    public string ZipCode { get; set; }
    public Point Point { get; set; }
    public string CountryName { get; set; }
}

public class Point
{
    public string Latitude { get; set; }
    public string Longitude { get; set; }
}

public class Dimensions
{
    public decimal Length { get; set; }
    public decimal Height { get; set; }
    public decimal Width { get; set; }
    public string Unit { get; set; }
}

public class Account
{
    public int Number { get; set; }
}

public class DeclaredValue
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}
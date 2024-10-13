// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using System.Collections.Generic;

public class Commodity
{
    public string description { get; set; }
    public decimal value { get; set; }
    public decimal weight { get; set; }
    public string hsTariffNumber { get; set; }
    public int quantity { get; set; }
    public string countryOfGoods { get; set; }
}

public class Package
{
    public string preAlertCode { get; set; }
    public string stamp { get; set; }
    public string senderCode { get; set; }
    public string senderName { get; set; }
    public string senderIdentification { get; set; }
    public string senderCountry { get; set; }
    public string receiverName { get; set; }
    public string receiverIdentification { get; set; }
    public string receiverAddress1 { get; set; }
    public string receiverAddress2 { get; set; }
    public string receiverAddress3 { get; set; }
    public string receiverNeighborhood { get; set; }
    public string receiverZipCode { get; set; }
    public string receiverCity { get; set; }
    public string receiverState { get; set; }
    public string receiverCountry { get; set; }
    public string receiverPhoneNumber { get; set; }
    public string receiverEmail { get; set; }
    public string receiverTaxId { get; set; }
    public List<Commodity> commodities { get; set; }
    public int division { get; set; }
    public int serviceValue { get; set; }
    public int serviceOptValue { get; set; }
    public int dimensionTypeValue { get; set; }
    public int weightTypeValue { get; set; }
    public string senderIOSS { get; set; }
    public int mailType { get; set; }
}

public class APGRequest
{
    public List<Package> packages { get; set; }
}


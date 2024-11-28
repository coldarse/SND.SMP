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
    public string senderAddress1 { get; set; }
    public string senderAddress2 { get; set; }
    public string senderAddress3 { get; set; }
    public string senderNeighborhood { get; set; }
    public string senderZipCode { get; set; }
    public string senderCity { get; set; }
    public string senderState { get; set; }
    public string senderCountry { get; set; }
    public string senderPhoneNumber { get; set; }
    public string senderEmail { get; set; }
    public string receiverCode { get; set; }
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
    public int weight { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int length { get; set; }
    public List<Commodity> commodities { get; set; }
    public string mawb { get; set; }
    public int division { get; set; }
    public int postalCharges { get; set; }
    //public string license { get; set; }
    //public string certificate { get; set; }
    //public string invoice { get; set; }
    public int serviceValue { get; set; }
    public int serviceOptValue { get; set; }
    public int dimensionTypeValue { get; set; }
    public int weightTypeValue { get; set; }
    public string officeCode { get; set; }
    public string originWebsite { get; set; }
    public int mailType { get; set; }
    public string senderIOSS { get; set; }
}

public class APGRequest
{
    public List<Package> packages { get; set; }
}


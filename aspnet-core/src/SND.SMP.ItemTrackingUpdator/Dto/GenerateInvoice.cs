using System.Collections.Generic;

public class GenerateInvoice
{
    public string Customer { get; set; }
    public string InvoiceNo { get; set; }
    public string InvoiceDate { get; set; }
    public List<string> Dispatches { get; set; }
    public string BillTo { get; set; }
    public List<ExtraCharge> ExtraCharges { get; set; }
    public int GenerateBy { get; set; }
}

public class ExtraCharge
{
    public string Description { get; set; }
    public decimal Weight { get; set; }
    public string Country { get; set; }
    public decimal RatePerKG { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}

public class InvoiceInfo
{
    public string Customer { get; set; }
    public string InvoiceNo { get; set; }
    public string InvoiceDate { get; set; }
    public string BillTo { get; set; }
    public List<string> Dispatches { get; set; }
    public List<ExtraCharge> ExtraCharges { get; set; }
    public List<ItemsByCurrency> CurrencyItem { get; set; }
}


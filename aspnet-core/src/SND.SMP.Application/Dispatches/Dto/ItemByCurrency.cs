using System.Collections.Generic;


public class SimplifiedItem
{
    public string DispatchNo { get; set; }
    public decimal Weight { get; set; }
    public string Country { get; set; }
    public string Identifier { get; set; }
    public decimal Rate { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public string ProductCode { get; set; }
    public string Currency { get; set; }
}

public class InvoiceDispatches
{
    public List<string> Dispatches { get; set; }
    public int GenerateBy { get; set; }
}

public class ItemWrapper
{
    public List<SimplifiedItem> DispatchItems { get; set; }
    public List<SimplifiedItem> SurchargeItems { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalAmountWithSurcharge { get; set; }
}
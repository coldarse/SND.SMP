using SND.SMP.DispatchConsole.EF;

public class ItemsByCurrency
{
    public string Currency { get; set; }
    public List<SimplifiedItem> Items { get; set; }
    public decimal TotalAmount { get; set; }
}

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
}
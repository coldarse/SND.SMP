using System.ComponentModel;

public class GQBag
{
    [DisplayName("Running No.")]
    public int RunningNo { get; set; }
    [DisplayName("Bag No.")]
    public string BagNo { get; set; }
    [DisplayName("Destination")]
    public string Destination { get; set; }
    [DisplayName("Quantity")]
    public int Qty { get; set; }
    [DisplayName("Weight")]
    public decimal Weight { get; set; }
    [DisplayName("Dispatch Date")]
    public string DispatchDate { get; set; }
}
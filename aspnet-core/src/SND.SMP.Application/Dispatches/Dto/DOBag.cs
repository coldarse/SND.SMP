using System.ComponentModel;

public class DOBag
{
    [DisplayName("No.")]
    public int RunningNo { get; set; }
    [DisplayName("Bag Number")]
    public string BagNo { get; set; }
    [DisplayName("Destination")]
    public string Destination { get; set; }
    [DisplayName("No. of items")]
    public int Qty { get; set; }
    [DisplayName("Total Weight (KG)")]
    public decimal Weight { get; set; }
    [DisplayName("Date")]
    public string DispatchDate { get; set; }
}


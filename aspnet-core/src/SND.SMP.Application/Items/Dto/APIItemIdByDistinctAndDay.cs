public class APIItemIdByDistinctAndDay
{
    public int TotalItems_Uploaded { get; set; }
    public int TotalItems_Pending { get; set; }
    public int TotalItems_Unregistered { get; set; }
    public decimal TotalWeight_Uploaded { get; set; }
    public decimal TotalWeight_Pending { get; set; }
    public decimal TotalWeight_Unregistered { get; set; }
    public decimal AverageValue_Uploaded { get; set; }
    public decimal AverageValue_Pending { get; set; }
    public decimal AverageValue_Unregistered { get; set; }
    public string Date { get; set; }
}
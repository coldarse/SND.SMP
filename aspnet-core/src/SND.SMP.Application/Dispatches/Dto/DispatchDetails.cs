using System;
using System.Collections.Generic;


public class CustomerDispatchDetails
{
    public List<DispatchDetails> Details { get; set; }
    public String Address { get; set; }
}

public class DispatchDetails
{
    public DateOnly Date { get; set; }
    public string Name { get; set; } = "";
    public decimal Weight { get; set; } = 0.000m;
    public decimal Credit { get; set; } = 0;
    public decimal Debit { get; set; } = 0;
    public int ItemCount { get; set; } = 0;
    public bool Selected { get; set; } = false;
}
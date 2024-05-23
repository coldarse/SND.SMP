using System.Collections.Generic;
using SND.SMP.CustomerPostals;

public class GroupedCustomerPostal
{
    public long CustomerId { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public List<CustomerPostal> CustomerPostal { get; set; }
}
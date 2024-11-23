using System;
using System.Collections.Generic;

namespace SND.SMP.DispatchValidator.EF;

public partial class Customer
{
    // public string CustomerCode { get; set; }
    public string Code { get; set; }

    public string CompanyName { get; set; }

    public virtual ICollection<Customercurrency> Customercurrencies { get; set; } = new List<Customercurrency>();

    public virtual ICollection<Customerpostal> Customerpostals { get; set; } = new List<Customerpostal>();

    public virtual ICollection<Customertransaction> Customertransactions { get; set; } = new List<Customertransaction>();
}

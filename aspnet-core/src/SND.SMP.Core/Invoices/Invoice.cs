using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.Invoices
{

    public class Invoice : Entity<int>
    {
        public DateTime DateTime { get; set; }
        public string InvoiceNo { get; set; }
        public string Customer { get; set; }
    }
}

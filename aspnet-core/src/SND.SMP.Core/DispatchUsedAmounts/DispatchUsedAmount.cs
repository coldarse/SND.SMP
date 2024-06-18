using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.DispatchUsedAmounts
{

    public class DispatchUsedAmount : Entity<int>
    {
        public string CustomerCode { get; set; }
        public string Wallet { get; set; }
        public decimal Amount { get; set; }
        public string DispatchNo { get; set; }
        public DateTime DateTime { get; set; }
        public string Description { get; set; }
    }
}

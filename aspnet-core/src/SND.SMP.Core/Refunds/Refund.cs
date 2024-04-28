using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.Refunds
{

    public class Refund : Entity<int>
    {
        public int UserId { get; set; }
        public string ReferenceNo { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Weight { get; set; }
    }
}

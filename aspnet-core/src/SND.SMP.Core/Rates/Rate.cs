using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.Rates
{

    public class Rate : Entity<int>
    {
        public string CardName { get; set; }
        public long Count { get; set; }
        public string Service { get; set; }
    }
}

using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.CustomerPostals
{

    public class CustomerPostal : Entity<long>
    {
        public string Postal { get; set; }
        public int Rate { get; set; }
        public long AccountNo { get; set; }
    }
}

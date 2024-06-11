using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.Airports
{

    public class Airport : Entity<int>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Country { get; set; }
    }
}

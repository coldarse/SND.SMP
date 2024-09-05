using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.RateZones
{

    public class RateZone : Entity<long>
    {
        public string Zone { get; set; }
        public string State { get; set; }
        public string City { get; set; }
    }
}

using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.Currencies
{

    public class Currency : Entity<long>
    {
        public string Abbr { get; set; }
        public string Description { get; set; }
    }
}

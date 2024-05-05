using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.IMPCS
{

    public class IMPC : Entity<int>
    {
        public string Type { get; set; }
        public string CountryCode { get; set; }
        public string AirportCode { get; set; }
        public string IMPCCode { get; set; }
        public string LogisticCode { get; set; }
    }
}

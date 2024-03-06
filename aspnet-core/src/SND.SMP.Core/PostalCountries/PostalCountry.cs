using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.PostalCountries
{

    public class PostalCountry : Entity<long>
    {
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
    }
}

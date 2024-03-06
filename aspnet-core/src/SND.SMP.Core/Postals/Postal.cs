using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.Postals
{

    public class Postal : Entity<long>
    {
        public string PostalCode { get; set; }
        public string PostalDesc { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceDesc { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public decimal ItemTopUpValue { get; set; }
    }
}

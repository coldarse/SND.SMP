using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.SAParams
{

    public class SAParam : Entity<long>
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string PostOfficeId { get; set; }
        public string CityId { get; set; }
        public string FinalOfficeId { get; set; }
        public int Seq { get; set; }
    }
}

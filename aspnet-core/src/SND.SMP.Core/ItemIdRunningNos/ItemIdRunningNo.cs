using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.ItemIdRunningNos
{

    public class ItemIdRunningNo : Entity<long>
    {
        public string Customer { get; set; }
        public string Prefix { get; set; }
        public string PrefixNo { get; set; }
        public string Suffix { get; set; }
        public int RunningNo { get; set; }
    }
}

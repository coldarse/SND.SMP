using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.EWalletTypes
{

    public class EWalletType : Entity<long>
    {
        public string Type { get; set; }
    }
}

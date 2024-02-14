using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.Wallets
{

    public class Wallet : Entity
    {
        [Key]
        [Column(Order = 0)]
        public long Customer { get; set; }
        [Key]
        [Column(Order = 1)]
        public long EWalletType { get; set; }
        [Key]
        [Column(Order = 2)]
        public long Currency { get; set; }
    }
}

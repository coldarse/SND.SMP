using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.Bags
{

    public class Bag : Entity<int>
    {
        public string BagNo                          { get; set; }
        public int? DispatchId                     { get; set; } = 0;
        public string CountryCode                    { get; set; }
        public decimal? WeightPre                      { get; set; } = 0;
        public decimal? WeightPost                     { get; set; } = 0;
        public int? ItemCountPre                   { get; set; } = 0;
        public int? ItemCountPost                  { get; set; } = 0;
        public decimal? WeightVariance                 { get; set; } = 0;
        public string CN35No                         { get; set; }
        public decimal? UnderAmount                    { get; set; } = 0;
    }
}

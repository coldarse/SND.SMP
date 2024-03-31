using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.Bags
{

    public class PagedBagResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string BagNo                          { get; set; }
        public int? DispatchId                     { get; set; }
        public string CountryCode                    { get; set; }
        public decimal WeightPre                      { get; set; }
        public decimal WeightPost                     { get; set; }
        public int? ItemCountPre                   { get; set; }
        public int? ItemCountPost                  { get; set; }
        public decimal WeightVariance                 { get; set; }
        public string CN35No                         { get; set; }
        public decimal UnderAmount                    { get; set; }
    }
}

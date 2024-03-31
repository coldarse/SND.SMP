using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.ItemMins.Dto
{

    [AutoMap(typeof(ItemMin))]
    public class ItemMinDto : EntityDto<string>
    {
        public string ExtID                          { get; set; }
        public int? DispatchID                     { get; set; }
        public int? BagID                          { get; set; }
        public DateOnly DispatchDate                   { get; set; }
        public int? Month                          { get; set; }
        public string CountryCode                    { get; set; }
        public decimal Weight                         { get; set; }
        public decimal ItemValue                      { get; set; }
        public string RecpName                       { get; set; }
        public string ItemDesc                       { get; set; }
        public string Address                        { get; set; }
        public string City                           { get; set; }
        public string TelNo                          { get; set; }
        public int? DeliveredInDays                { get; set; }
        public bool? IsDelivered                    { get; set; }
        public int? Status                         { get; set; }
    }
}

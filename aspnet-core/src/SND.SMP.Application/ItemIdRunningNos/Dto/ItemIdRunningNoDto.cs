using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.ItemIdRunningNos.Dto
{

    [AutoMap(typeof(ItemIdRunningNo))]
    public class ItemIdRunningNoDto : EntityDto<long>
    {
        public string Prefix { get; set; }
        public string PrefixNo { get; set; }
        public string Suffix { get; set; }
        public int? RunningNo { get; set; }
    }
}

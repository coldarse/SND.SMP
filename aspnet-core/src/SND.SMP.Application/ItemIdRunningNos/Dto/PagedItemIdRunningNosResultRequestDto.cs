using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.ItemIdRunningNos
{

    public class PagedItemIdRunningNoResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string Prefix { get; set; }
        public string PrefixNo { get; set; }
        public string Suffix { get; set; }
        public int? RunningNo { get; set; }
    }
}

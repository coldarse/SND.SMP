using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.SAParams
{

    public class PagedSAParamResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string PostOfficeId { get; set; }
        public string CityId { get; set; }
        public string FinalOfficeId { get; set; }
        public int? Seq { get; set; }
    }
}

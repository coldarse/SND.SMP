using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.APIRequestResponses
{

    public class PagedAPIRequestResponseResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string URL { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public DateTime? RequestDateTime { get; set; }
        public DateTime? ResponseDateTime { get; set; }
        public int? Duration { get; set; }
    }
}

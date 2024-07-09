using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.APIRequestResponses.Dto
{

    [AutoMap(typeof(APIRequestResponse))]
    public class APIRequestResponseDto : EntityDto<long>
    {
        public string URL { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public DateTime? RequestDateTime { get; set; }
        public DateTime? ResponseDateTime { get; set; }
        public int? Duration { get; set; }
    }
}

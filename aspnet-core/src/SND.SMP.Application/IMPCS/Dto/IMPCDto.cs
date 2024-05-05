using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.IMPCS.Dto
{

    [AutoMap(typeof(IMPC))]
    public class IMPCDto : EntityDto<int>
    {
        public string Type { get; set; }
        public string CountryCode { get; set; }
        public string AirportCode { get; set; }
        public string IMPCCode { get; set; }
        public string LogisticCode { get; set; }
    }
}

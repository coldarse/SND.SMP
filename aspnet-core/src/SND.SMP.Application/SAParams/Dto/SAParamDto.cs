using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.SAParams.Dto
{

    [AutoMap(typeof(SAParam))]
    public class SAParamDto : EntityDto<long>
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string PostOfficeId { get; set; }
        public string CityId { get; set; }
        public string FinalOfficeId { get; set; }
        public int? Seq { get; set; }
    }
}

using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.SA_Params.Dto
{

    [AutoMap(typeof(SA_Params))]
    public class SA_ParamsDto : EntityDto<long>
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

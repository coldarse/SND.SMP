using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.Airports.Dto
{

    [AutoMap(typeof(Airport))]
    public class AirportDto : EntityDto<int>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Country { get; set; }
    }
}

using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.RateZones.Dto
{

    [AutoMap(typeof(RateZone))]
    public class RateZoneDto : EntityDto<long>
    {
        public string Zone { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
    }
}

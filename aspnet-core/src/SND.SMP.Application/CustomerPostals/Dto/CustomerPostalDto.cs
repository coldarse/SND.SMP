using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.CustomerPostals.Dto
{

    [AutoMap(typeof(CustomerPostal))]
    public class CustomerPostalDto : EntityDto<long>
    {
        public string Postal { get; set; }
        public int Rate { get; set; }
        public long AccountNo { get; set; }
    }
}

using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.Rates.Dto
{

    [AutoMap(typeof(Rate))]
    public class RateDto : EntityDto<int>
    {
        public string CardName { get; set; }
        public long Count { get; set; }
    }
}

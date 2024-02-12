using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.Currencies.Dto
{

    [AutoMap(typeof(Currency))]
    public class CurrencyDto : EntityDto<long>
    {
        public string Abbr { get; set; }
        public string Description { get; set; }
    }
}

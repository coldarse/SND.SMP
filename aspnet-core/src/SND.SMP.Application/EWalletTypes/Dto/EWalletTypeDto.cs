using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.EWalletTypes.Dto
{

    [AutoMap(typeof(EWalletType))]
    public class EWalletTypeDto : EntityDto<long>
    {
        public string Type { get; set; }
    }
}

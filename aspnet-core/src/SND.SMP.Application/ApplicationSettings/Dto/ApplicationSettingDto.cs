using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.ApplicationSettings.Dto
{

    [AutoMap(typeof(ApplicationSetting))]
    public class ApplicationSettingDto : EntityDto<int>
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}

using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.ApplicationSettings
{

    public class PagedApplicationSettingResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}

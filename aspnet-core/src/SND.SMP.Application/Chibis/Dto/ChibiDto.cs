using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.Chibis.Dto
{

    [AutoMap(typeof(Chibi))]
    public class ChibiDto : EntityDto<long>
    {
        public string FileName { get; set; }
        public string UUID { get; set; }
        public string URL { get; set; }
        public string OriginalName { get; set; }
        public string GeneratedName { get; set; }
    }
}

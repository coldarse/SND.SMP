using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.Chibis
{

    public class PagedChibiResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string FileName { get; set; }
        public string UUID { get; set; }
        public string URL { get; set; }
        public string OriginalName { get; set; }
        public string GeneratedName { get; set; }
    }
}

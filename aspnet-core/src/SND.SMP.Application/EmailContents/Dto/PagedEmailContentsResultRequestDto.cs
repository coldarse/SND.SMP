using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.EmailContents
{

    public class PagedEmailContentResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}

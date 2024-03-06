using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.PostalOrgs
{

    public class PagedPostalOrgResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string Name { get; set; }
    }
}

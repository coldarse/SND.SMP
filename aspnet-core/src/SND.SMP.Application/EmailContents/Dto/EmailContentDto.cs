using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.EmailContents.Dto
{

    [AutoMap(typeof(EmailContent))]
    public class EmailContentDto : EntityDto<int>
    {
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}

using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.PostalOrgs.Dto
{

    [AutoMap(typeof(PostalOrg))]
    public class PostalOrgDto : EntityDto<string>
    {
        public string Name { get; set; }
    }
}

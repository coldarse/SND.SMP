using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using System;
using System.Linq;
using SND.SMP.PostalOrgs.Dto;

namespace SND.SMP.PostalOrgs
{
    public class PostalOrgAppService : AsyncCrudAppService<PostalOrg, PostalOrgDto, string, PagedPostalOrgResultRequestDto>
    {

        public PostalOrgAppService(IRepository<PostalOrg, string> repository) : base(repository)
        {
        }
        protected override IQueryable<PostalOrg> CreateFilteredQuery(PagedPostalOrgResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.Name.Contains(input.Keyword));
        }
    }
}

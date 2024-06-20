using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.EmailContents.Dto;

namespace SND.SMP.EmailContents
{
    public class EmailContentAppService : AsyncCrudAppService<EmailContent, EmailContentDto, int, PagedEmailContentResultRequestDto>
    {

        public EmailContentAppService(IRepository<EmailContent, int> repository) : base(repository)
        {
        }
        protected override IQueryable<EmailContent> CreateFilteredQuery(PagedEmailContentResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.Name.Contains(input.Keyword) ||
                    x.Subject.Contains(input.Keyword) ||
                    x.Content.Contains(input.Keyword));
        }
    }
}

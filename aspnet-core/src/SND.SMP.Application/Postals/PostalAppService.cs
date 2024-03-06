using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.Postals.Dto;

namespace SND.SMP.Postals
{
    public class PostalAppService : AsyncCrudAppService<Postal, PostalDto, long, PagedPostalResultRequestDto>
    {

        public PostalAppService(IRepository<Postal, long> repository) : base(repository)
        {
        }
        protected override IQueryable<Postal> CreateFilteredQuery(PagedPostalResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.PostalCode.Contains(input.Keyword) ||
                    x.PostalDesc.Contains(input.Keyword) ||
                    x.ServiceCode.Contains(input.Keyword) ||
                    x.ServiceDesc.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword) ||
                    x.ProductDesc.Contains(input.Keyword)).AsQueryable();
        }
    }
}

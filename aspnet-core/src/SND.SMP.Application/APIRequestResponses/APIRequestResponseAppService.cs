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
using SND.SMP.APIRequestResponses.Dto;

namespace SND.SMP.APIRequestResponses
{
    public class APIRequestResponseAppService : AsyncCrudAppService<APIRequestResponse, APIRequestResponseDto, long, PagedAPIRequestResponseResultRequestDto>
    {

        public APIRequestResponseAppService(IRepository<APIRequestResponse, long> repository) : base(repository)
        {
        }
        protected override IQueryable<APIRequestResponse> CreateFilteredQuery(PagedAPIRequestResponseResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.URL.Contains(input.Keyword) ||
                    x.RequestBody.Contains(input.Keyword) ||
                    x.ResponseBody.Contains(input.Keyword))
                .OrderByDescending(x => x.RequestDateTime);
        }
    }
}

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
using SND.SMP.IMPCS.Dto;

namespace SND.SMP.IMPCS
{
    public class IMPCAppService(IRepository<IMPC, int> repository) : AsyncCrudAppService<IMPC, IMPCDto, int, PagedIMPCResultRequestDto>(repository)
    {
        protected override IQueryable<IMPC> CreateFilteredQuery(PagedIMPCResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.Type.Contains(input.Keyword) ||
                    x.CountryCode.Contains(input.Keyword) ||
                    x.AirportCode.Contains(input.Keyword) ||
                    x.IMPCCode.Contains(input.Keyword) ||
                    x.LogisticCode.Contains(input.Keyword));
        }
    }
}

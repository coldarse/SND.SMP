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
using SND.SMP.SAParams.Dto;

namespace SND.SMP.SAParams
{
    public class SAParamAppService : AsyncCrudAppService<SAParam, SAParamDto, long, PagedSAParamResultRequestDto>
    {

        public SAParamAppService(IRepository<SAParam, long> repository) : base(repository)
        {
        }
        protected override IQueryable<SAParam> CreateFilteredQuery(PagedSAParamResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.Type.Contains(input.Keyword) ||
                    x.Name.Contains(input.Keyword) ||
                    x.Code.Contains(input.Keyword) ||
                    x.PostOfficeId.Contains(input.Keyword) ||
                    x.CityId.Contains(input.Keyword) ||
                    x.FinalOfficeId.Contains(input.Keyword));
        }
    }
}

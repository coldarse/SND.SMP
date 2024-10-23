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
using SND.SMP.SA_Params.Dto;

namespace SND.SMP.SA_Params
{
    public class SA_ParamsAppService : AsyncCrudAppService<SA_Params, SA_ParamsDto, long, PagedSA_ParamsResultRequestDto>
    {

        public SA_ParamsAppService(IRepository<SA_Params, long> repository) : base(repository)
        {
        }
        protected override IQueryable<SA_Params> CreateFilteredQuery(PagedSA_ParamsResultRequestDto input)
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

using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.Chibis.Dto;

namespace SND.SMP.Chibis
{
    public class ChibiAppService : AsyncCrudAppService<Chibi, ChibiDto, long, PagedChibiResultRequestDto>
    {

        public ChibiAppService(IRepository<Chibi, long> repository) : base(repository)
        {
        }
        protected override IQueryable<Chibi> CreateFilteredQuery(PagedChibiResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.FileName.Contains(input.Keyword) ||
                    x.UUID.Contains(input.Keyword) ||
                    x.URL.Contains(input.Keyword) ||
                    x.OriginalName.Contains(input.Keyword) ||
                    x.GeneratedName.Contains(input.Keyword)).AsQueryable();
        }
    }
}

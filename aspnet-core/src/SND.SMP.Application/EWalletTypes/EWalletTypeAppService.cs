using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.EWalletTypes.Dto;

namespace SND.SMP.EWalletTypes
{
    public class EWalletTypeAppService : AsyncCrudAppService<EWalletType, EWalletTypeDto, long, PagedEWalletTypeResultRequestDto>
    {

        public EWalletTypeAppService(IRepository<EWalletType, long> repository) : base(repository)
        {
        }
        protected override IQueryable<EWalletType> CreateFilteredQuery(PagedEWalletTypeResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.Type.Contains(input.Keyword)).AsQueryable();
        }

        public async Task<List<EWalletType>> GetEWalletTypes()
        {
            return await Repository.GetAllListAsync();
        }
    }
}

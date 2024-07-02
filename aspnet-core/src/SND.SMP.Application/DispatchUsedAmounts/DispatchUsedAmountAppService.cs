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
using SND.SMP.DispatchUsedAmounts.Dto;

namespace SND.SMP.DispatchUsedAmounts
{
    public class DispatchUsedAmountAppService : AsyncCrudAppService<DispatchUsedAmount, DispatchUsedAmountDto, int, PagedDispatchUsedAmountResultRequestDto>
    {

        public DispatchUsedAmountAppService(IRepository<DispatchUsedAmount, int> repository) : base(repository)
        {
        }
        protected override IQueryable<DispatchUsedAmount> CreateFilteredQuery(PagedDispatchUsedAmountResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.CustomerCode.Contains(input.Keyword) ||
                    x.Wallet.Contains(input.Keyword) ||
                    x.DispatchNo.Contains(input.Keyword) ||
                    x.Description.Contains(input.Keyword));
        }
    }
}

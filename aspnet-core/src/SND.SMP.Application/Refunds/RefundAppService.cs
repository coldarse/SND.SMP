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
using SND.SMP.Refunds.Dto;

namespace SND.SMP.Refunds
{
    public class RefundAppService : AsyncCrudAppService<Refund, RefundDto, int, PagedRefundResultRequestDto>
    {

        public RefundAppService(IRepository<Refund, int> repository) : base(repository)
        {
        }
        protected override IQueryable<Refund> CreateFilteredQuery(PagedRefundResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.ReferenceNo.Contains(input.Keyword) ||
                    x.Description.Contains(input.Keyword));
        }
    }
}

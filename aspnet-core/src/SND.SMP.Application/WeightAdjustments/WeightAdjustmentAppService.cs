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
using SND.SMP.WeightAdjustments.Dto;

namespace SND.SMP.WeightAdjustments
{
    public class WeightAdjustmentAppService : AsyncCrudAppService<WeightAdjustment, WeightAdjustmentDto, int, PagedWeightAdjustmentResultRequestDto>
    {

        public WeightAdjustmentAppService(IRepository<WeightAdjustment, int> repository) : base(repository)
        {
        }
        protected override IQueryable<WeightAdjustment> CreateFilteredQuery(PagedWeightAdjustmentResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.ReferenceNo.Contains(input.Keyword) ||
                    x.Description.Contains(input.Keyword));
        }
    }
}

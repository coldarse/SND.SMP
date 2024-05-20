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
using SND.SMP.ItemTrackingApplications.Dto;

namespace SND.SMP.ItemTrackingApplications
{
    public class ItemTrackingApplicationAppService : AsyncCrudAppService<ItemTrackingApplication, ItemTrackingApplicationDto, int, PagedItemTrackingApplicationResultRequestDto>
    {

        public ItemTrackingApplicationAppService(IRepository<ItemTrackingApplication, int> repository) : base(repository)
        {
        }
        protected override IQueryable<ItemTrackingApplication> CreateFilteredQuery(PagedItemTrackingApplicationResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.CustomerCode.Contains(input.Keyword) ||
                    x.PostalCode.Contains(input.Keyword) ||
                    x.PostalDesc.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword) ||
                    x.ProductDesc.Contains(input.Keyword) ||
                    x.Status.Contains(input.Keyword));
        }
    }
}

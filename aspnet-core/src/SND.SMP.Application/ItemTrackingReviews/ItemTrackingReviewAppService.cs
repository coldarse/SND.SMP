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
using SND.SMP.ItemTrackingReviews.Dto;

namespace SND.SMP.ItemTrackingReviews
{
    public class ItemTrackingReviewAppService : AsyncCrudAppService<ItemTrackingReview, ItemTrackingReviewDto, int, PagedItemTrackingReviewResultRequestDto>
    {

        public ItemTrackingReviewAppService(IRepository<ItemTrackingReview, int> repository) : base(repository)
        {
        }
        protected override IQueryable<ItemTrackingReview> CreateFilteredQuery(PagedItemTrackingReviewResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.CustomerCode.Contains(input.Keyword) ||
                    x.PostalCode.Contains(input.Keyword) ||
                    x.PostalDesc.Contains(input.Keyword) ||
                    x.Status.Contains(input.Keyword) ||
                    x.Prefix.Contains(input.Keyword) ||
                    x.PrefixNo.Contains(input.Keyword) ||
                    x.Suffix.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword));
        }
    }
}

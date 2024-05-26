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
using SND.SMP.ItemTrackingApplications;
using Abp.EntityFrameworkCore.Repositories;
using SND.SMP.ItemTrackings;

namespace SND.SMP.ItemTrackingReviews
{
    public class ItemTrackingReviewAppService(
        IRepository<ItemTrackingReview, int> repository, 
        IRepository<ItemTrackingApplication, int> itemTrackingApplicationRepository,
        IRepository<ItemTracking, int> itemTrackingRepository
    ) : AsyncCrudAppService<ItemTrackingReview, ItemTrackingReviewDto, int, PagedItemTrackingReviewResultRequestDto>(repository)
    {
        private readonly IRepository<ItemTrackingApplication, int> _itemTrackingApplicationRepository = itemTrackingApplicationRepository;
        private readonly IRepository<ItemTracking, int> _itemTrackingRepository = itemTrackingRepository;

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

        public override async Task<ItemTrackingReviewDto> CreateAsync(ItemTrackingReviewDto input)
        {
            CheckCreatePermission();

            var entity = MapToEntity(input);

            await Repository.InsertAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();

            var application = await _itemTrackingApplicationRepository.FirstOrDefaultAsync(x => x.Id.Equals(input.ApplicationId));
            application.Status = input.Status;
            await _itemTrackingApplicationRepository.UpdateAsync(application);
            await _itemTrackingApplicationRepository.GetDbContext().SaveChangesAsync();

            return MapToEntityDto(entity);
        }

        public async Task<bool> UndoReview(int applicationId)
        {
            var application = await _itemTrackingApplicationRepository.FirstOrDefaultAsync(x => x.Id.Equals(applicationId));
            application.Status = "Pending";
            await _itemTrackingApplicationRepository.UpdateAsync(application);

            var review = await Repository.FirstOrDefaultAsync(x => x.ApplicationId.Equals(applicationId));
            await Repository.DeleteAsync(review);

            return true;
        }

        public async Task<ReviewAmount> GetReviewAmount(int applicationId)
        {
            var review = await Repository.FirstOrDefaultAsync(x => x.ApplicationId.Equals(applicationId));

            var tracking = await _itemTrackingRepository.GetAllListAsync(x => x.ApplicationId.Equals(applicationId));

            return new ReviewAmount(){
                Issued = review is null ? "0" : review.TotalGiven.ToString(),
                Remaining = review is null ? "0" : (review.TotalGiven - tracking.Count).ToString(),
                Uploaded = tracking.Count.ToString(),
            };
        }
    }
}

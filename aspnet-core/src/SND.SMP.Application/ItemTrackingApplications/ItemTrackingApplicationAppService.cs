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
using SND.SMP.Postals;
using Abp.UI;

namespace SND.SMP.ItemTrackingApplications
{
    public class ItemTrackingApplicationAppService(
        IRepository<ItemTrackingApplication, int> repository,
        IRepository<Postal, long> postalRepository
    ) : AsyncCrudAppService<ItemTrackingApplication, ItemTrackingApplicationDto, int, PagedItemTrackingApplicationResultRequestDto>(repository)
    {

        private readonly IRepository<Postal, long> _postalRepository = postalRepository;

        protected override IQueryable<ItemTrackingApplication> CreateFilteredQuery(PagedItemTrackingApplicationResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.CustomerCode.Contains(input.Keyword) ||
                    x.PostalCode.Contains(input.Keyword) ||
                    x.PostalDesc.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword) ||
                    x.ProductDesc.Contains(input.Keyword) ||
                    x.Status.Contains(input.Keyword) ||
                    x.Range.Contains(input.Keyword))
                .WhereIf(!input.CustomerCode.IsNullOrWhiteSpace(), x => 
                    x.CustomerCode.Equals(input.CustomerCode));
        }

        public async Task<bool> CreateTrackingApplication(string CustomerCode, long CustomerId, int Total, string ProductCode, string ProductDesc, string PostalCode)
        {
            var postals = await _postalRepository.GetAllListAsync();

            var postal = 
                postals.FirstOrDefault(x => 
                                            x.PostalCode[..2].Equals(PostalCode[..2]) && 
                                            x.ProductCode.Equals(ProductCode)
                    ) 
                ?? throw new UserFriendlyException($"Postal Code {PostalCode} or Product Code {ProductCode} combination not found.");

            await Repository.InsertAsync(new ItemTrackingApplication()
            {
                CustomerCode = CustomerCode,
                CustomerId = CustomerId,
                Total = Total,
                ProductCode = ProductCode,
                ProductDesc = ProductDesc,
                PostalCode = PostalCode[..2],
                PostalDesc = postal.PostalDesc,
                DateCreated = DateTime.Now,
                Status = "Pending",
                Range = "",
                Path = ""
            }).ConfigureAwait(false);

            return true;
        }


    }
}

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
using SND.SMP.DispatchValidations.Dto;
using Abp.Application.Services.Dto;
using System.Linq.Dynamic.Core;
using Abp.AutoMapper;

namespace SND.SMP.DispatchValidations
{
    public class DispatchValidationAppService(IRepository<DispatchValidation, string> repository) : AsyncCrudAppService<DispatchValidation, DispatchValidationDto, string, PagedDispatchValidationResultRequestDto>(repository)
    {
        protected override IQueryable<DispatchValidation> CreateFilteredQuery(PagedDispatchValidationResultRequestDto input)
        {
            return input.isAdmin ?
                Repository.GetAllIncluding()
                    .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                        x.CustomerCode.Contains(input.Keyword) ||
                        x.DispatchNo.Contains(input.Keyword) ||
                        x.FilePath.Contains(input.Keyword) ||
                        x.PostalCode.Contains(input.Keyword) ||
                        x.ServiceCode.Contains(input.Keyword) ||
                        x.ProductCode.Contains(input.Keyword) ||
                        x.Status.Contains(input.Keyword))
                :
                Repository.GetAllIncluding()
                    .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                        x.CustomerCode.Contains(input.Keyword) ||
                        x.DispatchNo.Contains(input.Keyword) ||
                        x.FilePath.Contains(input.Keyword) ||
                        x.PostalCode.Contains(input.Keyword) ||
                        x.ServiceCode.Contains(input.Keyword) ||
                        x.ProductCode.Contains(input.Keyword) ||
                        x.Status.Contains(input.Keyword))
                    .Where(x => x.CustomerCode.Equals(input.CustomerCode));
        }

        private IQueryable<DispatchValidation> ApplySorting(IQueryable<DispatchValidation> query, PagedDispatchValidationResultRequestDto input)
        {
            //Try to sort query if available
            if (input is ISortedResultRequest sortInput)
            {
                if (!sortInput.Sorting.IsNullOrWhiteSpace())
                {
                    return query.OrderBy(sortInput.Sorting);
                }
            }

            //IQueryable.Task requires sorting, so we should sort if Take will be used.
            if (input is ILimitedResultRequest)
            {
                return query.OrderByDescending(e => e.ValidationProgress);
            }

            //No sorting
            return query;
        }

        private IQueryable<DispatchValidation> ApplyPaging(IQueryable<DispatchValidation> query, PagedDispatchValidationResultRequestDto input)
        {
            if ((object)input is IPagedResultRequest pagedResultRequest)
            {
                return query.PageBy(pagedResultRequest);
            }

            if ((object)input is ILimitedResultRequest limitedResultRequest)
            {
                return query.Take(limitedResultRequest.MaxResultCount);
            }

            return query;
        }

        public async Task<PagedResultDto<DispatchValidationDto>> GetDispatchValidation(PagedDispatchValidationResultRequestDto input)
        {
            CheckGetAllPermission();

            var query = CreateFilteredQuery(input);

            var totalCount = await AsyncQueryableExecuter.CountAsync(query);

            query = query.OrderByDescending(x => x.DateStarted);
            query = ApplyPaging(query, input);

            var entities = await AsyncQueryableExecuter.ToListAsync(query);

            return new PagedResultDto<DispatchValidationDto>(
                totalCount,
                [.. entities.Select(MapToEntityDto)]
            );
        }

        public async Task<List<DispatchValidation>> GetDashboardDispatchValidation(bool isAdmin, int top, string customer = null)
        {
            var validations = isAdmin ? await Repository.GetAllListAsync() : await Repository.GetAllListAsync(x => x.CustomerCode.Equals(customer));

            return [.. validations.OrderByDescending(x => x.DateStarted).Take(top)];
        }
    }
}

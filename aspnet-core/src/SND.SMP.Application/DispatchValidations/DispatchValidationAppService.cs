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

namespace SND.SMP.DispatchValidations
{
    public class DispatchValidationAppService : AsyncCrudAppService<DispatchValidation, DispatchValidationDto, string, PagedDispatchValidationResultRequestDto>
    {

        public DispatchValidationAppService(IRepository<DispatchValidation, string> repository) : base(repository)
        {
        }
        protected override IQueryable<DispatchValidation> CreateFilteredQuery(PagedDispatchValidationResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.CustomerCode                    .Contains(input.Keyword) ||
                    x.DispatchNo                      .Contains(input.Keyword) ||
                    x.FilePath                        .Contains(input.Keyword) ||
                    x.PostalCode                      .Contains(input.Keyword) ||
                    x.ServiceCode                     .Contains(input.Keyword) ||
                    x.ProductCode                     .Contains(input.Keyword) ||
                    x.Status                          .Contains(input.Keyword));
        }
    }
}

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
using SND.SMP.Invoices.Dto;

namespace SND.SMP.Invoices
{
    public class InvoiceAppService : AsyncCrudAppService<Invoice, InvoiceDto, int, PagedInvoiceResultRequestDto>
    {

        public InvoiceAppService(IRepository<Invoice, int> repository) : base(repository)
        {
        }
        protected override IQueryable<Invoice> CreateFilteredQuery(PagedInvoiceResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.InvoiceNo.Contains(input.Keyword) ||
                    x.Customer.Contains(input.Keyword));
        }

    }
}

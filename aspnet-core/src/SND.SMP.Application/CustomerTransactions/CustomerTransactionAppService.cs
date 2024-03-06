﻿using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using SND.SMP.Currencies;
using SND.SMP.CustomerTransactions.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SND.SMP.CustomerTransactions
{
    public class CustomerTransactionAppService : AsyncCrudAppService<CustomerTransaction, CustomerTransactionDto, long, PagedCustomerTransactionsResultRequestDto>
    {
        public CustomerTransactionAppService(IRepository<CustomerTransaction, long> repository) : base(repository)
        {
        }
        protected override IQueryable<CustomerTransaction> CreateFilteredQuery(PagedCustomerTransactionsResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.Wallet.Contains(input.Keyword) ||
                    x.Customer.Contains(input.Keyword) ||
                    x.PaymentMode.Contains(input.Keyword) ||
                    x.Currency.Contains(input.Keyword) ||
                    x.TransactionType.Contains(input.Keyword) ||
                    x.ReferenceNo.Contains(input.Keyword) ||
                    x.Description.Contains(input.Keyword));
    }

        public async Task<List<CustomerTransaction>> GetCustomerTransactions()
        {
            return await Repository.GetAllListAsync();
        }
    }
}

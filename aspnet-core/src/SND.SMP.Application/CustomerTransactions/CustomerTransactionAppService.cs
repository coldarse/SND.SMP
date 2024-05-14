using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using SND.SMP.CustomerTransactions.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SND.SMP.CustomerTransactions
{
    public class CustomerTransactionAppService(IRepository<CustomerTransaction, long> repository) : AsyncCrudAppService<CustomerTransaction, CustomerTransactionDto, long, PagedCustomerTransactionsResultRequestDto>(repository)
    {
        protected override IQueryable<CustomerTransaction> CreateFilteredQuery(PagedCustomerTransactionsResultRequestDto input)
        {
            return input.isAdmin ?
                Repository.GetAllIncluding()
                    .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                        x.Wallet.Contains(input.Keyword) ||
                        x.Customer.Contains(input.Keyword) ||
                        x.PaymentMode.Contains(input.Keyword) ||
                        x.Currency.Contains(input.Keyword) ||
                        x.TransactionType.Contains(input.Keyword) ||
                        x.ReferenceNo.Contains(input.Keyword) ||
                        x.Description.Contains(input.Keyword))
                :
                Repository.GetAllIncluding()
                    .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                        x.Wallet.Contains(input.Keyword) ||
                        x.PaymentMode.Contains(input.Keyword) ||
                        x.Currency.Contains(input.Keyword) ||
                        x.TransactionType.Contains(input.Keyword) ||
                        x.ReferenceNo.Contains(input.Keyword) ||
                        x.Description.Contains(input.Keyword))
                    .Where(x => x.Customer.Equals(input.Customer));
        }

        public async Task<List<CustomerTransaction>> GetCustomerTransactions()
        {
            return await Repository.GetAllListAsync();
        }

        public async Task<List<CustomerTransaction>> GetDashboardTransaction(bool isAdmin, int top, string customer = null)
        {
            var transactions = isAdmin ? await Repository.GetAllListAsync() : await Repository.GetAllListAsync(x => x.Customer.Equals(customer));

            return [.. transactions.OrderByDescending(x => x.TransactionDate).Take(top)];
        }
    }
}

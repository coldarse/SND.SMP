using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using SND.SMP.Currencies;
using SND.SMP.CustomerTransactions.Dto;
using SND.SMP.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SND.SMP.CustomerTransactions
{
    public class CustomerTransactionAppService(
        IRepository<CustomerTransaction, long> repository,
        IRepository<Wallet, string> walletRepository,
        IRepository<Currency, long> currencyRepository
    ) : AsyncCrudAppService<CustomerTransaction, CustomerTransactionDto, long, PagedCustomerTransactionsResultRequestDto>(repository)
    {
        private readonly IRepository<Wallet, string> _walletRepository = walletRepository;
        private readonly IRepository<Currency, long> _currencyRepository = currencyRepository;
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

        [HttpPost]
        public async Task<bool> DeleteAndCreditWallet(TransactionDetailAndAmount input)
        {
            var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Abbr.Equals(input.Currency));

            var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(input.Code) && x.Currency.Equals(currency.Id)) ?? throw new UserFriendlyException("Wallet Not Found");

            wallet.Balance += input.Amount;

            await _walletRepository.UpdateAsync(wallet).ConfigureAwait(false);

            await Repository.DeleteAsync(input.TransactionId).ConfigureAwait(false);

            return true;
        }
    }
}

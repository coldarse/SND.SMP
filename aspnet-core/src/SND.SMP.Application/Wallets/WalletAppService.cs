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
using SND.SMP.Wallets.Dto;
using Abp.Application.Services.Dto;
using Abp.UI;
using SND.SMP.Currencies;
using SND.SMP.EWalletTypes;
using SND.SMP.CustomerTransactions;
using System.Linq.Dynamic.Core;

namespace SND.SMP.Wallets
{
    public class WalletAppService(
        IRepository<Wallet, string> repository,
        IRepository<EWalletType, long> eWalletTypeRepository,
        IRepository<Currency, long> currencyRepository,
        IRepository<CustomerTransaction, long> customerTransactionRepository
        ) : AsyncCrudAppService<Wallet, WalletDto, string, PagedWalletResultRequestDto>(repository)
    {
        protected override IQueryable<Wallet> CreateFilteredQuery(PagedWalletResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.Customer.Contains(input.Keyword));
        }

        private IQueryable<WalletDetailDto> ApplySorting(IQueryable<WalletDetailDto> query, PagedWalletResultRequestDto input)
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
                return query.OrderByDescending(e => e.Id);
            }

            //No sorting
            return query;
        }

        private IQueryable<WalletDetailDto> ApplyPaging(IQueryable<WalletDetailDto> query, PagedWalletResultRequestDto input)
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

        public async Task<List<DetailedEWallet>> GetAllWalletsAsync(string code)
        {
            var wallet = await Repository.GetAllListAsync(x => x.Customer.Equals(code));

            var currency = await currencyRepository.GetAllListAsync();

            List<DetailedEWallet> wallets = [];
            foreach (Wallet w in wallet.ToList())
            {
                string curr = currency.FirstOrDefault(x => x.Id.Equals(w.Currency)).Abbr;
                wallets.Add(new DetailedEWallet()
                {
                    Currency = curr,
                    Balance = w.Balance,
                    EWalletType = w.EWalletType,
                });
            }

            return wallets;
        }

        public async Task<PagedResultDto<WalletDetailDto>> GetWalletDetail(PagedWalletResultRequestDto input)
        {
            CheckGetAllPermission();

            var query = CreateFilteredQuery(input);

            var eWalletTypes = await eWalletTypeRepository.GetAllListAsync();

            var currencies = await currencyRepository.GetAllListAsync();

            List<WalletDetailDto> detailed = [];

            foreach (var wallet in query.ToList())
            {
                var eWalletType = eWalletTypes.FirstOrDefault(x => x.Id.Equals(wallet.EWalletType));
                var currency = currencies.FirstOrDefault(x => x.Id.Equals(wallet.Currency));

                detailed.Add(new WalletDetailDto()
                {
                    Customer = wallet.Customer,
                    EWalletType = wallet.EWalletType,
                    EWalletTypeDesc = eWalletType.Type,
                    Currency = wallet.Currency,
                    CurrencyDesc = currency.Abbr,
                    Balance = wallet.Balance,
                    Id = wallet.Id
                });
            }

            var totalCount = detailed.Count;

            detailed = [.. ApplySorting(detailed.AsQueryable(), input)];
            detailed = [.. ApplyPaging(detailed.AsQueryable(), input)];


            return new PagedResultDto<WalletDetailDto>(
                totalCount,
                [.. detailed]
            );
        }

        public override async Task<WalletDto> CreateAsync(WalletDto input)
        {
            var wallets = await Repository.GetAllListAsync(x => x.Customer.Equals(input.Customer));
            /* If Wallet does not exist */
            if (wallets.Count.Equals(0)) return await base.CreateAsync(input);
            /* If Wallet exist */
            else
            {
                bool eWalletTypeExist = wallets.Any(x => x.EWalletType.Equals(input.EWalletType));
                /* If EwalletType does not exist */
                if (!eWalletTypeExist) return await base.CreateAsync(input);
                /* If EwalletType exist */
                else
                {
                    var customerWithEWalletType = wallets.Where(x => x.EWalletType.Equals(input.EWalletType));
                    bool currencyExist = customerWithEWalletType.Any(x => x.Currency.Equals(input.Currency));
                    /* If Currency does not exist */
                    if (!currencyExist) return await base.CreateAsync(input);
                    /* If Currency does not exist */
                    else throw new UserFriendlyException("You have already created a similar wallet. Please try again.");
                }
            }
        }

        public async Task<bool> TopUpEWallet(TopUpEWalletDto input)
        {
            var ewallet = await Repository.FirstOrDefaultAsync(x => x.Id.Equals(input.Id));

            if (ewallet is not null)
            {
                ewallet.Balance += input.Amount;
                var update = await Repository.UpdateAsync(ewallet);

                DateTime DateTimeUTC = DateTime.UtcNow;
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                DateTime cstDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTimeUTC, cstZone);

                var addTransaction = await customerTransactionRepository.InsertAsync(new CustomerTransaction()
                {
                    Wallet = ewallet.Id,
                    Customer = ewallet.Customer,
                    PaymentMode = input.EWalletType,
                    Currency = input.Currency,
                    TransactionType = "Top Up",
                    Amount = input.Amount,
                    ReferenceNo = input.ReferenceNo,
                    Description = input.Description,
                    TransactionDate = cstDateTime
                });
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateEWalletAsync(UpdateWalletDto input)
        {
            var ewallet = await Repository.FirstOrDefaultAsync(x =>
            (
                x.Customer.Equals(input.OGCustomer) &&
                x.EWalletType.Equals(input.OGEWalletType) &&
                x.Currency.Equals(input.OGCurrency)
            )) ?? throw new UserFriendlyException("No E-Wallet Found");

            var transactions = input.OGCurrency != input.Currency ? await customerTransactionRepository.FirstOrDefaultAsync(x => x.Wallet.Equals(input.Id)) : new CustomerTransaction();

            /* Remove Existing E-Wallet if no transactions are made with said wallet. */
            if (transactions is null) await Repository.DeleteAsync(ewallet);
            else throw new UserFriendlyException("Wallet has been used. Unable to Update.");

            /* Insert New E-Wallet */
            var insert = await Repository.InsertAsync(new Wallet()
            {
                Customer = input.Customer,
                EWalletType = input.EWalletType,
                Currency = input.Currency,
                Id = input.Id
            }) ?? throw new UserFriendlyException("Error Updating EWallet");

            return true;
        }

        public async Task DeleteEWalletAsync(WalletDto input)
        {
            var ewallet = await Repository.FirstOrDefaultAsync(x =>
            (
                x.Customer.Equals(input.Customer) &&
                x.EWalletType.Equals(input.EWalletType) &&
                x.Currency.Equals(input.Currency)
            )) ?? throw new UserFriendlyException("No E-Wallet Found");

            var transactions = await customerTransactionRepository.FirstOrDefaultAsync(x => x.Wallet.Equals(input.Id));

            /* Remove Existing E-Wallet if no transactions are made with said wallet. */
            if (transactions is null) await Repository.DeleteAsync(ewallet);
            else throw new UserFriendlyException("Wallet has been used. Unable to Delete.");
        }

        public async Task<EWalletDto> GetEWalletAsync(WalletDto input)
        {
            if (input.Id is null)
            {
                var eWalletTypes = await eWalletTypeRepository.GetAllListAsync();
                var currencies = await currencyRepository.GetAllListAsync();

                EWalletDto selectedEWallet = new()
                {
                    Id = null,
                    Customer = "",
                    EWalletType = 0,
                    Currency = 0,
                    EWalletTypeDesc = "",
                    CurrencyDesc = "",
                    EWalletTypeList = eWalletTypes,
                    CurrencyList = currencies,
                    Balance = 0
                };

                return selectedEWallet;
            }
            else
            {
                var ewallet = await Repository.FirstOrDefaultAsync(x =>
                            (
                                x.Id.Equals(input.Id)
                            )) ?? throw new UserFriendlyException("No E-Wallet Found");

                var ewallettype = await eWalletTypeRepository.FirstOrDefaultAsync(x => x.Id.Equals(ewallet.EWalletType));
                var currency = await currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(ewallet.Currency));

                EWalletDto selectedEWallet = new()
                {
                    Id = ewallet.Id,
                    Customer = ewallet.Customer,
                    EWalletType = ewallet.EWalletType,
                    Currency = ewallet.Currency,
                    EWalletTypeDesc = ewallettype.Type,
                    CurrencyDesc = currency.Abbr,
                    Balance = ewallet.Balance
                };

                var customerWallet = await Repository.GetAllListAsync(x => x.Customer.Equals(input.Customer));

                /* Get Available E-Wallet Types for this Customer */
                var eWalletTypes = await eWalletTypeRepository.GetAllListAsync();
                var availableEWalletTypes = eWalletTypes.Where(x => !customerWallet.Any(y => y.EWalletType.Equals(x.Id))).ToList();
                var eWalletTypeCurrent = eWalletTypes.FirstOrDefault(x => x.Id.Equals(input.EWalletType));
                availableEWalletTypes.Add(eWalletTypeCurrent);
                selectedEWallet.EWalletTypeList = availableEWalletTypes;
                selectedEWallet.EWalletTypeList.Remove(null);

                /* Get Available Currencies for this Customer */
                var currencies = await currencyRepository.GetAllListAsync();
                var availableCurrencies = currencies.Where(x => !customerWallet.Any(y => y.Currency.Equals(x.Id))).ToList();
                var currencyCurrent = currencies.FirstOrDefault(x => x.Id.Equals(input.Currency));
                availableCurrencies.Add(currencyCurrent);
                selectedEWallet.CurrencyList = availableCurrencies;
                selectedEWallet.CurrencyList.Remove(null);

                return selectedEWallet;
            }
        }

    }
}

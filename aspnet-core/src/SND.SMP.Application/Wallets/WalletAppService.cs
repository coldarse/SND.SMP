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
using System.Reflection.Metadata.Ecma335;
using JetBrains.Annotations;
using SND.SMP.EWalletTypes;
using System.ComponentModel;

namespace SND.SMP.Wallets
{
    public class WalletAppService : AsyncCrudAppService<Wallet, WalletDto, string, PagedWalletResultRequestDto>
    {
        private readonly IRepository<EWalletType, long> _eWalletTypeRepository;
        private readonly IRepository<Currency, long> _currencyRepository;

        public WalletAppService(
            IRepository<Wallet, string> repository,
            IRepository<EWalletType, long> eWalletTypeRepository,
            IRepository<Currency, long> currencyRepository
        ) : base(repository)
        {
            _eWalletTypeRepository = eWalletTypeRepository;
            _currencyRepository = currencyRepository;
        }
        protected override IQueryable<Wallet> CreateFilteredQuery(PagedWalletResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.Customer.Contains(input.Keyword));
        }

        public override async Task<WalletDto> CreateAsync(WalletDto input)
        {
            var wallets = await Repository.GetAllListAsync(x => x.Customer.Equals(input.Customer));
            /* If Wallet does not exist */
            if (wallets.Count == 0) return await base.CreateAsync(input);
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

        public async Task<bool> UpdateEWalletAsync(UpdateWalletDto input)
        {
            var ewallet = await Repository.FirstOrDefaultAsync(x =>
            (
                x.Customer.Equals(input.OGCustomer) &&
                x.EWalletType.Equals(input.OGEWalletType) &&
                x.Currency.Equals(input.OGCurrency)
            )) ?? throw new UserFriendlyException("No E-Wallet Found");

            /* Remove Exisiting E-Wallet */
            await Repository.DeleteAsync(ewallet);

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

            await Repository.DeleteAsync(ewallet);
        }

        public async Task<EWalletDto> GetEWalletAsync(WalletDto input)
        {
            if (input.Id != null)
            {
                var eWalletTypes = await _eWalletTypeRepository.GetAllListAsync();
                var currencies = await _currencyRepository.GetAllListAsync();

                EWalletDto selectedEWallet = new EWalletDto()
                {
                    Id = null,
                    Customer = "",
                    EWalletType = 0,
                    Currency = 0,
                    EWalletTypeDesc = "",
                    CurrencyDesc = "",
                    EWalletTypeList = eWalletTypes,
                    CurrencyList = currencies,
                    Balance = 0f
                };

                return selectedEWallet;
            }
            else
            {
                var ewallet = await Repository.FirstOrDefaultAsync(x =>
                            (
                                x.Customer.Equals(input.Customer) &&
                                x.EWalletType.Equals(input.EWalletType) &&
                                x.Currency.Equals(input.Currency)
                            )) ?? throw new UserFriendlyException("No E-Wallet Found");

                var ewallettype = await _eWalletTypeRepository.FirstOrDefaultAsync(x => x.Id.Equals(ewallet.EWalletType));
                var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(ewallet.Currency));

                EWalletDto selectedEWallet = new EWalletDto()
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
                var eWalletTypes = await _eWalletTypeRepository.GetAllListAsync();
                var availableEWalletTypes = eWalletTypes.Where(x => !customerWallet.Any(y => y.EWalletType.Equals(x.Id))).ToList();
                var eWalletTypeCurrent = eWalletTypes.FirstOrDefault(x => x.Id.Equals(input.EWalletType));
                availableEWalletTypes.Add(eWalletTypeCurrent);
                selectedEWallet.EWalletTypeList = availableEWalletTypes;

                /* Get Available Currencies for this Customer */
                var currencies = await _currencyRepository.GetAllListAsync();
                var availableCurrencies = currencies.Where(x => !customerWallet.Any(y => y.Currency.Equals(x.Id))).ToList();
                var currencyCurrent = currencies.FirstOrDefault(x => x.Id.Equals(input.Currency));
                availableCurrencies.Add(currencyCurrent);
                selectedEWallet.CurrencyList = availableCurrencies;

                return selectedEWallet;
            }
        }

    }
}

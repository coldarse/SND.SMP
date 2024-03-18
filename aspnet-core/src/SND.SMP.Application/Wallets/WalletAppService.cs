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
using SND.SMP.CustomerTransactions;
using System.Diagnostics;

namespace SND.SMP.Wallets
{
    public class WalletAppService : AsyncCrudAppService<Wallet, WalletDto, string, PagedWalletResultRequestDto>
    {
        private readonly IRepository<EWalletType, long> _eWalletTypeRepository;
        private readonly IRepository<Currency, long> _currencyRepository;
        private readonly IRepository<CustomerTransaction, long> _customerTransactionRepository;

        public WalletAppService(
            IRepository<Wallet, string> repository,
            IRepository<EWalletType, long> eWalletTypeRepository,
            IRepository<Currency, long> currencyRepository,
            IRepository<CustomerTransaction, long> customerTransactionRepository
        ) : base(repository)
        {
            _eWalletTypeRepository = eWalletTypeRepository;
            _currencyRepository = currencyRepository;
            _customerTransactionRepository = customerTransactionRepository;
        }
        protected override IQueryable<Wallet> CreateFilteredQuery(PagedWalletResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.Customer.Contains(input.Keyword));
        }

        // public bool RestartContainerByName(string name)
        // {
        //     string getContainerId_Command = $"docker ps -aqf \"name={name}\"";
        //     var psi = new ProcessStartInfo();
        //     psi.FileName = "/bin/bash";
        //     psi.Arguments = $"-c \"{getContainerId_Command}\"";
        //     psi.RedirectStandardOutput = true;
        //     psi.UseShellExecute = false;
        //     psi.CreateNoWindow = true;

        //     using var getContainerId_Process = Process.Start(psi);

        //     getContainerId_Process.WaitForExit();

        //     var containerId = getContainerId_Process.StandardOutput.ReadToEnd();

        //     containerId = containerId.Replace("\"n", "");

        //     string restartContainer_Command = $"docker restart {containerId}";

        //     psi.Arguments = $"-c \"{restartContainer_Command}\"";

        //     using var restartContainer_Process = Process.Start(psi);

        //     restartContainer_Process.WaitForExit();

        //     var restartedContainer = restartContainer_Process.StandardOutput.ReadToEnd();

        //     if (restartedContainer is not null) return true;
        //     return false;
        // }

        // public string runBash(string command)
        // {
        //     var psi = new ProcessStartInfo();
        //     psi.FileName = "/bin/bash";
        //     psi.Arguments = $"-c \"{command}\"";
        //     psi.RedirectStandardOutput = true;
        //     psi.UseShellExecute = false;
        //     psi.CreateNoWindow = true;

        //     using var process = Process.Start(psi);

        //     process.WaitForExit();

        //     var output = process.StandardOutput.ReadToEnd();

        //     return output;
        // }


        public async Task<List<DetailedEWallet>> GetAllWalletsAsync(string code)
        {
            var wallet = await Repository.GetAllListAsync(x => x.Customer.Equals(code));

            var currency = await _currencyRepository.GetAllListAsync();

            List<DetailedEWallet> wallets = new List<DetailedEWallet>();
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

                var addTransaction = await _customerTransactionRepository.InsertAsync(new CustomerTransaction()
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
            if (input.Id is null)
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
                selectedEWallet.EWalletTypeList.Remove(null);

                /* Get Available Currencies for this Customer */
                var currencies = await _currencyRepository.GetAllListAsync();
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

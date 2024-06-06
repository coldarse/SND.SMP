using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.Customers;
using Abp.Application.Services.Dto;
using SND.SMP.Rates;
using Abp.Linq.Extensions;
using System.Linq.Dynamic.Core;
using Abp.UI;
using SND.SMP.Postals;
using System.Drawing;
using SND.SMP.RateItems;
using SND.SMP.Wallets;
using SND.SMP.Currencies;
using SND.SMP.EWalletTypes;
using SND.SMP.PostalCountries;

namespace SND.SMP.CustomerPostals
{
    public class CustomerPostalAppService(
        IRepository<CustomerPostal, long> repository,
        IRepository<Rate, int> rateRepository,
        IRepository<Customer, long> customerRepository,
        IRepository<Postal, long> postalRepository,
        IRepository<CustomerPostal, long> customerPostalRepository,
        IRepository<RateItem, long> rateItemRepository,
        IRepository<Wallet, string> walletRepository,
        IRepository<Currency, long> currencyRepository,
        IRepository<EWalletType, long> eWalletTypeRepository,
        IRepository<PostalCountry, long> postalCountryRepository
        ) : AsyncCrudAppService<CustomerPostal, DetailedCustomerPostalDto, long, PagedCustomerPostalResultRequestDto>(repository)
    {
        private readonly IRepository<Rate, int> _rateRepository = rateRepository;
        private readonly IRepository<Customer, long> _customerRepository = customerRepository;
        private readonly IRepository<Postal, long> _postalRepository = postalRepository;
        private readonly IRepository<CustomerPostal, long> _customerPostalRepository = customerPostalRepository;
        private readonly IRepository<RateItem, long> _rateItemRepository = rateItemRepository;
        private readonly IRepository<Wallet, string> _walletRepository = walletRepository;
        private readonly IRepository<Currency, long> _currencyRepository = currencyRepository;
        private readonly IRepository<EWalletType, long> _eWalletTypeRepository = eWalletTypeRepository;
        private readonly IRepository<PostalCountry, long> _postalCountryRepository = postalCountryRepository;

        protected override IQueryable<CustomerPostal> CreateFilteredQuery(PagedCustomerPostalResultRequestDto input)
        {
            return Repository.GetAllIncluding().Where(x => x.AccountNo.Equals(input.AccountNo));
        }

        public async Task<FullDetailedCustomerPostal> GetFullDetailedCustomerPostal(PagedCustomerPostalResultRequestDto input)
        {
            var postals = await _postalRepository.GetAllListAsync();
            var rates = await rateRepository.GetAllListAsync();

            postals = postals.DistinctBy(x => x.PostalCode).ToList();

            List<PostalDDL> postalDDL = [];
            foreach (var postal in postals.ToList())
            {
                postalDDL.Add(new PostalDDL()
                {
                    PostalCode = postal.PostalCode,
                    PostalDesc = postal.PostalDesc
                });
            }

            List<RateDDL> rateDDL = [];
            foreach (var rate in rates.ToList())
            {
                rateDDL.Add(new RateDDL()
                {
                    Id = rate.Id,
                    CardName = rate.CardName
                });
            }

            var detailed = await GetAllAsync(input);

            return new FullDetailedCustomerPostal()
            {
                PagedResultDto = detailed,
                PostalDDLs = postalDDL,
                RateDDLs = rateDDL,
            };
        }

        public override async Task<PagedResultDto<DetailedCustomerPostalDto>> GetAllAsync(PagedCustomerPostalResultRequestDto input)
        {
            CheckGetAllPermission();

            var query = CreateFilteredQuery(input);

            var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id.Equals(input.AccountNo));

            var rates = await _rateRepository.GetAllListAsync();

            List<DetailedCustomerPostalDto> detailed = [];

            foreach (var postal in query.ToList())
            {
                var rateCard = rates.FirstOrDefault(x => x.Id.Equals(postal.Rate));

                detailed.Add(new DetailedCustomerPostalDto()
                {
                    Id = postal.Id,
                    Postal = postal.Postal,
                    Rate = postal.Rate,
                    RateCard = rateCard.CardName,
                    AccountNo = postal.AccountNo,
                    Code = customer.Code
                });
            }

            detailed = detailed.
                WhereIf(!input.Keyword.IsNullOrWhiteSpace(),
                    x => x.Postal.Contains(input.Keyword) ||
                         x.RateCard.Contains(input.Keyword)).ToList();

            var totalCount = detailed.Count;

            detailed = [.. ApplySorting(detailed.AsQueryable(), input)];
            detailed = [.. ApplyPaging(detailed.AsQueryable(), input)];


            return new PagedResultDto<DetailedCustomerPostalDto>(
                totalCount,
                [.. detailed]
            );
        }

        public async Task<CreateWalletDto> IsCurrencyWalletExist(int rate, int accNo)
        {
            var rateItem = await _rateItemRepository.FirstOrDefaultAsync(x => x.RateId.Equals(rate));

            var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id.Equals(accNo));

            var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(rateItem.CurrencyId));

            var wallet = await _walletRepository.FirstOrDefaultAsync(x =>
                x.Customer.Equals(customer.Code) &&
                x.Currency.Equals(rateItem.CurrencyId)
            );

            if (wallet is not null)
                return new CreateWalletDto() { Exists = true };

            return new CreateWalletDto()
            {
                Exists = false,
                Create = false,
                Customer = customer.Code,
                EWalletType = 1,
                Currency = rateItem.CurrencyId,
                CurrencyDesc = currency.Abbr,
                Balance = Convert.ToDecimal(0),
            };
        }

        private async Task<string> GetEWalletTypeAbbr(long id)
        {
            var ewallettype = await _eWalletTypeRepository.FirstOrDefaultAsync(x => x.Id.Equals(id));
            string abbr = "";
            switch (ewallettype.Type)
            {
                case "Prepaid":
                    abbr = "PP";
                    break;
                case "Credit Term":
                    abbr = "CT";
                    break;
            }
            return abbr;
        }

        private async Task<string> GetCurrencyAbbr(long id)
        {
            var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(id));
            return currency.Abbr;
        }

        public override async Task<DetailedCustomerPostalDto> CreateAsync(DetailedCustomerPostalDto input)
        {
            CheckCreatePermission();

            var exists = await Repository.FirstOrDefaultAsync(x =>
                // x.Postal.Equals(input.Postal) && 
                x.Rate.Equals(input.Rate) &&
                x.AccountNo.Equals(input.AccountNo)
            );

            if (exists is not null) throw new UserFriendlyException("Rate for this Customer Postal already exists");

            if (input.CreateWallet.Create == true)
            {
                var walletName =
                    input.CreateWallet.Customer +
                    await GetEWalletTypeAbbr((long)input.CreateWallet.EWalletType) +
                    await GetCurrencyAbbr((long)input.CreateWallet.Currency);

                await _walletRepository.InsertAsync(new Wallet()
                {
                    Customer = input.CreateWallet.Customer,
                    EWalletType = (long)input.CreateWallet.EWalletType,
                    Currency = (long)input.CreateWallet.Currency,
                    Balance = (decimal)input.CreateWallet.Balance,
                    Id = walletName
                });
            }

            CustomerPostal entity = new()
            {
                Postal = input.Postal,
                Rate = input.Rate,
                AccountNo = input.AccountNo
            };

            var created = await Repository.InsertAsync(entity);

            return new DetailedCustomerPostalDto()
            {
                Postal = created.Postal,
                Rate = created.Rate,
                RateCard = "",
                AccountNo = created.AccountNo,
                Code = ""
            };
        }

        public override async Task<DetailedCustomerPostalDto> UpdateAsync(DetailedCustomerPostalDto input)
        {
            CheckUpdatePermission();

            var entity = await GetEntityByIdAsync(input.Id);

            if (input.CreateWallet.Create == true)
            {
                var walletName =
                    input.CreateWallet.Customer +
                    await GetEWalletTypeAbbr((long)input.CreateWallet.EWalletType) +
                    await GetCurrencyAbbr((long)input.CreateWallet.Currency);

                await _walletRepository.InsertAsync(new Wallet()
                {
                    Customer = input.CreateWallet.Customer,
                    EWalletType = (long)input.CreateWallet.EWalletType,
                    Currency = (long)input.CreateWallet.Currency,
                    Balance = (decimal)input.CreateWallet.Balance,
                    Id = walletName
                });
            }

            entity.Postal = input.Postal;
            entity.Rate = input.Rate;
            entity.AccountNo = input.AccountNo;

            var updated = await Repository.UpdateAsync(entity);

            return new DetailedCustomerPostalDto()
            {
                Postal = updated.Postal,
                Rate = updated.Rate,
                RateCard = "",
                AccountNo = updated.AccountNo,
                Code = ""
            };
        }

        public async Task<List<PostalDDL>> GetCustomerPostalsByAccountNo(long accountNo)
        {
            var customerPostals = await Repository.GetAllListAsync(x => x.AccountNo.Equals(accountNo));
            customerPostals = customerPostals.DistinctBy(x => x.Postal).ToList();

            var postals = await _postalRepository.GetAllListAsync();
            postals = postals.DistinctBy(x => x.PostalCode).ToList();

            var rates = await _rateRepository.GetAllListAsync();

            List<PostalDDL> postalDDLs = [];
            foreach (CustomerPostal cp in customerPostals.ToList())
            {
                var postal = postals.FirstOrDefault(x => x.PostalCode.Equals(cp.Postal));
                string rateCardName = rates.FirstOrDefault(x => x.Id.Equals(cp.Rate)).CardName;

                postalDDLs.Add(new PostalDDL()
                {
                    PostalCode = postal.PostalCode,
                    PostalDesc = postal.PostalDesc + $" ({rateCardName})",
                });
            }

            return postalDDLs;
        }

        private IQueryable<DetailedCustomerPostalDto> ApplySorting(IQueryable<DetailedCustomerPostalDto> query, PagedCustomerPostalResultRequestDto input)
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

        private IQueryable<DetailedCustomerPostalDto> ApplyPaging(IQueryable<DetailedCustomerPostalDto> query, PagedCustomerPostalResultRequestDto input)
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

        public async Task<List<GroupedCustomerPostal>> GetGroupedCustomerPostal()
        {
            var customerPostals = await _customerPostalRepository.GetAllListAsync();

            var customers = await _customerRepository.GetAllListAsync();

            var grouped = customerPostals
                                .GroupBy(g => g.AccountNo)
                                .Select(u => new GroupedCustomerPostal()
                                {
                                    CustomerId = customers.FirstOrDefault(x => x.Id.Equals(u.Key)).Id,
                                    CustomerCode = customers.FirstOrDefault(x => x.Id.Equals(u.Key)).Code,
                                    CustomerName = customers.FirstOrDefault(x => x.Id.Equals(u.Key)).CompanyName,
                                    CustomerPostal = [.. u]
                                });

            return [.. grouped];
        }
    }
}

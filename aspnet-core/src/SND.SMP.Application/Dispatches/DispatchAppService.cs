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
using SND.SMP.Dispatches.Dto;
using SND.SMP.Customers;
using SND.SMP.Bags;
using Abp.UI;
using SND.SMP.Items;
using Abp.Runtime.Session;
using Abp.EntityFrameworkCore.Repositories;
using SND.SMP.CustomerPostals;
using SND.SMP.Rates;
using SND.SMP.Authorization.Users;
using SND.SMP.RateItems;
using SND.SMP.Wallets;

namespace SND.SMP.Dispatches
{
    public class DispatchAppService(IRepository<Dispatch, int> repository, IRepository<Customer, long> customerRepository, IRepository<Bag, int> bagRepository, IRepository<Item, string> itemRepository, IRepository<CustomerPostal, long> customerPostalRepository, IRepository<Rate, int> rateRepository, IRepository<RateItem, long> rateItemRepository, IRepository<Wallet, string> walletRepository) : AsyncCrudAppService<Dispatch, DispatchDto, int, PagedDispatchResultRequestDto>(repository)
    {

        public readonly IRepository<Customer, long> _customerRepository = customerRepository;
        public readonly IRepository<Bag, int> _bagRepository = bagRepository;
        public readonly IRepository<Item, string> _itemRepository = itemRepository;
        public readonly IRepository<CustomerPostal, long> _customerPostalRepository = customerPostalRepository;
        public readonly IRepository<Rate, int> _rateRepository = rateRepository;
        public readonly IRepository<RateItem, long> _rateItemRepository = rateItemRepository;
        public readonly IRepository<Wallet, string> _walletRepository = walletRepository;

        protected override IQueryable<Dispatch> CreateFilteredQuery(PagedDispatchResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.CustomerCode.Contains(input.Keyword) ||
                    x.POBox.Contains(input.Keyword) ||
                    x.PPI.Contains(input.Keyword) ||
                    x.PostalCode.Contains(input.Keyword) ||
                    x.ServiceCode.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword) ||
                    x.DispatchNo.Contains(input.Keyword) ||
                    x.FlightTrucking.Contains(input.Keyword) ||
                    x.BatchId.Contains(input.Keyword) ||
                    x.CN38.Contains(input.Keyword) ||
                    x.Remark.Contains(input.Keyword) ||
                    x.AirlineCode.Contains(input.Keyword) ||
                    x.FlightNo.Contains(input.Keyword) ||
                    x.PortDeparture.Contains(input.Keyword) ||
                    x.ExtDispatchNo.Contains(input.Keyword) ||
                    x.AirportTranshipment.Contains(input.Keyword) ||
                    x.OfficeDestination.Contains(input.Keyword) ||
                    x.OfficeOrigin.Contains(input.Keyword) ||
                    x.Stage1StatusDesc.Contains(input.Keyword) ||
                    x.Stage2StatusDesc.Contains(input.Keyword) ||
                    x.Stage3StatusDesc.Contains(input.Keyword) ||
                    x.Stage4StatusDesc.Contains(input.Keyword) ||
                    x.Stage5StatusDesc.Contains(input.Keyword) ||
                    x.Stage6StatusDesc.Contains(input.Keyword) ||
                    x.Stage7StatusDesc.Contains(input.Keyword) ||
                    x.Stage8StatusDesc.Contains(input.Keyword) ||
                    x.Stage9StatusDesc.Contains(input.Keyword) ||
                    x.StatusAPI.Contains(input.Keyword) ||
                    x.CountryOfLoading.Contains(input.Keyword) ||
                    x.PostManifestMsg.Contains(input.Keyword) ||
                    x.PostDeclarationMsg.Contains(input.Keyword) ||
                    x.AirwayBLNo.Contains(input.Keyword) ||
                    x.BRCN38RequestId.Contains(input.Keyword) ||
                    x.CORateOptionId.Contains(input.Keyword) ||
                    x.PaymentMode.Contains(input.Keyword) ||
                    x.CurrencyId.Contains(input.Keyword));
        }


        public async Task<GetPostCheck> GetPostCheckAsync(string dispatchNo)
        {
            var dispatch = await Repository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatchNo)) ?? throw new UserFriendlyException("Dispatch Not Found.");

            var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Code.Equals(dispatch.CustomerCode)) ?? throw new UserFriendlyException("Customer Not Found.");

            var bags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id)) ?? throw new UserFriendlyException("No Bags Found.");

            return new GetPostCheck()
            {
                CompanyName = customer.CompanyName ?? "",
                CompanyCode = customer.Code ?? "",
                DispatchNo = dispatchNo ?? "",
                FlightTrucking = dispatch.FlightTrucking ?? "",
                ETA = dispatch.ETAtoHKG ?? DateOnly.FromDateTime(DateTime.Now),
                ATA = dispatch.ATA ?? DateTime.MinValue,
                PreCheckNoOfBag = dispatch.NoofBag ?? 0,
                PostCheckNoOfBag = dispatch.PostCheckTotalBags ?? 0,
                PreCheckWeight = dispatch.TotalWeight ?? Convert.ToDecimal(0),
                PostCheckWeight = dispatch.PostCheckTotalWeight ?? Convert.ToDecimal(0),
                Bags = bags ?? []
            };
        }

        private async Task<PriceAndCurrencyId> CalculatePrice(decimal weightVariance, string countryCode, int rateId, string productCode, string serviceCode, int totalItems, bool skipRegisterFee)
        {
            decimal registerFee = 0;
            if (!skipRegisterFee)
            {
                var rateItemForFee = await _rateItemRepository.FirstOrDefaultAsync(x =>
                                x.RateId.Equals(rateId) &&
                                x.CountryCode.Equals(countryCode) &&
                                x.ProductCode.Equals(productCode) &&
                                x.ServiceCode.Equals(serviceCode)) ?? throw new UserFriendlyException("No Rate Item Found");

                registerFee = rateItemForFee.Fee;
            }

            var rateItem = await _rateItemRepository.FirstOrDefaultAsync(x =>
                                x.RateId.Equals(rateId) &&
                                x.CountryCode.Equals(countryCode) &&
                                x.ProductCode.Equals(productCode) &&
                                x.ServiceCode.Equals(serviceCode)) ?? throw new UserFriendlyException("No Rate Item Found");

            return new PriceAndCurrencyId()
            {
                Price = (rateItem.Total * weightVariance) + (totalItems * registerFee),
                CurrencyId = rateItem.CurrencyId,
            };
        }


        public async Task<bool> ByPassPostCheck(string dispatchNo, decimal weightGap)
        {
            var dispatch = await Repository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatchNo)) ?? throw new UserFriendlyException("No Dispatch Found");

            decimal averageWeight = (decimal)(weightGap / dispatch.NoofBag);

            var remainingItems = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatch.Id) && x.DateStage2.Equals(null)) ?? throw new UserFriendlyException("No Items with this Dispatch");

            foreach (var item in remainingItems)
            {
                item.DateStage2 = DateTime.Now;
                await _itemRepository.UpdateAsync(item);
                await _itemRepository.GetDbContext().SaveChangesAsync();
            }

            var remainingBags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id)) ?? throw new UserFriendlyException("No Bags with this Dispatch");

            foreach (var bag in remainingBags)
            {
                bag.WeightPost = (bag.WeightPre == null ? 0 : bag.WeightPre) + averageWeight;
                bag.WeightVariance = averageWeight;
                await _bagRepository.UpdateAsync(bag);
                await _bagRepository.GetDbContext().SaveChangesAsync();
            }

            dispatch.WeightGap = weightGap;
            dispatch.WeightAveraged = averageWeight;
            dispatch.Status = 2;
            dispatch.PostCheckTotalBags = dispatch.NoofBag;
            dispatch.PostCheckTotalWeight = (dispatch.TotalWeight.Equals(null) ? 0 : dispatch.TotalWeight) + weightGap;
            await Repository.UpdateAsync(dispatch);
            await Repository.GetDbContext().SaveChangesAsync();

            var weightAdjustmentBags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id) && !x.WeightVariance.Equals(null)) ?? throw new UserFriendlyException("No Bags without WeightVariance");

            decimal totalWeightAdjustmentPrice = 0;
            decimal totalWeightAdjustment = 0;
            decimal totalSurchargePrice = 0;
            decimal totalSurchargeWeight = 0;
            decimal totalRefundPrice = 0;
            decimal totalRefundWeight = 0;
            PriceAndCurrencyId priceAndCurrencyId = new();

            var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Code.Equals(dispatch.CustomerCode)) ?? throw new UserFriendlyException("No Customer Found");

            var customerPostal = await _customerPostalRepository.FirstOrDefaultAsync(x => x.AccountNo.Equals(customer.Id) && x.Postal.Equals(dispatch.PostalCode)) ?? throw new UserFriendlyException("No Customer Postal Found with this Customer and Postal Code");

            var rate = await _rateRepository.FirstOrDefaultAsync(x => x.Id.Equals(customerPostal.Rate)) ?? throw new UserFriendlyException("No Rate Found");

            foreach (var bag in weightAdjustmentBags)
            {
                var itemList = await _itemRepository.GetAllListAsync(x => x.BagNo == bag.BagNo) ?? throw new UserFriendlyException($"No Items found with Bag No of {bag.BagNo}");
                int totalItems = itemList.Count;

                priceAndCurrencyId = await CalculatePrice((decimal)bag.WeightVariance, bag.CountryCode, rate.Id, dispatch.ProductCode, dispatch.ServiceCode, totalItems, true);
                totalWeightAdjustmentPrice = priceAndCurrencyId.Price;
                totalWeightAdjustment = (decimal)bag.WeightVariance;

                if (totalWeightAdjustmentPrice > 0)
                {
                    totalSurchargePrice += totalWeightAdjustmentPrice;
                    totalSurchargeWeight += totalWeightAdjustment;
                }
                if (totalWeightAdjustmentPrice < 0)
                {
                    totalRefundPrice += totalWeightAdjustmentPrice;
                    totalRefundWeight += totalWeightAdjustment;
                }
            }

            var rateItem = await _rateItemRepository.FirstOrDefaultAsync(x => x.Id.Equals(customerPostal.Rate) && x.ServiceCode.Equals(dispatch.ServiceCode));

            if (totalSurchargePrice > 0)
            {
                // db.WeightAdjustment.Add(new WeightAdjustment()
                // {
                //     Amount = totalSurchargePrice,
                //     DateTime = DateTime.UtcNow,
                //     Description = PTS.Services.Common.SystemHelper.PostCheck + " Under Declare",
                //     ReferenceNo = dispatch.DispatchNo,
                //     UserId = customer.UserId,
                //     Weight = totalSurchargeWeight
                // });

                var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                wallet.Balance -= totalSurchargePrice;
                await _walletRepository.UpdateAsync(wallet);
                await _walletRepository.GetDbContext().SaveChangesAsync();
            }

            if (totalRefundPrice < 0)
            {
                // db.WeightAdjustment.Add(new WeightAdjustment()
                // {
                //     Amount = totalRefundPrice * (-1),
                //     DateTime = DateTime.UtcNow,
                //     Description = PTS.Services.Common.SystemHelper.PostCheck + " Over Declare",
                //     ReferenceNo = dispatch.DispatchNo,
                //     UserId = customer.UserId,
                //     Weight = totalRefundWeight
                // });

                var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                wallet.Balance -= totalRefundPrice;
                await _walletRepository.UpdateAsync(wallet);
                await _walletRepository.GetDbContext().SaveChangesAsync();
            }

            return true;
        }
    }
}

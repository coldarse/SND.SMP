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
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Data;
using System.IO;
using SND.SMP.WeightAdjustments;
using SND.SMP.Refunds;
using Abp.Application.Services.Dto;
using SND.SMP.Postals;

namespace SND.SMP.Dispatches
{
    public class DispatchAppService(
        IRepository<Dispatch, int> repository,
        IRepository<Customer, long> customerRepository,
        IRepository<Bag, int> bagRepository,
        IRepository<Item, string> itemRepository,
        IRepository<CustomerPostal, long> customerPostalRepository,
        IRepository<Rate, int> rateRepository,
        IRepository<RateItem, long> rateItemRepository,
        IRepository<Wallet, string> walletRepository,
        IRepository<Dispatch, int> dispatchRepository,
        IRepository<WeightAdjustment, int> weightAdjustmentRepository,
        IRepository<Refund, int> refundRepository,
        IRepository<Postal, long> postalRepository
    ) : AsyncCrudAppService<Dispatch, DispatchDto, int, PagedDispatchResultRequestDto>(repository)
    {

        private readonly IRepository<Customer, long> _customerRepository = customerRepository;
        private readonly IRepository<Bag, int> _bagRepository = bagRepository;
        private readonly IRepository<Item, string> _itemRepository = itemRepository;
        private readonly IRepository<CustomerPostal, long> _customerPostalRepository = customerPostalRepository;
        private readonly IRepository<Rate, int> _rateRepository = rateRepository;
        private readonly IRepository<RateItem, long> _rateItemRepository = rateItemRepository;
        private readonly IRepository<Wallet, string> _walletRepository = walletRepository;
        private readonly IRepository<Dispatch, int> _dispatchRepository = dispatchRepository;
        private readonly IRepository<WeightAdjustment, int> _weightAdjustmentRepository = weightAdjustmentRepository;
        private readonly IRepository<Refund, int> _refundRepository = refundRepository;
        private readonly IRepository<Postal, long> _postalRepository = postalRepository;

        private async Task<DataTable> ConvertToDatatable(Stream ms)
        {
            DataTable dataTable = new();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(ms))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                // Assuming the first row is the header
                for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                {
                    string columnName = worksheet.Cells[1, i].Value?.ToString();
                    if (!string.IsNullOrEmpty(columnName))
                    {
                        dataTable.Columns.Add(columnName);
                    }
                }

                // Populate DataTable with data from Excel
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    DataRow dataRow = dataTable.NewRow();
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        dataRow[col - 1] = worksheet.Cells[row, col].Value;
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
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


        protected override IQueryable<Dispatch> CreateFilteredQuery(PagedDispatchResultRequestDto input)
        {
            return input.isAdmin ?
                Repository.GetAllIncluding()
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
                        x.CurrencyId.Contains(input.Keyword))
                :
                Repository.GetAllIncluding()
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
                        x.CurrencyId.Contains(input.Keyword))
                    .Where(x => x.CustomerCode.Equals(input.CustomerCode));

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

        public async Task<bool> UndoPostCheck(string dispatchNo)
        {
            var dispatch = await _dispatchRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatchNo)) ?? throw new UserFriendlyException("No Dispatch Found");
            var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Code.Equals(dispatch.CustomerCode)) ?? throw new UserFriendlyException("No Customer Found");
            var customerPostal = await _customerPostalRepository.FirstOrDefaultAsync(x => x.AccountNo.Equals(customer.Id) && x.Postal.Equals(dispatch.PostalCode)) ?? throw new UserFriendlyException("No Customer Postal Found with this Customer and Postal Code");
            var rate = await _rateRepository.FirstOrDefaultAsync(x => x.Id.Equals(customerPostal.Rate)) ?? throw new UserFriendlyException("No Rate Found");
            var rateItem = await _rateItemRepository.FirstOrDefaultAsync(x => x.Id.Equals(customerPostal.Rate) && x.ServiceCode.Equals(dispatch.ServiceCode));

            dispatch.Status = 1;
            dispatch.WeightAveraged = null;
            dispatch.WeightGap = null;
            dispatch.PostCheckTotalBags = null;
            dispatch.PostCheckTotalWeight = null;

            var bags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id));
            foreach (var bag in bags)
            {
                bag.WeightVariance = null;
                bag.WeightPost = null;
            }

            var items = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatch.Id));
            foreach (var item in items)
            {
                item.DateStage2 = DateTime.MinValue;
            }

            var wa = await _weightAdjustmentRepository.FirstOrDefaultAsync(x =>
                                         x.ReferenceNo.Equals(dispatch.DispatchNo) &&
                                         x.Description.Contains("Under Declare") &&
                                        !x.InvoiceId.Equals(null)
                                ) ?? throw new UserFriendlyException("No Weight Adjustment Record Found");

            decimal refundAmount = wa.Amount.Equals(null) ? 0 : wa.Amount;

            wa.InvoiceId = 0;
            wa.Description = $"Undid Post Check for Dispatch {dispatchNo}";

            await _weightAdjustmentRepository.UpdateAsync(wa);
            await _weightAdjustmentRepository.GetDbContext().SaveChangesAsync();

            var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
            wallet.Balance += refundAmount;
            await _walletRepository.UpdateAsync(wallet);
            await _walletRepository.GetDbContext().SaveChangesAsync();

            return true;
        }

        public async Task<bool> SavePostCheck(GetPostCheck getPostCheck)
        {
            if (getPostCheck.Bags is not null && getPostCheck.Bags.Count > 0)
            {
                var random = new Random();

                int? dispatchId = getPostCheck.Bags[0].DispatchId is null ? 0 : getPostCheck.Bags[0].DispatchId;
                Dispatch dispatch = await _dispatchRepository.FirstOrDefaultAsync(x => x.Id.Equals(dispatchId)) ?? throw new UserFriendlyException("No dispatch found");

                string productCode = dispatch.ProductCode;

                Customer customer = await _customerRepository.FirstOrDefaultAsync(x => x.Code.Equals(getPostCheck.CompanyCode));

                dispatch.ATA = getPostCheck.ATA;
                dispatch.PostCheckTotalBags = getPostCheck.PostCheckNoOfBag;
                dispatch.PostCheckTotalWeight = getPostCheck.PostCheckWeight;
                dispatch.NoofBag = dispatch.PostCheckTotalBags;
                dispatch.TotalWeight = dispatch.PostCheckTotalWeight;
                dispatch.Status = 2;

                await _dispatchRepository.UpdateAsync(dispatch);
                await _dispatchRepository.GetDbContext().SaveChangesAsync();


                var customerPostal = await _customerPostalRepository.FirstOrDefaultAsync(x => x.AccountNo.Equals(customer.Id) && x.Postal.Equals(dispatch.PostalCode)) ?? throw new UserFriendlyException("No Customer Postal Found with this Customer and Postal Code");

                var rate = await _rateRepository.FirstOrDefaultAsync(x => x.Id.Equals(customerPostal.Rate)) ?? throw new UserFriendlyException("No Rate Found");

                var rateItem = await _rateItemRepository.FirstOrDefaultAsync(x => x.Id.Equals(customerPostal.Rate) && x.ServiceCode.Equals(dispatch.ServiceCode));

                var items = await _itemRepository.GetAllListAsync(u => u.DispatchID.Equals(dispatchId));

                foreach (var bag in getPostCheck.Bags)
                {
                    if (bag.WeightPost is not null)
                    {
                        var bagItems = items.Where(x => x.BagNo.Equals(bag.BagNo)).ToList();

                        foreach (var bagItem in bagItems)
                        {
                            bagItem.DateStage2 = DateTime.Now.AddMilliseconds(random.Next(5000, 60000));
                            await _itemRepository.UpdateAsync(bagItem);
                            await _itemRepository.GetDbContext().SaveChangesAsync();
                        }

                        await _bagRepository.UpdateAsync(bag);
                        await _bagRepository.GetDbContext().SaveChangesAsync();
                    }
                }

                var missingBags = await _bagRepository.GetAllListAsync(x =>
                                                    x.DispatchId.Equals(dispatchId) &&
                                                    x.WeightPost.Equals(0));

                if (missingBags is not null)
                {
                    decimal totalRefund = 0;
                    decimal missingWeight = 0;
                    var itemsUnderCurrenctDispatch = await _itemRepository.GetAllListAsync(u => u.DispatchID.Equals(dispatchId));

                    foreach (var missingBag in missingBags)
                    {
                        missingWeight = missingBag.WeightVariance.Value;

                        var missingItems = itemsUnderCurrenctDispatch.Where(x => x.BagNo.Equals(missingBag.BagNo)).ToList();
                        if (missingItems is not null)
                        {
                            foreach (var missingItem in missingItems)
                            {
                                totalRefund += missingItem.Price is null ? Convert.ToDecimal(0) : Convert.ToDecimal(missingItem.Price);
                            }
                        }
                    }

                    if (totalRefund > 0)
                    {
                        await _refundRepository.InsertAsync(new Refund()
                        {
                            Amount = totalRefund,
                            DateTime = DateTime.UtcNow,
                            Description = "Post Check Over Declare",
                            ReferenceNo = dispatch.DispatchNo,
                            UserId = 0,
                            Weight = missingWeight
                        });
                        await _refundRepository.GetDbContext().SaveChangesAsync();

                        var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                        wallet.Balance += totalRefund;
                        await _walletRepository.UpdateAsync(wallet);
                        await _walletRepository.GetDbContext().SaveChangesAsync();
                    }
                }

                var waBags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatchId) && !x.WeightVariance.Equals(null));


                decimal totalWeightAdjustmentPrice = 0;
                decimal totalWeightAdjustment = 0;
                decimal totalSurchargePrice = 0;
                decimal totalSurchargeWeight = 0;
                decimal totalRefundPrice = 0;
                decimal totalRefundWeight = 0;

                PriceAndCurrencyId priceAndCurrencyId;

                string rateCardName = rate.CardName;

                if (waBags is null)
                {
                    var dispatchItems = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatchId));

                    foreach (var waBag in waBags)
                    {
                        int totalItems = dispatchItems.Count(x => x.BagNo.Equals(waBag.BagNo));

                        priceAndCurrencyId = await CalculatePrice(waBag.WeightVariance.Value, waBag.CountryCode, rate.Id, dispatch.ProductCode, dispatch.ServiceCode, totalItems, true);
                        totalWeightAdjustmentPrice = priceAndCurrencyId.Price;
                        totalWeightAdjustment = waBag.WeightVariance.Value;

                        if (totalWeightAdjustmentPrice >= 0)
                        {
                            totalSurchargePrice += totalWeightAdjustmentPrice;
                            totalSurchargeWeight += totalWeightAdjustment;
                            waBag.UnderAmount = totalWeightAdjustmentPrice;
                        }
                        else
                        {
                            totalRefundPrice += totalWeightAdjustmentPrice;
                            totalRefundWeight += totalWeightAdjustment;
                            waBag.UnderAmount = totalWeightAdjustmentPrice * (-1);
                        }
                    }


                    if (totalSurchargePrice > 0)
                    {
                        await _weightAdjustmentRepository.InsertAsync(new WeightAdjustment()
                        {
                            Amount = totalSurchargePrice,
                            DateTime = DateTime.UtcNow,
                            Description = "Post Check Under Declare",
                            ReferenceNo = dispatch.DispatchNo,
                            UserId = 0,
                            Weight = totalSurchargeWeight
                        });
                        await _weightAdjustmentRepository.GetDbContext().SaveChangesAsync();

                        var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                        wallet.Balance -= totalSurchargePrice;
                        await _walletRepository.UpdateAsync(wallet);
                        await _walletRepository.GetDbContext().SaveChangesAsync();
                    }


                    if (totalRefundPrice < 0)
                    {
                        await _weightAdjustmentRepository.InsertAsync(new WeightAdjustment()
                        {
                            Amount = totalRefundPrice * (-1),
                            DateTime = DateTime.UtcNow,
                            Description = "Post Check Over Declare",
                            ReferenceNo = dispatch.DispatchNo,
                            UserId = 0,
                            Weight = totalRefundWeight
                        });
                        await _weightAdjustmentRepository.GetDbContext().SaveChangesAsync();

                        var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                        wallet.Balance -= totalRefundPrice;
                        await _walletRepository.UpdateAsync(wallet);
                        await _walletRepository.GetDbContext().SaveChangesAsync();
                    }
                }
                return true;
            }

            return false;


        }

        public async Task<bool> ByPassPostCheck(string dispatchNo, decimal weightGap)
        {
            var random = new Random();

            var dispatch = await Repository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatchNo)) ?? throw new UserFriendlyException("No Dispatch Found");

            decimal averageWeight = (decimal)(weightGap / dispatch.NoofBag);

            var remainingItems = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatch.Id) && x.DateStage2.Equals(null)) ?? throw new UserFriendlyException("No Items with this Dispatch");

            foreach (var item in remainingItems)
            {
                item.DateStage2 = DateTime.Now.AddMilliseconds(random.Next(5000, 60000));
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
                await _weightAdjustmentRepository.InsertAsync(new WeightAdjustment()
                {
                    Amount = totalSurchargePrice,
                    DateTime = DateTime.UtcNow,
                    Description = "Post Check Under Declare",
                    ReferenceNo = dispatch.DispatchNo,
                    UserId = 0,
                    Weight = totalSurchargeWeight
                });
                await _weightAdjustmentRepository.GetDbContext().SaveChangesAsync();

                var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                wallet.Balance -= totalSurchargePrice;
                await _walletRepository.UpdateAsync(wallet);
                await _walletRepository.GetDbContext().SaveChangesAsync();
            }

            if (totalRefundPrice < 0)
            {
                await _weightAdjustmentRepository.InsertAsync(new WeightAdjustment()
                {
                    Amount = totalRefundPrice * (-1),
                    DateTime = DateTime.UtcNow,
                    Description = "Post Check Over Declare",
                    ReferenceNo = dispatch.DispatchNo,
                    UserId = 0,
                    Weight = totalRefundWeight
                });
                await _weightAdjustmentRepository.GetDbContext().SaveChangesAsync();

                var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                wallet.Balance -= totalRefundPrice;
                await _walletRepository.UpdateAsync(wallet);
                await _walletRepository.GetDbContext().SaveChangesAsync();
            }

            return true;
        }

        [Consumes("multipart/form-data")]
        public async Task<bool> UploadPostCheck([FromForm] UploadPostCheck input)
        {
            if (input.file == null || input.file.Length == 0) throw new UserFriendlyException("File is no uploaded.");

            DataTable dataTable = await ConvertToDatatable(input.file.OpenReadStream());

            if (dataTable.Rows.Count == 0) throw new UserFriendlyException("No Rows in the Uploaded Excel");

            var random = new Random();

            List<Bag> bags = [];
            foreach (DataRow dr in dataTable.Rows)
            {
                bags.Add(new Bag()
                {
                    BagNo = dr.ItemArray[4].ToString(),
                    WeightPost = dr.ItemArray[6] is null ? 0 : Convert.ToDecimal(dr.ItemArray[6]),
                    ItemCountPost = dr.ItemArray[7] is null ? 0 : Convert.ToInt32(dr.ItemArray[7]),
                    WeightVariance = 0,
                });
            }

            int postCheckTotalBags = bags.Count;
            decimal postCheckTotalWeight = bags
                                            .Where(x => !x.WeightPost.Equals(null))
                                            .Where(x => !Convert.ToString(x.WeightPost).Trim().Equals(""))
                                            .Sum(x => Convert.ToDecimal(x.WeightPost));

            var dispatch = await _dispatchRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(input.dispatchNo)) ?? throw new UserFriendlyException("No Dispatch Found");
            var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Code.Equals(dispatch.CustomerCode));
            var dispatchBags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id));
            var dispatchItems = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatch.Id));
            var customerPostal = await _customerPostalRepository.FirstOrDefaultAsync(x => x.AccountNo.Equals(customer.Id) && x.Postal.Equals(dispatch.PostalCode)) ?? throw new UserFriendlyException("No Customer Postal Found with this Customer and Postal Code");
            var rate = await _rateRepository.FirstOrDefaultAsync(x => x.Id.Equals(customerPostal.Rate)) ?? throw new UserFriendlyException("No Rate Found");
            var rateItem = await _rateItemRepository.FirstOrDefaultAsync(x => x.Id.Equals(customerPostal.Rate) && x.ServiceCode.Equals(dispatch.ServiceCode));

            int dispatchID = dispatch.Id;
            string productCode = dispatch.ProductCode;

            dispatch.PostCheckTotalBags = postCheckTotalBags;
            dispatch.PostCheckTotalWeight = postCheckTotalWeight;
            dispatch.Status = 2;

            foreach (Bag bag in bags)
            {
                var dipatchBag = dispatchBags.FirstOrDefault(x => x.BagNo.Equals(bag.BagNo)) ?? throw new UserFriendlyException("Bag not found");
                decimal bagPrecheckWeight = dipatchBag.WeightPre == null ? 0 : dipatchBag.WeightPre.Value;

                bag.WeightVariance = bag.WeightPost >= bagPrecheckWeight ? (bag.WeightPost.Value - bagPrecheckWeight) : 0;
                bag.WeightPost = bag.WeightPost.Value >= bagPrecheckWeight ? bag.WeightPost.Value : bagPrecheckWeight;

                var listItems = dispatchItems.Where(x => x.BagNo.Equals(bag.BagNo)).ToList();

                foreach (var bagItem in listItems)
                {
                    bagItem.DateStage2 = DateTime.Now.AddMilliseconds(random.Next(5000, 60000));

                    await _itemRepository.UpdateAsync(bagItem);
                    await _itemRepository.GetDbContext().SaveChangesAsync();
                }

                await _bagRepository.UpdateAsync(bag);
                await _bagRepository.GetDbContext().SaveChangesAsync();
            }

            var missingBags = await _bagRepository.GetAllListAsync(x =>
                                                    x.DispatchId.Equals(dispatch.Id) &&
                                                    x.WeightPost.Equals(0));

            if (missingBags is not null)
            {
                decimal totalRefund = 0;
                decimal missingWeight = 0;
                var itemsUnderCurrenctDispatch = await _itemRepository.GetAllListAsync(u => u.DispatchID.Equals(dispatch.Id));

                foreach (var missingBag in missingBags)
                {
                    missingWeight = missingBag.WeightVariance.Value;

                    var missingItems = itemsUnderCurrenctDispatch.Where(x => x.BagNo.Equals(missingBag.BagNo)).ToList();
                    if (missingItems is not null)
                    {
                        foreach (var missingItem in missingItems)
                        {
                            totalRefund += missingItem.Price is null ? Convert.ToDecimal(0) : Convert.ToDecimal(missingItem.Price);
                        }
                    }
                }

                if (totalRefund > 0)
                {
                    await _refundRepository.InsertAsync(new Refund()
                    {
                        Amount = totalRefund,
                        DateTime = DateTime.UtcNow,
                        Description = "Post Check Over Declare",
                        ReferenceNo = dispatch.DispatchNo,
                        UserId = 0,
                        Weight = missingWeight
                    });
                    await _refundRepository.GetDbContext().SaveChangesAsync();

                    var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                    wallet.Balance += totalRefund;
                    await _walletRepository.UpdateAsync(wallet);
                    await _walletRepository.GetDbContext().SaveChangesAsync();
                }
            }

            var waBags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id) && !x.WeightVariance.Equals(null));

            decimal totalWeightAdjustmentPrice = 0;
            decimal totalWeightAdjustment = 0;
            decimal totalSurchargePrice = 0;
            decimal totalSurchargeWeight = 0;
            decimal totalRefundPrice = 0;
            decimal totalRefundWeight = 0;

            PriceAndCurrencyId priceAndCurrencyId;

            string rateCardName = rate.CardName;

            if (waBags is null)
            {
                dispatchItems = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatch.Id));

                foreach (var waBag in waBags)
                {
                    int totalItems = dispatchItems.Count(x => x.BagNo.Equals(waBag.BagNo));

                    priceAndCurrencyId = await CalculatePrice(waBag.WeightVariance.Value, waBag.CountryCode, rate.Id, dispatch.ProductCode, dispatch.ServiceCode, totalItems, true);
                    totalWeightAdjustmentPrice = priceAndCurrencyId.Price;
                    totalWeightAdjustment = waBag.WeightVariance.Value;

                    if (totalWeightAdjustmentPrice >= 0)
                    {
                        totalSurchargePrice += totalWeightAdjustmentPrice;
                        totalSurchargeWeight += totalWeightAdjustment;
                        waBag.UnderAmount = totalWeightAdjustmentPrice;
                    }
                    else
                    {
                        totalRefundPrice += totalWeightAdjustmentPrice;
                        totalRefundWeight += totalWeightAdjustment;
                        waBag.UnderAmount = totalWeightAdjustmentPrice * (-1);
                    }
                }


                if (totalSurchargePrice > 0)
                {
                    await _weightAdjustmentRepository.InsertAsync(new WeightAdjustment()
                    {
                        Amount = totalSurchargePrice,
                        DateTime = DateTime.UtcNow,
                        Description = "Post Check Under Declare",
                        ReferenceNo = dispatch.DispatchNo,
                        UserId = 0,
                        Weight = totalSurchargeWeight
                    });
                    await _weightAdjustmentRepository.GetDbContext().SaveChangesAsync();

                    var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                    wallet.Balance -= totalSurchargePrice;
                    await _walletRepository.UpdateAsync(wallet);
                    await _walletRepository.GetDbContext().SaveChangesAsync();
                }


                if (totalRefundPrice < 0)
                {
                    await _weightAdjustmentRepository.InsertAsync(new WeightAdjustment()
                    {
                        Amount = totalRefundPrice * (-1),
                        DateTime = DateTime.UtcNow,
                        Description = "Post Check Over Declare",
                        ReferenceNo = dispatch.DispatchNo,
                        UserId = 0,
                        Weight = totalRefundWeight
                    });
                    await _weightAdjustmentRepository.GetDbContext().SaveChangesAsync();

                    var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                    wallet.Balance -= totalRefundPrice;
                    await _walletRepository.UpdateAsync(wallet);
                    await _walletRepository.GetDbContext().SaveChangesAsync();
                }
            }
            return true;
        }

        public async Task<PagedResultDto<DispatchInfoDto>> GetDispatchInfoListPaged(PagedDispatchResultRequestDto input)
        {
            CheckGetAllPermission();

            var query = CreateFilteredQuery(input);

            var totalCount = await AsyncQueryableExecuter.CountAsync(query);

            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);

            var entities = await AsyncQueryableExecuter.ToListAsync(query);

            List<DispatchInfoDto> result = [];

            foreach (var entity in entities)
            {
                DispatchInfoDto dispatchInfo = new();

                var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Code.Equals(entity.CustomerCode));
                dispatchInfo.CustomerName = customer.CompanyName;
                dispatchInfo.CustomerCode = customer.Code;

                var postal = await _postalRepository.FirstOrDefaultAsync(x =>
                                                        x.PostalCode.Equals(entity.PostalCode) &&
                                                        x.ServiceCode.Equals(entity.ServiceCode) &&
                                                        x.ProductCode.Equals(entity.ProductCode)
                                                    );

                dispatchInfo.PostalCode = postal.PostalCode;
                dispatchInfo.PostalDesc = postal.PostalDesc;

                dispatchInfo.ServiceCode = postal.ServiceCode;
                dispatchInfo.ServiceDesc = postal.ServiceDesc;

                dispatchInfo.ProductCode = postal.ProductCode;
                dispatchInfo.ProductDesc = postal.ProductDesc;

                dispatchInfo.DispatchDate = entity.DispatchDate;
                dispatchInfo.DispatchNo = entity.DispatchNo;

                dispatchInfo.TotalBags = entity.NoofBag;
                dispatchInfo.TotalWeight = entity.TotalWeight;

                var bags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(entity.Id));
                dispatchInfo.TotalCountry = bags.GroupBy(x => x.CountryCode).Count();

                int status = (int)entity.Status;
                switch (status)
                {
                    case 1:
                        dispatchInfo.Status = "Upload Completed";
                        break;
                    case 2:
                        dispatchInfo.Status = "Post Check";
                        break;
                    case 3:
                        dispatchInfo.Status = "CN35 Completed";
                        break;
                    case 4:
                        dispatchInfo.Status = "Leg 1 Completed";
                        break;
                    case 5:
                        dispatchInfo.Status = "Leg 2 Completed";
                        break;
                    case 6:
                        dispatchInfo.Status = "Arrived At Destination";
                        break;
                }

                result.Add(dispatchInfo);

            }


            return new PagedResultDto<DispatchInfoDto>(
                totalCount,
                result
            );
        }
    }
}

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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SND.SMP.IMPCS;
using System.IO.Compression;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using SND.SMP.CustomerTransactions;
using SND.SMP.EWalletTypes;
using SND.SMP.Currencies;
using SND.SMP.DispatchUsedAmounts;
using SND.SMP.DispatchValidations;
using System.Reflection;
using System.Globalization;
using System.Threading;
using SND.SMP.Chibis;
using SND.SMP.ItemTrackingApplications;
using SND.SMP.ItemTrackingReviews;
using SND.SMP.Chibis.Dto;
using SND.SMP.ApplicationSettings;
using Microsoft.EntityFrameworkCore;
using SND.SMP.ItemTrackings;

namespace SND.SMP.Dispatches
{
    public partial class DispatchAppService(
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
        IRepository<Postal, long> postalRepository,
        IRepository<IMPC, int> impcRepository,
        IRepository<CustomerTransaction, long> customerTransactionRepository,
        IRepository<EWalletType, long> ewalletTypeRepository,
        IRepository<Currency, long> currencyRepository,
        IRepository<DispatchUsedAmount, int> dispatchUsedAmountRepository,
        IRepository<DispatchValidation, string> dispatchValidationRepository,
        IRepository<ItemTracking, int> itemTrackingRepository
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
        private readonly IRepository<IMPC, int> _impcRepository = impcRepository;
        private readonly IRepository<CustomerTransaction, long> _customerTransactionRepository = customerTransactionRepository;
        private readonly IRepository<EWalletType, long> _ewalletTypeRepository = ewalletTypeRepository;
        private readonly IRepository<Currency, long> _currencyRepository = currencyRepository;
        private readonly IRepository<DispatchUsedAmount, int> _dispatchUsedAmountRepository = dispatchUsedAmountRepository;
        private readonly IRepository<DispatchValidation, string> _dispatchValidationRepository = dispatchValidationRepository;
        private readonly IRepository<ItemTracking, int> _itemTrackingRepository = itemTrackingRepository;


        [System.Text.RegularExpressions.GeneratedRegex(@"[a-zA-Z]")]
        private static partial System.Text.RegularExpressions.Regex MyRegex();
        private static DataTable ConvertToDatatable(Stream ms)
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
        private static byte[] CreateSLManifestExcelFile(List<SLManifest> dataList)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SL Manifest");
            var properties = typeof(SLManifest).GetProperties();

            for (int col = 0; col < properties.Length; col++)
            {
                System.ComponentModel.DisplayNameAttribute displayName = properties[col].GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
                worksheet.Cells[1, col + 1].Value = displayName is not null ? displayName.DisplayName : properties[col].Name;
            }

            for (int row = 0; row < dataList.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(dataList[row]);
                }
            }
            return package.GetAsByteArray();
        }
        private static byte[] CreateGQManifestExcelFile(List<GQManifest> dataList)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("GQ Manifest");
            var properties = typeof(GQManifest).GetProperties();

            for (int col = 0; col < properties.Length; col++)
            {
                System.ComponentModel.DisplayNameAttribute displayName = properties[col].GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
                worksheet.Cells[1, col + 1].Value = displayName is not null ? displayName.DisplayName : properties[col].Name;
            }

            for (int row = 0; row < dataList.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(dataList[row]);
                }
            }
            return package.GetAsByteArray();
        }
        private static byte[] CreateKGManifestExcelFile(List<KGManifest> dataList)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("KG Manifest");

            var properties = typeof(KGManifest).GetProperties();

            for (int col = 0; col < properties.Length; col++)
            {
                System.ComponentModel.DisplayNameAttribute displayName = properties[col].GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
                worksheet.Cells[1, col + 1].Value = displayName is not null ? displayName.DisplayName : properties[col].Name;
            }

            for (int row = 0; row < dataList.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(dataList[row]);
                }
            }
            return package.GetAsByteArray();
        }
        private static byte[] CreateDOManifestExcelFile(List<DOManifest> dataList)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("DO Manifest");

            var properties = typeof(DOManifest).GetProperties();

            for (int col = 0; col < properties.Length; col++)
            {
                System.ComponentModel.DisplayNameAttribute displayName = properties[col].GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
                worksheet.Cells[1, col + 1].Value = displayName is not null ? displayName.DisplayName : properties[col].Name;
            }

            for (int row = 0; row < dataList.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(dataList[row]);
                }
            }
            return package.GetAsByteArray();
        }
        private static byte[] CreateSLBagExcelFile(List<SLBag> dataList)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SL Bags");
            var properties = typeof(SLBag).GetProperties();

            for (int col = 0; col < properties.Length; col++)
            {
                System.ComponentModel.DisplayNameAttribute displayName = properties[col].GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
                worksheet.Cells[1, col + 1].Value = displayName is not null ? displayName.DisplayName : properties[col].Name;
            }

            for (int row = 0; row < dataList.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(dataList[row]);
                }
            }
            return package.GetAsByteArray();
        }
        private static byte[] CreateGQBagExcelFile(List<GQBag> dataList)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("GQ Bags");
            var properties = typeof(GQBag).GetProperties();

            for (int col = 0; col < properties.Length; col++)
            {
                System.ComponentModel.DisplayNameAttribute displayName = properties[col].GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
                worksheet.Cells[1, col + 1].Value = displayName is not null ? displayName.DisplayName : properties[col].Name;
            }

            for (int row = 0; row < dataList.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(dataList[row]);
                }
            }
            return package.GetAsByteArray();
        }
        private static byte[] CreateKGBagExcelFile(List<KGBag> dataList)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("KG Bags");

            var properties = typeof(KGBag).GetProperties();

            for (int col = 0; col < properties.Length; col++)
            {
                System.ComponentModel.DisplayNameAttribute displayName = properties[col].GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
                worksheet.Cells[1, col + 1].Value = displayName is not null ? displayName.DisplayName : properties[col].Name;
            }

            for (int row = 0; row < dataList.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(dataList[row]);
                }
            }
            return package.GetAsByteArray();
        }
        private static byte[] CreateDOBagExcelFile(List<DOBag> dataList)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("DO Bags");

            var properties = typeof(DOBag).GetProperties();

            for (int col = 0; col < properties.Length; col++)
            {
                System.ComponentModel.DisplayNameAttribute displayName = properties[col].GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
                worksheet.Cells[1, col + 1].Value = displayName is not null ? displayName.DisplayName : properties[col].Name;
            }

            for (int row = 0; row < dataList.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(dataList[row]);
                }
            }
            return package.GetAsByteArray();
        }
        private static string RemoveKGForbiddenKeywords(string description)
        {
            var result = description;

            var listKeywords = new List<string> { "China", "Chinese" };

            foreach (var keyword in listKeywords)
            {
                if (result.Contains(keyword.ToUpper().Trim(), StringComparison.CurrentCultureIgnoreCase))
                {
                    result = result.ToUpper().Replace(keyword.ToUpper().Trim(), "");
                }
            }

            return result;
        }
        private async Task<List<KGManifest>> GetKGManifest(Dispatch dispatch, List<Bag> bags, List<Item> items, List<IMPC> impcs, bool isPreCheckWeight, string countryCode = null)
        {
            List<KGManifest> kgManifest = [];
            countryCode = countryCode.ToUpper().Trim();
            string postalCode = "";

            postalCode = dispatch.PostalCode;

            if (!string.IsNullOrWhiteSpace(countryCode))
                items = items.Where(x => x.CountryCode.Equals(countryCode)).ToList();

            var groupedBags = items.GroupBy(x => x.BagNo).Select(x => x.Key).ToList();

            List<BagWeights> bagWeightsInGram = [];

            bagWeightsInGram = isPreCheckWeight ?
                items.Where(u => u.Weight is not null).GroupBy(u => u.BagNo).Select(u => new BagWeights
                {
                    BagNo = u.Key,
                    Weight = Math.Round(u.Sum(p => Convert.ToDecimal(p.Weight) * 1000), 0)
                }).ToList()
                :
                bags.Select(u => new BagWeights
                {
                    BagNo = u.BagNo,
                    Weight = u.WeightPost == null ? u.WeightPre.Value * 1000 : u.WeightPost.Value * 1000
                }).ToList();

            var random = new Random();
            var tare = 110m;

            var listDeductedTare = GetDeductTare(bags, items, tare, !isPreCheckWeight, 3m, 1);

            var kgc = impcs.FirstOrDefault(p => p.CountryCode.Equals(countryCode));
            var impcToCode = kgc != null ? kgc.IMPCCode : "";
            var logisticCode = kgc != null ? kgc.LogisticCode : "";

            foreach (var u in items)
            {
                var bagNo = groupedBags.IndexOf(u.BagNo) + 1;

                var itemWeightInGram = Math.Round(Convert.ToDecimal(u.Weight) * 1000, 0);
                var bagWeightInGram = bagWeightsInGram.FirstOrDefault(p => p.BagNo.Equals(u.BagNo)).Weight;

                var itemAfterWeight = listDeductedTare.FirstOrDefault(p => p.TrackingNo.Equals(u.Id)).Weight;

                var taxCode = postalCode == "KG01" ? u.Address2 : "";
                var chineseProductName = postalCode == "KG01" ? "" : u.TaxPayMethod;
                var ioss = postalCode == "KG01" ? u.TaxPayMethod : "";
                var poBoxNo = random.Next(300001, 350000);

                #region Tel
                var TelDefault = "36193912";
                var TelNo = u.TelNo;
                var telephone = string.IsNullOrWhiteSpace(MyRegex().Replace(TelNo, "")) ? TelDefault : TelNo;

                var parseTel = double.TryParse(telephone, out double telNo);
                if (parseTel && telNo == 0) telephone = TelDefault;

                int telMinLength = 8;
                if (telephone.Length < telMinLength)
                {
                    telephone = telephone.PadRight(telMinLength, '0');
                }
                #endregion

                kgManifest.Add(new KGManifest
                {
                    Barcode = u.Id,
                    Weight = itemAfterWeight,
                    Attachments_Type = "Other",
                    Sender_Fname = $"PO Box {poBoxNo}",
                    Sender_Lname = "",
                    Sender_Address = u.AddressNo,
                    Sender_City = "Bishkek",
                    Sender_Province = "Kyrgyzstan",
                    Sender_Zip = u.PassportNo,
                    Destination_Fname = u.RecpName,
                    Destination_Lname = "",
                    Destination_Address = u.Address,
                    Destination_City = u.City,
                    Destination_Province = u.State,
                    Destination_Zip = u.Postcode,
                    Destination_Phone = telephone,
                    Attachment_Description = RemoveKGForbiddenKeywords(u.ItemDesc),
                    ChineseProductName = chineseProductName,
                    Attachment_Quantity = u.Qty,
                    Attachment_Weight = itemAfterWeight,
                    Attachment_Price_USD = u.ItemValue is null ? 0 : u.ItemValue,
                    Attachment_Hs_Code = u.HSCode,
                    Attachment_Width = u.Width == null ? 0 : Convert.ToInt32(Math.Round(u.Width.Value, 0)),
                    Attachment_Height = u.Height == null ? 0 : Convert.ToInt32(Math.Round(u.Height.Value, 0)),
                    Attachment_Length = u.Length == null ? 0 : Convert.ToInt32(Math.Round(u.Length.Value, 0)),
                    Bag_Number = bagNo,
                    Impc_To_Code = impcToCode,
                    Bag_Tare_Weight = tare,
                    Bag_Weight = bagWeightInGram,
                    Dispatch_Sent_Date = u.DispatchDate.Value.ToString("dd/MM/yyyy"),
                    Logistic_Code = logisticCode,
                    IOSS = ioss,
                    Tax_Code = taxCode
                });
            }
            return kgManifest;
        }
        private async Task<List<GQManifest>> GetGQManifest(Dispatch dispatch, List<Bag> bags, List<Item> items, List<IMPC> impcs, bool isPreCheckWeight, string countryCode = null)
        {
            List<GQManifest> gqManifest = [];
            countryCode = countryCode.ToUpper().Trim();
            string postalCode = "";

            postalCode = dispatch.PostalCode;

            if (!string.IsNullOrWhiteSpace(countryCode))
                items = items.Where(x => x.CountryCode.Equals(countryCode)).ToList();

            var groupedBags = items.GroupBy(x => x.BagNo).Select(x => x.Key).ToList();

            List<BagWeights> bagWeightsInGram = [];

            bagWeightsInGram = isPreCheckWeight ?
                items.Where(u => u.Weight is not null).GroupBy(u => u.BagNo).Select(u => new BagWeights
                {
                    BagNo = u.Key,
                    Weight = Math.Round(u.Sum(p => Convert.ToDecimal(p.Weight) * 1000), 0)
                }).ToList()
                :
                bags.Select(u => new BagWeights
                {
                    BagNo = u.BagNo,
                    Weight = u.WeightPost == null ? u.WeightPre.Value * 1000 : u.WeightPost.Value * 1000
                }).ToList();

            var random = new Random();
            var tare = 110m;

            var listDeductedTare = GetDeductTare(bags, items, tare, !isPreCheckWeight, 3m, 1);

            var gqc = impcs.FirstOrDefault(p => p.CountryCode.Equals(countryCode));
            string impcToCode = gqc != null ? gqc.IMPCCode : "";
            string logisticCode = gqc != null ? gqc.LogisticCode : "";

            foreach (var u in items)
            {
                var bagNo = groupedBags.IndexOf(u.BagNo) + 1;

                var itemWeightInGram = Math.Round(Convert.ToDecimal(u.Weight) * 1000, 0);
                var bagWeightInGram = bagWeightsInGram.FirstOrDefault(p => p.BagNo == u.BagNo).Weight;

                var itemAfterWeight = listDeductedTare.FirstOrDefault(p => p.TrackingNo.Equals(u.Id)).Weight;

                var telephone = string.IsNullOrWhiteSpace(u.TelNo) ? "87654321" : u.TelNo;
                var chineseProductName = postalCode == "KG02" ? u.TaxPayMethod : "";
                var ioss = string.IsNullOrWhiteSpace(u.TaxPayMethod) ? u.IOSSTax : u.TaxPayMethod;

                #region Tel - Select Randomly
                var willSelectRandomly = false;

                if (!willSelectRandomly)
                {
                    if (MyRegex().IsMatch(telephone))
                    {
                        willSelectRandomly = true;
                    }
                }

                if (!willSelectRandomly)
                {
                    //covers 0, 1, 2, 3, 9, 000000, 111111, 222222, 999999, 18475, 1256, 591
                    var parseResult = long.TryParse(telephone, out long outTel);

                    if (parseResult)
                    {
                        if (outTel.ToString().Length < 6)
                        {
                            willSelectRandomly = true;
                        }
                    }
                }

                if (!willSelectRandomly)
                {
                    //covers 123456, 234567, 345678
                    bool isSeq = "0123456789".Contains(telephone);
                    if (isSeq)
                    {
                        willSelectRandomly = true;
                    }
                }
                #endregion

                gqManifest.Add(new GQManifest
                {
                    Barcode = u.Id,
                    Weight = itemAfterWeight,
                    Attachments_Type = "Other",
                    Sender_Fname = u.IdentityType,
                    Sender_Lname = "",
                    Sender_Address = u.AddressNo,
                    Sender_City = u.PassportNo,
                    Sender_Province = "Equatorial Guinea",
                    Sender_Zip = "",
                    Destination_Fname = u.RecpName,
                    Destination_Lname = "",
                    Destination_Address = u.Address,
                    Destination_City = u.City,
                    Destination_Province = u.State,
                    Destination_Zip = u.Postcode,
                    Destination_Phone = telephone,
                    Attachment_Description = u.ItemDesc,
                    ChineseProductName = chineseProductName,
                    Attachment_Quantity = u.Qty,
                    Attachment_Weight = itemAfterWeight,
                    Attachment_Price_USD = u.ItemValue is null ? 0 : u.ItemValue,
                    Attachment_Hs_Code = u.HSCode,
                    Attachment_Width = u.Width == null ? 0 : Convert.ToInt32(Math.Round(u.Width.Value, 0)),
                    Attachment_Height = u.Height == null ? 0 : Convert.ToInt32(Math.Round(u.Height.Value, 0)),
                    Attachment_Length = u.Length == null ? 0 : Convert.ToInt32(Math.Round(u.Length.Value, 0)),
                    Bag_Number = bagNo,
                    Impc_To_Code = impcToCode,
                    Bag_Tare_Weight = tare,
                    Bag_Weight = bagWeightInGram,
                    Dispatch_Sent_Date = DateTime.Now.ToString("dd/MM/yyyy"),
                    Logistic_Code = logisticCode,
                    IOSS = ioss
                });
            }
            return gqManifest;
        }
        private async Task<List<SLManifest>> GetSLManifest(Dispatch dispatch, List<Bag> bags, List<Item> items, bool isPreCheckWeight)
        {
            List<SLManifest> slManifest = [];

            string postalCode = "";

            postalCode = dispatch.PostalCode;

            var groupedBags = items.GroupBy(x => x.BagNo).Select(x => x.Key).ToList();

            List<BagWeights> bagWeightsInGram = [];

            bagWeightsInGram = isPreCheckWeight ?
                items.Where(u => u.Weight is not null).GroupBy(u => u.BagNo).Select(u => new BagWeights
                {
                    BagNo = u.Key,
                    Weight = Math.Round(u.Sum(p => Convert.ToDecimal(p.Weight) * 1000), 0)
                }).ToList()
                :
                bags.Select(u => new BagWeights
                {
                    BagNo = u.BagNo,
                    Weight = u.WeightPost == null ? u.WeightPre.Value * 1000 : u.WeightPost.Value * 1000
                }).ToList();

            var random = new Random();

            var tare = 110m;
            var listDeductedTare = GetDeductTare(bags, items, tare, !isPreCheckWeight, 3m, 1);

            foreach (var u in items)
            {
                var bagNo = groupedBags.IndexOf(u.BagNo) + 1;

                var itemWeightInGram = Math.Round(Convert.ToDecimal(u.Weight) * 1000, 0);
                var bagWeightInGram = bagWeightsInGram.FirstOrDefault(p => p.BagNo == u.BagNo).Weight;

                var itemAfterWeight = listDeductedTare.FirstOrDefault(p => p.TrackingNo.Equals(u.Id)).Weight;

                var chineseProductName = postalCode == "KG02" ? u.TaxPayMethod : "";

                #region Tel - Select Randomly
                var telephone = string.IsNullOrWhiteSpace(u.TelNo) ? "87654321" : u.TelNo;
                var willSelectRandomly = false;

                if (string.IsNullOrWhiteSpace(telephone))
                {
                    willSelectRandomly = true;
                }

                if (!willSelectRandomly)
                {
                    if (MyRegex().IsMatch(telephone))
                    {
                        willSelectRandomly = true;
                    }
                }

                if (!willSelectRandomly)
                {
                    //covers 0, 1, 2, 3, 9, 000000, 111111, 222222, 999999, 18475, 1256, 591
                    var parseResult = long.TryParse(telephone, out long outTel);

                    if (parseResult)
                    {
                        if (outTel.ToString().Length < 6)
                        {
                            willSelectRandomly = true;
                        }
                    }
                }

                if (!willSelectRandomly)
                {
                    //covers 123456, 234567, 345678
                    bool isSeq = "0123456789".Contains(telephone);
                    if (isSeq)
                    {
                        willSelectRandomly = true;
                    }
                }
                #endregion

                var senderAddresses = new List<SenderAddress>()
                {
                    new() {
                        Name = $"PO Box { random.Next(150000, 199999).ToString() }",
                        Address = "17 Lumley Beach Rd, Freetown, Sierra Leone",
                        City = "Freetown"
                    },
                    new() {
                        Name = $"PO Box { random.Next(150000, 199999).ToString() }",
                        Address = "14B Signal Hill Rd, Freetown, Sierra Leone",
                        City = "Freetown"
                    },
                    new() {
                        Name = $"PO Box { random.Next(150000, 199999).ToString() }",
                        Address = "3 Becklyn Drive, Freetown, Sierra Leone",
                        City = "Freetown"
                    },
                    new() {
                        Name = $"PO Box { random.Next(150000, 199999).ToString() }",
                        Address = "117 main regent road, regent, Freetown, Sierra Leone",
                        City = "Freetown"
                    },
                    new() {
                        Name = $"PO Box { random.Next(150000, 199999).ToString() }",
                        Address = "103, Bo-Kenema Highway, Bo Sierra Leone",
                        City = "Freetown"
                    },
                    new() {
                        Name = $"PO Box { random.Next(150000, 199999).ToString() }",
                        Address = "A41 Resettlement, Koidu, Sierra Leone",
                        City = "Freetown"
                    }
                };

                var ri = random.Next(0, senderAddresses.Count - 1);

                slManifest.Add(new SLManifest
                {
                    Barcode = u.Id,
                    Weight = itemAfterWeight,
                    Attachments_Type = "Other",
                    Sender_Fname = senderAddresses[ri].Name,
                    Sender_Lname = "",
                    Sender_Address = senderAddresses[ri].Address,
                    Sender_City = senderAddresses[ri].City,
                    Sender_Province = "Sierra Leone",
                    Sender_Zip = "",
                    Destination_Fname = u.RecpName,
                    Destination_Lname = "",
                    Destination_Address = u.Address,
                    Destination_City = u.City,
                    Destination_Province = u.State,
                    Destination_Zip = u.Postcode,
                    Destination_Phone = telephone,
                    Attachment_Description = u.ItemDesc,
                    ChineseProductName = chineseProductName,
                    Attachment_Quantity = u.Qty,
                    Attachment_Weight = itemAfterWeight,
                    Attachment_Price_USD = u.ItemValue is null ? 0 : u.ItemValue,
                    Attachment_Hs_Code = u.HSCode,
                    Attachment_Width = u.Width == null ? 0 : Convert.ToInt32(Math.Round(u.Width.Value, 0)),
                    Attachment_Height = u.Height == null ? 0 : Convert.ToInt32(Math.Round(u.Height.Value, 0)),
                    Attachment_Length = u.Length == null ? 0 : Convert.ToInt32(Math.Round(u.Length.Value, 0)),
                    Bag_Number = bagNo,
                    Impc_To_Code = "JPKWSA",
                    Bag_Tare_Weight = tare,
                    Bag_Weight = bagWeightInGram,
                    Dispatch_Sent_Date = DateTime.Now.ToString("dd/MM/yyyy"),
                    Logistic_Code = "KEPBKLGT001285",
                    IOSS = ""
                });
            }
            return slManifest;
        }
        private async Task<List<DOManifest>> GetDOManifest(Dispatch dispatch, List<Bag> bags, List<Item> items, bool isPreCheckWeight, string countryCode = null)
        {
            List<DOManifest> doManifest = [];
            countryCode = countryCode.ToUpper().Trim();

            var bagsGroupedByCountry = bags
                                        .GroupBy(b => b.CountryCode)
                                        .Select(g => new
                                        {
                                            CountryCode = g.Key,
                                            Bags = g.OrderBy(b => b.BagNo)
                                                    .Select((b, index) => new
                                                    {
                                                        b.BagNo,
                                                        BagIndex = index + 1
                                                    })
                                                    .ToList()
                                        })
                                        .ToList();

            if (!string.IsNullOrWhiteSpace(countryCode))
                items = items.Where(x => x.CountryCode.Equals(countryCode)).ToList();

            var shippers = new List<Shipper>
            {
                new() { name = "CM Coordinadora Mercantil, SRL", addr = "Calle Wenceslao Alvarez No. 201, Apto.203", zip = "10153", city = "Distrito Nacional", country = "República Dominicana" },
                new() { name = "SG7 Servicios Generales SRL", addr = "Juan Sanchez Ramirez No. 41", zip = "10103", city = "Distrito Nacional", country = "República Dominicana" },
                new() { name = "Dasom Areun", addr = "Juan Sanchez Ramirez No. 41, Local 1-B", zip = "10103", city = "Distrito Nacional", country = "República Dominicana" },
                new() { name = "Inversiones Tahiti", addr = "Calle Wenceslao Alvarez No. 201, Apto.203", zip = "10153", city = "Distrito Nacional", country = "República Dominicana" }
            };

            var mapping = new List<DOMapping>
            {
                new() { CountryCode = "PR", Origin = "TPE", Destination = "SDQ", Service = "PP105"},
                new() { CountryCode = "US", Origin = "TPE", Destination = "SDQ", Service = "PP105"},
                new() { CountryCode = "VI", Origin = "TPE", Destination = "SDQ", Service = "PP105"},
                new() { CountryCode = "CA", Origin = "TPE", Destination = "SDQ", Service = "PP105"},
                new() { CountryCode = "MU", Origin = "SDQ", Destination = "MRU", Service = "PP101"},
                new() { CountryCode = "MV", Origin = "SDQ", Destination = "MLE", Service = "PP101"},
            };

            var listManifestWeight = GetManifestWeight(bags, items, isPreCheckWeight);

            foreach (var u in items)
            {
                var map = mapping.FirstOrDefault(x => x.CountryCode.Equals(u.CountryCode));

                var itemCountryBags = bagsGroupedByCountry.FirstOrDefault(x => x.CountryCode.Equals(u.CountryCode)).Bags;

                var foundBag = itemCountryBags.FirstOrDefault(x => x.BagNo.Equals(u.BagNo));

                var itemAfterWeight = listManifestWeight.FirstOrDefault(p => p.TrackingNo.Equals(u.Id)).Weight;

                doManifest.Add(new DOManifest()
                {
                    MAWB = "",
                    BagNo = foundBag.BagIndex.ToString(),
                    ETD = "",
                    ETA = "",
                    OrderNo = dispatch.DispatchNo,
                    TrackingNo = u.Id,
                    Origin = map.Origin,
                    Destination = map.Destination,
                    ConsigneeAccNo = "",
                    Consignee = u.RecpName,
                    ConsigneeAddress1 = u.Address,
                    ConsigneeAddress2 = "",
                    ConsigneeAddress3 = "",
                    ConsigneeNeighbourhood = "",
                    ConsigneeCity = u.City,
                    ConsigneeState = "",
                    ConsigneeZip = u.Postcode,
                    ConsigneeCountry = u.CountryCode,
                    ConsigneeEmail = u.Email,
                    ConsigneePhone = u.TelNo,
                    ConsigneeMobile = "",
                    ConsigneeTaxId = "",
                    Pieces = 1,
                    Gweight = Math.Round(itemAfterWeight, 3, MidpointRounding.AwayFromZero),
                    Cweight = "",
                    WeightType = "KG",
                    Height = (int)u.Height.GetValueOrDefault(),
                    Length = (int)u.Length.GetValueOrDefault(),
                    Width = (int)u.Width.GetValueOrDefault(),
                    Commodity = u.ItemDesc,
                    Value = u.ItemValue.GetValueOrDefault(),
                    Freight = "",
                    Currency = "USD",
                    ServiceType = map.Service,
                    ServiceLevel = "DDU",
                    ShipperAccNo = "",
                    ShipperName = u.Address2,
                    ShipperAddress1 = shippers.Where(p => p.name == u.Address2).Select(p => p.addr).FirstOrDefault(),
                    ShipperCity = shippers.Where(p => p.name == u.Address2).Select(p => p.city).FirstOrDefault(),
                    ShipperZip = shippers.Where(p => p.name == u.Address2).Select(p => p.zip).FirstOrDefault(),
                    ShipperCountry = "DO"
                });
            }

            return doManifest;
        }
        private async Task<List<KGBag>> GetKGBag(Dispatch dispatch, List<Bag> bags, List<Item> items, List<IMPC> impcs, bool isPreCheckWeight, string countryCode = null)
        {
            List<KGBag> kgBag = [];

            if (!string.IsNullOrWhiteSpace(countryCode))
                items = items.Where(x => x.CountryCode.Equals(countryCode)).ToList();

            var dispatchDate = dispatch.DispatchDate.GetValueOrDefault().ToString("dd/MM/yyyy");

            List<BagWeights> bagList = [];

            if (isPreCheckWeight)
            {
                bagList = items
                            .Where(u => u.Weight is not null)
                            .GroupBy(u => u.BagNo)
                            .Select(u => new BagWeights
                            {
                                BagNo = u.Key,
                                Weight = Math.Round(u.Sum(p => Convert.ToDecimal(p.Weight) * 1000), 0)
                            })
                            .ToList();
            }
            else
            {
                bagList = bags
                            .Where(u => u.DispatchId.Equals(dispatch.Id))
                            .Select(u => new BagWeights
                            {
                                BagNo = u.BagNo,
                                Weight = u.WeightPost == null ? u.WeightPre.Value * 1000 : u.WeightPost.Value * 1000
                            })
                            .ToList();
            }

            var groupedBags = items.GroupBy(u => u.BagNo).ToList();

            var kgc = impcs.FirstOrDefault(u => u.CountryCode.Equals(countryCode));
            var destination = kgc is null ? "" : kgc.AirportCode;

            for (int i = 0; i < groupedBags.Count; i++)
            {
                var bagNo = groupedBags[i].Key;
                kgBag.Add(new KGBag
                {
                    RunningNo = i + 1,
                    BagNo = bagNo.ToString(),
                    Destination = destination,
                    Qty = groupedBags[i].Count(),
                    Weight = bagList.FirstOrDefault(p => p.BagNo.Equals(groupedBags[i].Key)).Weight,
                    DispatchDate = dispatchDate
                });
            }

            return kgBag;
        }
        private async Task<List<GQBag>> GetGQBag(Dispatch dispatch, List<Bag> bags, List<Item> items, List<IMPC> impcs, bool isPreCheckWeight, string countryCode = null)
        {
            List<GQBag> gqBag = [];

            if (!string.IsNullOrWhiteSpace(countryCode)) items = items.Where(x => x.CountryCode.Equals(countryCode)).ToList();

            var dispatchDate = dispatch.DispatchDate.GetValueOrDefault().ToString("dd/MM/yyyy");

            List<BagWeights> bagList = [];

            if (isPreCheckWeight)
            {
                bagList = items
                            .Where(u => u.Weight is not null)
                            .GroupBy(u => u.BagNo)
                            .Select(u => new BagWeights
                            {
                                BagNo = u.Key,
                                Weight = Math.Round(u.Sum(p => Convert.ToDecimal(p.Weight) * 1000), 0)
                            })
                            .ToList();
            }
            else
            {
                bagList = bags
                            .Where(u => u.DispatchId.Equals(dispatch.Id))
                            .Select(u => new BagWeights
                            {
                                BagNo = u.BagNo,
                                Weight = u.WeightPost == null ? u.WeightPre.Value * 1000 : u.WeightPost.Value * 1000
                            })
                            .ToList();
            }

            var groupedBags = items.GroupBy(u => u.BagNo).ToList();
            var gqc = impcs.FirstOrDefault(u => u.CountryCode.Equals(countryCode));
            var destination = gqc is null ? "" : gqc.AirportCode;

            for (int i = 0; i < groupedBags.Count; i++)
            {
                var bagNo = groupedBags[i].Key;

                gqBag.Add(new GQBag
                {
                    RunningNo = i + 1,
                    BagNo = bagNo.ToString(),
                    Destination = destination,
                    Qty = groupedBags[i].Count(),
                    Weight = bagList.FirstOrDefault(p => p.BagNo.Equals(groupedBags[i].Key)).Weight,
                    DispatchDate = dispatchDate
                });
            }
            return gqBag;
        }
        private List<SLBag> GetSLBag(Dispatch dispatch, List<Bag> bags, List<Item> items, bool isPreCheckWeight)
        {
            List<SLBag> slBag = [];

            var dispatchDate = dispatch.DispatchDate.GetValueOrDefault().ToString("dd/MM/yyyy");

            List<BagWeights> bagList = [];

            if (isPreCheckWeight)
            {
                bagList = items
                            .Where(u => u.Weight is not null)
                            .GroupBy(u => u.BagNo)
                            .Select(u => new BagWeights
                            {
                                BagNo = u.Key,
                                Weight = Math.Round(u.Sum(p => Convert.ToDecimal(p.Weight) * 1000), 0)
                            })
                            .ToList();
            }
            else
            {
                bagList = bags
                            .Where(u => u.DispatchId.Equals(dispatch.Id))
                            .Select(u => new BagWeights
                            {
                                BagNo = u.BagNo,
                                Weight = u.WeightPost == null ? u.WeightPre.Value * 1000 : u.WeightPost.Value * 1000
                            })
                            .ToList();
            }

            var groupedBags = items.GroupBy(u => u.BagNo).ToList();

            for (int i = 0; i < groupedBags.Count; i++)
            {
                var bagNo = groupedBags[i].Key;

                slBag.Add(new SLBag
                {
                    RunningNo = i + 1,
                    BagNo = bagNo.ToString(),
                    Destination = "NRT",
                    Qty = groupedBags[i].Count(),
                    Weight = bagList.FirstOrDefault(p => p.BagNo.Equals(groupedBags[i].Key)).Weight,
                    DispatchDate = dispatchDate
                });
            }
            return slBag;
        }

        /// <summary>
        /// Function to prepare DO Bag list. Keep in mind that DO bag weights are in KG
        /// </summary>
        /// <param name="items"></param>
        /// <param name="dispatch">Entire Dispatch Object</param>
        /// <param name="bags"></param>
        /// <param name="isPreCheckWeight">indicator to get pre-check / post-check weight</param>
        /// <param name="countryCode">nullable countryCode</param>
        /// <returns>
        /// Returns a list of DOBag Object
        /// </returns>
        private List<DOBag> GetDOBag(Dispatch dispatch, List<Bag> bags, List<Item> items, bool isPreCheckWeight, string countryCode = null)
        {
            List<DOBag> doBag = [];

            if (!string.IsNullOrWhiteSpace(countryCode)) items = items.Where(x => x.CountryCode.Equals(countryCode)).ToList();

            var dispatchDate = dispatch.DispatchDate.GetValueOrDefault().ToString("dd/MM/yyyy");

            List<BagWeights> bagList = [];

            if (isPreCheckWeight)
            {
                bagList = items
                            .Where(u => u.Weight is not null)
                            .GroupBy(u => u.BagNo)
                            .Select(u => new BagWeights
                            {
                                BagNo = u.Key,
                                Weight = u.Sum(p => Convert.ToDecimal(p.Weight))
                            })
                            .ToList();
            }
            else
            {
                bagList = bags
                            .Where(u => u.DispatchId.Equals(dispatch.Id))
                            .Select(u => new BagWeights
                            {
                                BagNo = u.BagNo,
                                Weight = u.WeightPost == null ? u.WeightPre.Value * 1000 : u.WeightPost.Value
                            })
                            .ToList();
            }

            var groupedBags = items
                            .GroupBy(u => u.BagNo)
                            .Select(g => new
                            {
                                g.Key,
                                Items = g
                            })
                            .OrderBy(u => u.Key)
                            .ToList();


            var destination = countryCode ?? "";

            for (int i = 0; i < groupedBags.Count; i++)
            {
                var bagNo = groupedBags[i].Key;
                doBag.Add(new DOBag
                {
                    RunningNo = i + 1,
                    BagNo = bagNo.ToString(),
                    Destination = destination,
                    Qty = groupedBags[i].Items.Count(),
                    Weight = Math.Round(bagList.FirstOrDefault(p => p.BagNo.Equals(groupedBags[i].Key)).Weight, 3),
                    DispatchDate = dispatchDate
                });
            }

            return doBag;
        }
        private List<DeductTare> GetDeductTare(List<Bag> task_bags, List<Item> task_items, decimal deductAmount, bool usePostCheckWeight = true, decimal minWeight = 3, decimal deductFactor = 1)
        {
            var bags = task_bags.Select(u => new
            {
                u.BagNo,
                u.WeightPre,
                u.WeightPost
            }).ToList();

            var items = task_items.Select(u => new
            {
                u.Id,
                u.BagNo,
                u.Weight
            }).ToList();

            var result = items.Select(u => new DeductTare
            {
                TrackingNo = u.Id,
                BagNo = u.BagNo,
                Weight = u.Weight.GetValueOrDefault() * 1000
            }).ToList();

            foreach (var bag in bags)
            {
                decimal weightBefore = usePostCheckWeight ? bag.WeightPost.GetValueOrDefault() * 1000 : bag.WeightPre.GetValueOrDefault() * 1000;
                decimal weightAfter = usePostCheckWeight ? (bag.WeightPost.GetValueOrDefault() * 1000) - deductAmount : (bag.WeightPre.GetValueOrDefault() * 1000) - deductAmount;

                #region Set bag weight after
                _ = result.Where(u => u.BagNo == bag.BagNo).All(u => { u.BagWeightAfter = weightAfter; return true; });
                #endregion

                var bagItems = result.Where(u => u.BagNo.Equals(bag.BagNo));
                var bagItemsWeight = bagItems.Sum(u => u.Weight);

                decimal totalDeductionRequired = bagItemsWeight - weightAfter;
                decimal totalAdditionRequired = weightAfter - bagItemsWeight;

                var isEnough = false;

                if (totalDeductionRequired > 0)
                {
                    decimal totalDeducted = 0m;

                    do
                    {
                        decimal totalDeductedThisLoop = 0m;

                        foreach (var item in bagItems)
                        {
                            if (item.Weight > minWeight)
                            {
                                item.Weight -= deductFactor;

                                totalDeducted += deductFactor;
                                totalDeductedThisLoop += deductFactor;
                            }

                            if (totalDeducted >= totalDeductionRequired)
                            {
                                isEnough = true;
                                break;
                            }
                        }

                        if (totalDeductedThisLoop == 0)
                        {
                            isEnough = true;
                        }
                    }
                    while (!isEnough);
                }
                else
                {
                    decimal totalAdded = 0m;

                    do
                    {
                        decimal totalAddedThisLoop = 0m;

                        foreach (var item in bagItems)
                        {
                            if (item.Weight > minWeight)
                            {
                                item.Weight += deductFactor;

                                totalAdded += deductFactor;
                                totalAddedThisLoop += deductFactor;
                            }

                            if (totalAdded >= totalAdditionRequired)
                            {
                                isEnough = true;
                                break;
                            }
                        }

                        if (totalAddedThisLoop == 0)
                        {
                            isEnough = true;
                        }
                    }
                    while (!isEnough);
                }

                _ = result
                        .GroupBy(u => new { u.BagNo, u.BagWeightAfter })
                        .Where(u => u.Sum(p => p.Weight) != u.Key.BagWeightAfter)
                        .All(u => { var s = u.Select(p => p.IsEnough).First(); s = false; return true; });
            }

            return result;
        }
        private List<ManifestWeight> GetManifestWeight(List<Bag> bags, List<Item> items, bool isPreCheckWeight)
        {
            var result = items.Select(u => new ManifestWeight
            {
                TrackingNo = u.Id,
                BagNo = u.BagNo,
                Weight = u.Weight.GetValueOrDefault(),
            }).ToList();

            if (!isPreCheckWeight)
            {
                foreach (var bag in bags)
                {
                    decimal weightVariance = bag.WeightVariance ?? 0m;
                    var bagItems = result.Where(u => u.BagNo.Equals(bag.BagNo));
                    var itemCount = bag.ItemCountPost ?? bag.ItemCountPre;

                    if (weightVariance > 0)
                    {
                        var averageWeight = weightVariance / itemCount;
                        foreach (var item in bagItems)
                        {
                            item.Weight += averageWeight.GetValueOrDefault();
                        }
                    }
                }
            }

            return result;
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
                bag.ItemCountPost = null;
            }

            var items = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatch.Id));
            foreach (var item in items)
            {
                item.DateStage2 = DateTime.MinValue;
                item.Stage2StatusDesc = "";
            }

            var wa_ud = await _weightAdjustmentRepository.FirstOrDefaultAsync(x =>
                                         x.ReferenceNo.Equals(dispatch.DispatchNo) &&
                                         x.Description.Contains("Under Declare")
                                );

            if (wa_ud is not null)
            {
                decimal refundAmount = wa_ud.Amount.Equals(null) ? 0 : wa_ud.Amount;

                wa_ud.InvoiceId = 0;
                wa_ud.Description = $"Undid Post Check for Dispatch {dispatchNo}";

                wa_ud = await _weightAdjustmentRepository.UpdateAsync(wa_ud);
                await _weightAdjustmentRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                wallet.Balance += refundAmount;
                await _walletRepository.UpdateAsync(wallet);
                await _walletRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                dispatch.TotalWeight -= Math.Abs(wa_ud.Weight);

                var eWallet = await _ewalletTypeRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.EWalletType));
                var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.Currency));

                var custTransaction = await _customerTransactionRepository.InsertAsync(new CustomerTransaction()
                {
                    Wallet = wallet.Id,
                    Customer = wallet.Customer,
                    PaymentMode = eWallet.Type,
                    Currency = currency.Abbr,
                    TransactionType = "Refund Amount",
                    Amount = Math.Abs(refundAmount),
                    ReferenceNo = dispatch.DispatchNo,
                    Description = $"Credited {currency.Abbr} {decimal.Round(Math.Abs(refundAmount), 2, MidpointRounding.AwayFromZero)} to {wallet.Customer}'s {wallet.Id} Wallet. Remaining {currency.Abbr} {decimal.Round(wallet.Balance, 2, MidpointRounding.AwayFromZero)}.",
                    TransactionDate = DateTime.Now
                });

                var dispatchUsedAmount = await _dispatchUsedAmountRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatch.DispatchNo));

                if (dispatchUsedAmount is not null)
                {
                    dispatchUsedAmount.Amount -= Math.Abs(refundAmount);
                    dispatchUsedAmount.DateTime = DateTime.Now;
                    dispatchUsedAmount.Description = custTransaction.TransactionType;

                    await _dispatchUsedAmountRepository.UpdateAsync(dispatchUsedAmount).ConfigureAwait(false);
                }

            }
            else
            {
                var wa_od = await _weightAdjustmentRepository.FirstOrDefaultAsync(x =>
                                                         x.ReferenceNo.Equals(dispatch.DispatchNo) &&
                                                         x.Description.Contains("Over Declare")
                                                );

                dispatch.TotalWeight += wa_od is null ? 0 : Math.Abs(wa_od.Weight);
            }

            await _dispatchRepository.UpdateAsync(dispatch).ConfigureAwait(false);

            return true;
        }

        public async Task<bool> SavePostCheck(GetPostCheck getPostCheck)
        {
            if (getPostCheck.Bags is not null && getPostCheck.Bags.Count > 0)
            {
                var random = new Random();

                int dispatchId = getPostCheck.Bags[0].DispatchId is null ? 0 : (int)getPostCheck.Bags[0].DispatchId;
                Dispatch dispatch = await _dispatchRepository.FirstOrDefaultAsync(x => x.Id.Equals(dispatchId)) ?? throw new UserFriendlyException("No dispatch found");

                string productCode = dispatch.ProductCode;

                Customer customer = await _customerRepository.FirstOrDefaultAsync(x => x.Code.Equals(getPostCheck.CompanyCode));

                dispatch.ATA = getPostCheck.ATA;
                dispatch.PostCheckTotalBags = getPostCheck.PostCheckNoOfBag;
                dispatch.PostCheckTotalWeight = getPostCheck.PostCheckWeight;
                dispatch.NoofBag = dispatch.PostCheckTotalBags;
                dispatch.TotalWeight = dispatch.PostCheckTotalWeight;
                dispatch.Status = 2;

                await _dispatchRepository.UpdateAsync(dispatch).ConfigureAwait(false);

                var dispatchBags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id));
                var customerPostal = await _customerPostalRepository.FirstOrDefaultAsync(x => x.AccountNo.Equals(customer.Id) && x.Postal.Equals(dispatch.PostalCode)) ?? throw new UserFriendlyException("No Customer Postal Found with this Customer and Postal Code");
                var rate = await _rateRepository.FirstOrDefaultAsync(x => x.Id.Equals(customerPostal.Rate)) ?? throw new UserFriendlyException("No Rate Found");
                var rateItem = await _rateItemRepository.FirstOrDefaultAsync(x => x.Id.Equals(customerPostal.Rate) && x.ServiceCode.Equals(dispatch.ServiceCode));
                var items = await _itemRepository.GetAllListAsync(u => u.DispatchID.Equals(dispatchId));

                var bags = getPostCheck.Bags;

                for (int i = 0; i < dispatchBags.Count; i++)
                {
                    if (dispatchBags[i].WeightPost is null)
                    {
                        var bag = bags.FirstOrDefault(x => x.BagNo.Equals(dispatchBags[i].BagNo));
                        var bagItems = items.Where(x => x.BagNo.Equals(dispatchBags[i].BagNo)).ToList();

                        for (int j = 0; j < bagItems.Count; j++)
                        {
                            bagItems[j].DateStage2 = DateTime.Now.AddMilliseconds(random.Next(5000, 60000));
                            bagItems[j].Stage2StatusDesc = "Post Check";
                        }
                        _itemRepository.GetDbContext().AttachRange(bagItems);
                        _itemRepository.GetDbContext().UpdateRange(bagItems);
                        await _itemRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                        dispatchBags[i].WeightPost = bag.WeightPost;
                        dispatchBags[i].WeightVariance = bag.WeightVariance;
                        dispatchBags[i].ItemCountPost = bag.ItemCountPre;
                    }
                }
                _bagRepository.GetDbContext().AttachRange(dispatchBags);
                _bagRepository.GetDbContext().UpdateRange(dispatchBags);
                await _bagRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                var missingBags = await _bagRepository.GetAllListAsync(x =>
                                                    x.DispatchId.Equals(dispatchId) &&
                                                    x.WeightPost.Equals(0));

                if (missingBags.Count > 0)
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
                        }).ConfigureAwait(false);

                        var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                        wallet.Balance += totalRefund;
                        await _walletRepository.UpdateAsync(wallet).ConfigureAwait(false);

                        var eWallet = await _ewalletTypeRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.EWalletType));
                        var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.Currency));

                        await _customerTransactionRepository.InsertAsync(new CustomerTransaction()
                        {
                            Wallet = wallet.Id,
                            Customer = wallet.Customer,
                            PaymentMode = eWallet.Type,
                            Currency = currency.Abbr,
                            TransactionType = "Refund Amount",
                            Amount = Math.Abs(totalRefund),
                            ReferenceNo = dispatch.DispatchNo,
                            Description = $"Credited {currency.Abbr} {decimal.Round(Math.Abs(totalRefund), 2, MidpointRounding.AwayFromZero)} to {wallet.Customer}'s {wallet.Id} Wallet. Current Balance is {currency.Abbr} {decimal.Round(wallet.Balance, 2, MidpointRounding.AwayFromZero)}.",
                            TransactionDate = DateTime.Now
                        }).ConfigureAwait(false);

                        var dispatchUsedAmount = await _dispatchUsedAmountRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatch.DispatchNo));

                        if (dispatchUsedAmount is not null)
                        {
                            dispatchUsedAmount.Amount -= Math.Abs(totalRefund);
                            dispatchUsedAmount.DateTime = DateTime.Now;
                            dispatchUsedAmount.Description = "Refund Amount";

                            await _dispatchUsedAmountRepository.UpdateAsync(dispatchUsedAmount).ConfigureAwait(false);
                        }
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

                if (waBags is not null)
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
                            Amount = Math.Abs(totalSurchargePrice),
                            DateTime = DateTime.UtcNow,
                            Description = "Post Check Under Declare",
                            ReferenceNo = dispatch.DispatchNo,
                            UserId = 0,
                            Weight = totalSurchargeWeight
                        }).ConfigureAwait(false);

                        var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                        wallet.Balance -= totalSurchargePrice;
                        await _walletRepository.UpdateAsync(wallet).ConfigureAwait(false);

                        var eWallet = await _ewalletTypeRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.EWalletType));
                        var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.Currency));

                        await _customerTransactionRepository.InsertAsync(new CustomerTransaction()
                        {
                            Wallet = wallet.Id,
                            Customer = wallet.Customer,
                            PaymentMode = eWallet.Type,
                            Currency = currency.Abbr,
                            TransactionType = "Surcharge Amount",
                            Amount = -totalSurchargePrice,
                            ReferenceNo = dispatch.DispatchNo,
                            Description = $"Deducted {currency.Abbr} {decimal.Round(Math.Abs(totalSurchargePrice), 2, MidpointRounding.AwayFromZero)} from {wallet.Customer}'s {wallet.Id} Wallet. Remaining {currency.Abbr} {decimal.Round(wallet.Balance, 2, MidpointRounding.AwayFromZero)}.",
                            TransactionDate = DateTime.Now
                        }).ConfigureAwait(false);

                        var dispatchUsedAmount = await _dispatchUsedAmountRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatch.DispatchNo));

                        if (dispatchUsedAmount is not null)
                        {
                            dispatchUsedAmount.Amount += Math.Abs(totalSurchargePrice);
                            dispatchUsedAmount.DateTime = DateTime.Now;
                            dispatchUsedAmount.Description = "Surcharge Amount";

                            await _dispatchUsedAmountRepository.UpdateAsync(dispatchUsedAmount).ConfigureAwait(false);
                        }
                    }

                    if (totalRefundPrice < 0)
                    {
                        await _weightAdjustmentRepository.InsertAsync(new WeightAdjustment()
                        {
                            Amount = Math.Abs(totalRefundPrice),
                            DateTime = DateTime.UtcNow,
                            Description = "Post Check Over Declare",
                            ReferenceNo = dispatch.DispatchNo,
                            UserId = 0,
                            Weight = totalRefundWeight
                        }).ConfigureAwait(false);

                        var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                        wallet.Balance += Math.Abs(totalRefundPrice);
                        await _walletRepository.UpdateAsync(wallet).ConfigureAwait(false);

                        var eWallet = await _ewalletTypeRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.EWalletType));
                        var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.Currency));

                        await _customerTransactionRepository.InsertAsync(new CustomerTransaction()
                        {
                            Wallet = wallet.Id,
                            Customer = wallet.Customer,
                            PaymentMode = eWallet.Type,
                            Currency = currency.Abbr,
                            TransactionType = "Refund Amount",
                            Amount = Math.Abs(totalRefundPrice),
                            ReferenceNo = dispatch.DispatchNo,
                            Description = $"Credited {currency.Abbr} {decimal.Round(Math.Abs(totalRefundPrice), 2, MidpointRounding.AwayFromZero)} to {wallet.Customer}'s {wallet.Id} Wallet. Current Balance is {currency.Abbr} {decimal.Round(wallet.Balance, 2, MidpointRounding.AwayFromZero)}.",
                            TransactionDate = DateTime.Now
                        }).ConfigureAwait(false);

                        var dispatchUsedAmount = await _dispatchUsedAmountRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatch.DispatchNo));

                        if (dispatchUsedAmount is not null)
                        {
                            dispatchUsedAmount.Amount -= Math.Abs(totalRefundPrice);
                            dispatchUsedAmount.DateTime = DateTime.Now;
                            dispatchUsedAmount.Description = "Refund Amount";

                            await _dispatchUsedAmountRepository.UpdateAsync(dispatchUsedAmount).ConfigureAwait(false);
                        }
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

            decimal averageWeight = weightGap == 0 ? 0 : (decimal)(weightGap / dispatch.NoofBag);

            var remainingItems = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatch.Id) && x.DateStage2.Equals(null)) ?? throw new UserFriendlyException("No Items with this Dispatch");

            for (int i = 0; i < remainingItems.Count; i++)
            {
                remainingItems[i].DateStage2 = DateTime.Now.AddMilliseconds(random.Next(5000, 60000));
            }
            _itemRepository.GetDbContext().AttachRange(remainingItems);
            _itemRepository.GetDbContext().UpdateRange(remainingItems);
            await _itemRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

            var remainingBags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id)) ?? throw new UserFriendlyException("No Bags with this Dispatch");

            for (int i = 0; i < remainingBags.Count; i++)
            {
                remainingBags[i].WeightPost = (remainingBags[i].WeightPre == null ? 0 : remainingBags[i].WeightPre) + averageWeight;
                remainingBags[i].WeightVariance = averageWeight;
                remainingBags[i].ItemCountPost = remainingBags[i].ItemCountPre;
            }
            _bagRepository.GetDbContext().AttachRange(remainingBags);
            _bagRepository.GetDbContext().UpdateRange(remainingBags);
            await _bagRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

            dispatch.WeightGap = weightGap;
            dispatch.WeightAveraged = averageWeight;
            dispatch.Status = 2;
            dispatch.PostCheckTotalBags = dispatch.NoofBag;
            dispatch.PostCheckTotalWeight = (dispatch.TotalWeight.Equals(null) ? 0 : dispatch.TotalWeight) + weightGap;
            await Repository.UpdateAsync(dispatch).ConfigureAwait(false);

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
            var items = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatch.Id));

            foreach (var bag in weightAdjustmentBags)
            {
                var itemList = items.Where(x => x.BagNo == bag.BagNo).ToList();
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
                    Amount = Math.Abs(totalSurchargePrice),
                    DateTime = DateTime.UtcNow,
                    Description = "Post Check Under Declare",
                    ReferenceNo = dispatch.DispatchNo,
                    UserId = 0,
                    Weight = totalSurchargeWeight
                }).ConfigureAwait(false);

                var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                wallet.Balance -= totalSurchargePrice;
                await _walletRepository.UpdateAsync(wallet).ConfigureAwait(false);

                var eWallet = await _ewalletTypeRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.EWalletType));
                var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.Currency));

                await _customerTransactionRepository.InsertAsync(new CustomerTransaction()
                {
                    Wallet = wallet.Id,
                    Customer = wallet.Customer,
                    PaymentMode = eWallet.Type,
                    Currency = currency.Abbr,
                    TransactionType = "Surcharge Amount",
                    Amount = -totalSurchargePrice,
                    ReferenceNo = dispatch.DispatchNo,
                    Description = $"Deducted {currency.Abbr} {decimal.Round(Math.Abs(totalSurchargePrice), 2, MidpointRounding.AwayFromZero)} from {wallet.Customer}'s {wallet.Id} Wallet. Remaining {currency.Abbr} {decimal.Round(wallet.Balance, 2, MidpointRounding.AwayFromZero)}.",
                    TransactionDate = DateTime.Now
                }).ConfigureAwait(false);

                var dispatchUsedAmount = await _dispatchUsedAmountRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatch.DispatchNo));

                if (dispatchUsedAmount is not null)
                {
                    dispatchUsedAmount.Amount += Math.Abs(totalSurchargePrice);
                    dispatchUsedAmount.DateTime = DateTime.Now;
                    dispatchUsedAmount.Description = "Surcharge Amount";

                    await _dispatchUsedAmountRepository.UpdateAsync(dispatchUsedAmount).ConfigureAwait(false);
                }
            }

            if (totalRefundPrice < 0)
            {
                await _weightAdjustmentRepository.InsertAsync(new WeightAdjustment()
                {
                    Amount = Math.Abs(totalRefundPrice),
                    DateTime = DateTime.UtcNow,
                    Description = "Post Check Over Declare",
                    ReferenceNo = dispatch.DispatchNo,
                    UserId = 0,
                    Weight = totalRefundWeight
                }).ConfigureAwait(false);

                var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                wallet.Balance += Math.Abs(totalRefundPrice);
                await _walletRepository.UpdateAsync(wallet).ConfigureAwait(false);

                var eWallet = await _ewalletTypeRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.EWalletType));
                var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.Currency));

                await _customerTransactionRepository.InsertAsync(new CustomerTransaction()
                {
                    Wallet = wallet.Id,
                    Customer = wallet.Customer,
                    PaymentMode = eWallet.Type,
                    Currency = currency.Abbr,
                    TransactionType = "Refund Amount",
                    Amount = Math.Abs(totalRefundPrice),
                    ReferenceNo = dispatch.DispatchNo,
                    Description = $"Credited {currency.Abbr} {decimal.Round(Math.Abs(totalRefundPrice), 2, MidpointRounding.AwayFromZero)} to {wallet.Customer}'s {wallet.Id} Wallet. Current Balance is {currency.Abbr} {decimal.Round(wallet.Balance, 2, MidpointRounding.AwayFromZero)}.",
                    TransactionDate = DateTime.Now
                }).ConfigureAwait(false);

                var dispatchUsedAmount = await _dispatchUsedAmountRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatch.DispatchNo));

                if (dispatchUsedAmount is not null)
                {
                    dispatchUsedAmount.Amount -= Math.Abs(totalRefundPrice);
                    dispatchUsedAmount.DateTime = DateTime.Now;
                    dispatchUsedAmount.Description = "Refund Amount";

                    await _dispatchUsedAmountRepository.UpdateAsync(dispatchUsedAmount).ConfigureAwait(false);
                }
            }

            return true;
        }

        public async Task<List<DispatchInfoDto>> GetDashboardDispatchInfo(bool isAdmin, int top, string customerCode = null)
        {
            var dispatches = isAdmin ?
                        await Repository.GetAllListAsync() :
                        await Repository.GetAllListAsync(x => x.CustomerCode.Equals(customerCode));

            dispatches = [.. dispatches.Where(x => !x.DispatchNo.Contains("temp", StringComparison.CurrentCultureIgnoreCase))];

            dispatches = [.. dispatches.Where(x => x.IsActive.Equals(true))];

            dispatches = [.. dispatches.OrderByDescending(x => x.Id).Take(top)];

            List<DispatchInfoDto> result = [];

            foreach (var dispatch in dispatches)
            {
                DispatchInfoDto dispatchInfo = new();

                var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Code.Equals(dispatch.CustomerCode));
                dispatchInfo.CustomerName = customer.CompanyName;
                dispatchInfo.CustomerCode = customer.Code;

                var postal = await _postalRepository.FirstOrDefaultAsync(x =>
                                                        x.PostalCode.Equals(dispatch.PostalCode) &&
                                                        x.ServiceCode.Equals(dispatch.ServiceCode) &&
                                                        x.ProductCode.Equals(dispatch.ProductCode)
                                                    );

                dispatchInfo.PostalCode = postal.PostalCode;
                dispatchInfo.PostalDesc = postal.PostalDesc;

                dispatchInfo.ServiceCode = postal.ServiceCode;
                dispatchInfo.ServiceDesc = postal.ServiceDesc;

                dispatchInfo.ProductCode = postal.ProductCode;
                dispatchInfo.ProductDesc = postal.ProductDesc;

                dispatchInfo.DispatchDate = dispatch.DispatchDate;
                dispatchInfo.DispatchNo = dispatch.DispatchNo;

                dispatchInfo.TotalBags = dispatch.NoofBag;
                dispatchInfo.TotalWeight = dispatch.TotalWeight;

                dispatchInfo.ImportProgress = dispatch.ImportProgress ?? 0;

                var bags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id));
                dispatchInfo.TotalCountry = bags.GroupBy(x => x.CountryCode).Count();

                int status = dispatch.Status ?? 0;
                dispatchInfo.Status = status switch
                {
                    1 => "Upload Completed",
                    2 => "Post Check",
                    3 => "CN35 Completed",
                    4 => "Leg 1 Completed",
                    5 => "Leg 2 Completed",
                    6 => "Arrived At Destination",
                    _ => $"Stage {status}",
                };
                result.Add(dispatchInfo);
            }

            return result;
        }

        public async Task<PagedResultDto<DispatchInfoDto>> GetDispatchInfoListPaged(PagedDispatchResultRequestDto input)
        {
            CheckGetAllPermission();

            var query = CreateFilteredQuery(input);

            query = query.Where(x => !x.DispatchNo.ToLower().Contains("temp"));
            query = query.Where(x => x.IsActive.Equals(true));

            var totalCount = await AsyncQueryableExecuter.CountAsync(query);

            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);

            query = query.OrderByDescending(x => x.Id);

            var entities = await AsyncQueryableExecuter.ToListAsync(query);

            var dispatchValidation = await _dispatchValidationRepository.GetAllListAsync();

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
                dispatchInfo.TotalWeight = entity.PostCheckTotalWeight is not null ? entity.PostCheckTotalWeight : entity.TotalWeight;

                var bags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(entity.Id));
                dispatchInfo.TotalCountry = bags.GroupBy(x => x.CountryCode).Count();

                int status = (int)entity.Status;
                dispatchInfo.Status = status switch
                {
                    1 => "Upload Completed",
                    2 => "Post Check",
                    3 => "CN35 Completed",
                    4 => "Leg 1 Completed",
                    5 => "Leg 2 Completed",
                    6 => "Arrived At Destination",
                    _ => $"Stage {status}",
                };


                dispatchInfo.Path = dispatchValidation.FirstOrDefault(x => x.DispatchNo.Equals(entity.DispatchNo)).FilePath;

                result.Add(dispatchInfo);

            }


            return new PagedResultDto<DispatchInfoDto>(
                totalCount,
                result
            );
        }

        [HttpGet]
        public async Task<IActionResult> DownloadDispatchManifest(string dispatchNo, bool isPreCheckWeight)
        {
            string code = dispatchNo[..2];

            DateTime dateNowInMYT = DateTime.UtcNow.AddHours(8);
            int currentYear = dateNowInMYT.Year;
            string yearLetter = dateNowInMYT.Year.ToString().Substring(3, 1);
            string sessionID = dateNowInMYT.ToString("yyyyMMddhhmmss");

            var dispatch = await _dispatchRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatchNo));
            var bags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id));
            var items = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatch.Id));
            var countries = bags.GroupBy(x => x.CountryCode).Select(u => u.Key).OrderBy(u => u).ToList();
            var impcs = await _impcRepository.GetAllListAsync(x => x.Type.Equals(code));

            var customerCode = dispatch.CustomerCode;
            var productCode = dispatch.ProductCode;
            var postalCode = dispatch.PostalCode;

            var batchNo = dispatch.DispatchNo.Substring(dispatch.DispatchNo.Length - 3, 3);
            var date = DateTime.Now.ToString("ddMMyy");

            if (code == "KG")
            {
                List<Dictionary<string, List<KGManifest>>> manifestList = [];

                foreach (var country in countries)
                {
                    var model = await GetKGManifest(dispatch, bags, items, impcs, isPreCheckWeight, country);

                    if (model.Count != 0)
                    {
                        var kgc = impcs.FirstOrDefault(u => u.CountryCode.Equals(country));
                        var airportCode = kgc is null ? "" : kgc.AirportCode;

                        if (postalCode.Equals($"{code}02"))
                        {
                            manifestList.Add(new Dictionary<string, List<KGManifest>>() { { $"{airportCode}-{country}", model } });
                        }
                        else
                        {
                            manifestList.Add(new Dictionary<string, List<KGManifest>>() { { $"{airportCode}", model } });
                        }
                    }
                }

                using (MemoryStream zipStream = new())
                {
                    using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create, true))
                    {
                        var airports = manifestList.SelectMany(u => u.Keys).ToList().Distinct().ToList();
                        foreach (var airport in airports)
                        {
                            using (var entryStream = new MemoryStream())
                            {
                                using (var entry = archive.CreateEntry($"XY-{dispatch.ProductCode}-{date}-{batchNo}-{airport}-Manifest.xlsx", System.IO.Compression.CompressionLevel.Optimal).Open())
                                {
                                    byte[] excelBytes = CreateKGManifestExcelFile(manifestList.First(item => item.ContainsKey(airport)).First().Value);
                                    entryStream.Write(excelBytes, 0, excelBytes.Length);
                                    entryStream.Position = 0;
                                    entryStream.CopyTo(entry);
                                }
                            }
                        }
                    }

                    byte[] zipFileBytes = zipStream.ToArray();
                    return new FileContentResult(zipFileBytes, "application/zip")
                    {
                        FileDownloadName = $"{code}Manifest_{sessionID}.zip"
                    };
                }
            }
            else if (code == "GQ")
            {
                List<Dictionary<string, List<GQManifest>>> manifestList = [];

                foreach (var country in countries)
                {
                    var model = await GetGQManifest(dispatch, bags, items, impcs, isPreCheckWeight, country);

                    if (model.Count != 0)
                    {
                        var gqc = impcs.FirstOrDefault(u => u.CountryCode.Equals(country));
                        var airportCode = gqc is null ? "" : gqc.AirportCode;

                        if (postalCode.Equals($"{code}02"))
                        {
                            manifestList.Add(new Dictionary<string, List<GQManifest>>() { { $"{airportCode}-{country}", model } });
                        }
                        else
                        {
                            manifestList.Add(new Dictionary<string, List<GQManifest>>() { { $"{airportCode}", model } });
                        }
                    }
                }
                using (MemoryStream zipStream = new())
                {
                    using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create, true))
                    {
                        var airports = manifestList.SelectMany(u => u.Keys).ToList().Distinct().ToList();
                        foreach (var airport in airports)
                        {
                            using (var entryStream = new MemoryStream())
                            {
                                using (var entry = archive.CreateEntry($"XY-{dispatch.ProductCode}-{date}-{batchNo}-{airport}-Manifest.xlsx", System.IO.Compression.CompressionLevel.Optimal).Open())
                                {
                                    byte[] excelBytes = CreateGQManifestExcelFile(manifestList.First(item => item.ContainsKey(airport)).First().Value);
                                    entryStream.Write(excelBytes, 0, excelBytes.Length);
                                    entryStream.Position = 0;
                                    entryStream.CopyTo(entry);
                                }
                            }
                        }
                    }

                    byte[] zipFileBytes = zipStream.ToArray();
                    return new FileContentResult(zipFileBytes, "application/zip")
                    {
                        FileDownloadName = $"{code}Manifest_{sessionID}.zip"
                    };
                }
            }
            else if (code == "DO")
            {
                List<Dictionary<string, List<DOManifest>>> manifestList = [];

                foreach (var country in countries)
                {
                    var model = await GetDOManifest(dispatch, bags, items, isPreCheckWeight, country);

                    if (model.Count != 0)
                    {
                        manifestList.Add(new Dictionary<string, List<DOManifest>>() { { $"{country}", model } });
                    }
                }

                using (MemoryStream zipStream = new())
                {
                    using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create, true))
                    {
                        var countryDicts = manifestList.SelectMany(u => u.Keys).ToList().Distinct().ToList();
                        foreach (var countryDict in countryDicts)
                        {
                            using (var entryStream = new MemoryStream())
                            {
                                using (var entry = archive.CreateEntry($"XY-{dispatch.ProductCode}-{date}-{batchNo}-{countryDict}-Manifest.xlsx", System.IO.Compression.CompressionLevel.Optimal).Open())
                                {
                                    byte[] excelBytes = CreateDOManifestExcelFile(manifestList.First(item => item.ContainsKey(countryDict)).First().Value);
                                    entryStream.Write(excelBytes, 0, excelBytes.Length);
                                    entryStream.Position = 0;
                                    entryStream.CopyTo(entry);
                                }
                            }
                        }
                    }

                    byte[] zipFileBytes = zipStream.ToArray();
                    return new FileContentResult(zipFileBytes, "application/zip")
                    {
                        FileDownloadName = $"{code}Manifest_{sessionID}.zip"
                    };
                }
            }
            else
            {
                var model = await GetSLManifest(dispatch, bags, items, isPreCheckWeight);

                using (MemoryStream zipStream = new())
                {
                    using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create, true))
                    {
                        using (var entryStream = new MemoryStream())
                        {
                            using (var entry = archive.CreateEntry($"XY-{dispatch.ProductCode}-{date}-{batchNo}-LAX-Manifest.xlsx", System.IO.Compression.CompressionLevel.Optimal).Open())
                            {
                                byte[] excelBytes = CreateSLManifestExcelFile(model);
                                entryStream.Write(excelBytes, 0, excelBytes.Length);
                                entryStream.Position = 0;
                                entryStream.CopyTo(entry);
                            }
                        }
                    }

                    byte[] zipFileBytes = zipStream.ToArray();
                    return new FileContentResult(zipFileBytes, "application/zip")
                    {
                        FileDownloadName = $"{code}Manifest_{sessionID}.zip"
                    };
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadDispatchBag(string dispatchNo, bool isPreCheckWeight)
        {
            string code = dispatchNo[..2];

            DateTime dateNowInMYT = DateTime.UtcNow.AddHours(8);
            int currentYear = dateNowInMYT.Year;
            string yearLetter = dateNowInMYT.Year.ToString().Substring(3, 1);
            string sessionID = dateNowInMYT.ToString("yyyyMMddhhmmss");

            var dispatch = await _dispatchRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatchNo));
            var bags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id));
            var items = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatch.Id));
            var countries = bags.GroupBy(x => x.CountryCode).Select(u => u.Key).OrderBy(u => u).ToList();
            var impcs = await _impcRepository.GetAllListAsync(x => x.Type.Equals($"{code}"));

            var customerCode = dispatch.CustomerCode;
            var productCode = dispatch.ProductCode;
            var postalCode = dispatch.PostalCode;

            var batchNo = dispatch.DispatchNo.Substring(dispatch.DispatchNo.Length - 3, 3);
            var date = DateTime.Now.ToString("ddMMyy");

            if (code == "KG")
            {
                List<Dictionary<string, List<KGBag>>> bagList = [];

                foreach (var country in countries)
                {
                    var model = await GetKGBag(dispatch, bags, items, impcs, isPreCheckWeight, country);

                    if (model.Count != 0)
                    {
                        var kgc = impcs.FirstOrDefault(u => u.CountryCode.Equals(country));
                        var airportCode = kgc is null ? "" : kgc.AirportCode;

                        if (postalCode.Equals($"{code}02"))
                        {
                            bagList.Add(new Dictionary<string, List<KGBag>>() { { $"{airportCode}-{country}", model } });
                        }
                        else
                        {
                            bagList.Add(new Dictionary<string, List<KGBag>>() { { airportCode, model } });
                        }
                    }
                }

                using (MemoryStream zipStream = new())
                {
                    using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create, true))
                    {
                        var airports = bagList.SelectMany(u => u.Keys).ToList().Distinct().ToList();
                        foreach (var airport in airports)
                        {
                            using (var entryStream = new MemoryStream())
                            {
                                using (var entry = archive.CreateEntry($"XY-{dispatch.ProductCode}-{date}-{batchNo}-{airport}-Bag.xlsx", System.IO.Compression.CompressionLevel.Optimal).Open())
                                {
                                    byte[] excelBytes = CreateKGBagExcelFile(bagList.First(item => item.ContainsKey(airport)).First().Value);
                                    entryStream.Write(excelBytes, 0, excelBytes.Length);
                                    entryStream.Position = 0;
                                    entryStream.CopyTo(entry);
                                }
                            }
                        }
                    }

                    byte[] zipFileBytes = zipStream.ToArray();
                    return new FileContentResult(zipFileBytes, "application/zip")
                    {
                        FileDownloadName = $"{code}Bag_{sessionID}.zip"
                    };
                }
            }
            else if (code == "GQ")
            {
                List<Dictionary<string, List<GQBag>>> bagList = [];

                foreach (var country in countries)
                {
                    var model = await GetGQBag(dispatch, bags, items, impcs, isPreCheckWeight, country);

                    if (model.Count != 0)
                    {
                        var gqc = impcs.FirstOrDefault(u => u.CountryCode.Equals(country));
                        var airportCode = gqc is null ? "" : gqc.AirportCode;

                        if (postalCode.Equals($"{code}02"))
                        {
                            bagList.Add(new Dictionary<string, List<GQBag>>() { { $"{airportCode}-{country}", model } });
                        }
                        else
                        {
                            bagList.Add(new Dictionary<string, List<GQBag>>() { { airportCode, model } });
                        }
                    }
                }

                using (MemoryStream zipStream = new())
                {
                    using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create, true))
                    {
                        var airports = bagList.SelectMany(u => u.Keys).ToList().Distinct().ToList();
                        foreach (var airport in airports)
                        {
                            using (var entryStream = new MemoryStream())
                            {
                                using (var entry = archive.CreateEntry($"XY-{dispatch.ProductCode}-{date}-{batchNo}-{airport}-Bag.xlsx", System.IO.Compression.CompressionLevel.Optimal).Open())
                                {
                                    byte[] excelBytes = CreateGQBagExcelFile(bagList.First(item => item.ContainsKey(airport)).First().Value);
                                    entryStream.Write(excelBytes, 0, excelBytes.Length);
                                    entryStream.Position = 0;
                                    entryStream.CopyTo(entry);
                                }
                            }
                        }
                    }

                    byte[] zipFileBytes = zipStream.ToArray();
                    return new FileContentResult(zipFileBytes, "application/zip")
                    {
                        FileDownloadName = $"{code}Bag_{sessionID}.zip"
                    };
                }
            }
            else if (code == "DO")
            {
                List<Dictionary<string, List<DOBag>>> bagList = [];

                foreach (var country in countries)
                {
                    var model = GetDOBag(dispatch, bags, items, isPreCheckWeight, country);

                    if (model.Count != 0)
                    {
                        bagList.Add(new Dictionary<string, List<DOBag>>() { { country, model } });
                    }
                }

                using (MemoryStream zipStream = new())
                {
                    using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create, true))
                    {
                        var countryList = bagList.SelectMany(u => u.Keys).ToList().Distinct().ToList();
                        foreach (var country in countryList)
                        {
                            using (var entryStream = new MemoryStream())
                            {
                                using (var entry = archive.CreateEntry($"XY-{dispatch.ProductCode}-{date}-{batchNo}-{country}-Bag.xlsx", System.IO.Compression.CompressionLevel.Optimal).Open())
                                {
                                    byte[] excelBytes = CreateDOBagExcelFile(bagList.First(item => item.ContainsKey(country)).First().Value);
                                    entryStream.Write(excelBytes, 0, excelBytes.Length);
                                    entryStream.Position = 0;
                                    entryStream.CopyTo(entry);
                                }
                            }
                        }
                    }

                    byte[] zipFileBytes = zipStream.ToArray();
                    return new FileContentResult(zipFileBytes, "application/zip")
                    {
                        FileDownloadName = $"{code}Bag_{sessionID}.zip"
                    };
                }
            }
            else
            {
                var model = GetSLBag(dispatch, bags, items, isPreCheckWeight);

                using (MemoryStream zipStream = new())
                {
                    using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create, true))
                    {
                        using (var entryStream = new MemoryStream())
                        {
                            using (var entry = archive.CreateEntry($"XY-{dispatch.ProductCode}-{date}-{batchNo}-LAX-Manifest.xlsx", System.IO.Compression.CompressionLevel.Optimal).Open())
                            {
                                byte[] excelBytes = CreateSLBagExcelFile(model);
                                entryStream.Write(excelBytes, 0, excelBytes.Length);
                                entryStream.Position = 0;
                                entryStream.CopyTo(entry);
                            }
                        }
                    }

                    byte[] zipFileBytes = zipStream.ToArray();
                    return new FileContentResult(zipFileBytes, "application/zip")
                    {
                        FileDownloadName = $"{code}Bag_{sessionID}.zip"
                    };
                }
            }
        }

        [Consumes("multipart/form-data")]
        public async Task<GetPostCheck> UploadPostCheckForDisplay([FromForm] UploadPostCheck input)
        {
            if (input.file == null || input.file.Length == 0) throw new UserFriendlyException("File is no uploaded.");

            DataTable dataTable = ConvertToDatatable(input.file.OpenReadStream());

            if (dataTable.Rows.Count == 0) throw new UserFriendlyException("No Rows in the Uploaded Excel");

            var random = new Random();

            var dispatch = await Repository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(input.dispatchNo)) ?? throw new UserFriendlyException("Dispatch Not Found.");

            var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Code.Equals(dispatch.CustomerCode)) ?? throw new UserFriendlyException("Customer Not Found.");

            var bags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id)) ?? throw new UserFriendlyException("No Bags Found.");

            List<Bag> bagList = [];
            decimal postCheckWeight = 0m;
            foreach (DataRow dr in dataTable.Rows)
            {
                if (dr.ItemArray[4].ToString() != "")
                {
                    var foundBag = bags.FirstOrDefault(x => x.BagNo.Equals(dr.ItemArray[4].ToString()));
                    decimal weightPost = dr.ItemArray[6] is null ? 0 : Convert.ToDecimal(dr.ItemArray[6]);
                    decimal variance = weightPost - (decimal)foundBag.WeightPre;
                    bagList.Add(new Bag()
                    {
                        BagNo = dr.ItemArray[4].ToString(),
                        DispatchId = dispatch.Id,
                        CountryCode = dr.ItemArray[5].ToString(),
                        WeightPre = foundBag.WeightPre,
                        WeightPost = weightPost,
                        ItemCountPre = foundBag.ItemCountPre,
                        ItemCountPost = dr.ItemArray[7] is null ? 0 : Convert.ToInt32(dr.ItemArray[7]),
                        WeightVariance = variance,
                    });
                    postCheckWeight += weightPost;
                }
            }

            return new GetPostCheck()
            {
                CompanyName = customer.CompanyName ?? "",
                CompanyCode = customer.Code ?? "",
                DispatchNo = input.dispatchNo ?? "",
                FlightTrucking = dispatch.FlightTrucking ?? "",
                ETA = dispatch.ETAtoHKG ?? DateOnly.FromDateTime(DateTime.Now),
                ATA = dispatch.ATA ?? DateTime.MinValue,
                PreCheckNoOfBag = dispatch.NoofBag ?? 0,
                PostCheckNoOfBag = bagList.Count,
                PreCheckWeight = dispatch.TotalWeight ?? Convert.ToDecimal(0),
                PostCheckWeight = postCheckWeight,
                Bags = bagList ?? []
            };
        }

        [Consumes("multipart/form-data")]
        public async Task<bool> UploadPostCheck([FromForm] UploadPostCheck input)
        {
            if (input.file == null || input.file.Length == 0) throw new UserFriendlyException("File is no uploaded.");

            DataTable dataTable = ConvertToDatatable(input.file.OpenReadStream());

            if (dataTable.Rows.Count == 0) throw new UserFriendlyException("No Rows in the Uploaded Excel");

            var random = new Random();

            List<Bag> bags = [];
            foreach (DataRow dr in dataTable.Rows)
            {
                if (dr.ItemArray[4].ToString() != "")
                {
                    bags.Add(new Bag()
                    {
                        BagNo = dr.ItemArray[4].ToString(),
                        WeightPost = dr.ItemArray[6] is null ? 0 : Convert.ToDecimal(dr.ItemArray[6]),
                        ItemCountPost = dr.ItemArray[7] is null ? 0 : Convert.ToInt32(dr.ItemArray[7]),
                        WeightVariance = 0,
                    });
                }
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

            for (int i = 0; i < bags.Count; i++)
            {
                var listItems = dispatchItems.Where(x => x.BagNo.Equals(bags[i].BagNo)).ToList();

                for (int j = 0; j < listItems.Count; j++)
                {
                    listItems[j].DateStage2 = DateTime.Now.AddMilliseconds(random.Next(5000, 60000));
                }
                _itemRepository.GetDbContext().UpdateRange(listItems);
                await _itemRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                var dispatchBag = dispatchBags.FirstOrDefault(x => x.BagNo.Equals(bags[i].BagNo)) ?? throw new UserFriendlyException("Bag not found");
                decimal bagPrecheckWeight = dispatchBag.WeightPre == null ? 0 : dispatchBag.WeightPre.Value;

                dispatchBag.WeightVariance = bags[i].WeightPost >= bagPrecheckWeight ? bags[i].WeightPost.Value - bagPrecheckWeight : 0;
                dispatchBag.WeightPost = bags[i].WeightPost.Value >= bagPrecheckWeight ? bags[i].WeightPost.Value : bagPrecheckWeight;

                bags[i].WeightVariance = dispatchBag.WeightVariance;
                bags[i].WeightPost = dispatchBag.WeightPost;
            }
            _bagRepository.GetDbContext().UpdateRange(bags);
            await _bagRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

            var missingBags = await _bagRepository.GetAllListAsync(x =>
                                                    x.DispatchId.Equals(dispatch.Id) &&
                                                    x.WeightPost.Equals(0));

            if (missingBags.Count > 0)
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
                    await _refundRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                    var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                    wallet.Balance += totalRefund;
                    await _walletRepository.UpdateAsync(wallet);
                    await _walletRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                    var eWallet = await _ewalletTypeRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.EWalletType));
                    var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.Currency));

                    var custTransaction = await _customerTransactionRepository.InsertAsync(new CustomerTransaction()
                    {
                        Wallet = wallet.Id,
                        Customer = wallet.Customer,
                        PaymentMode = eWallet.Type,
                        Currency = currency.Abbr,
                        TransactionType = "Refund Amount",
                        Amount = Math.Abs(totalRefund),
                        ReferenceNo = dispatch.DispatchNo,
                        Description = $"Credited {currency.Abbr} {decimal.Round(Math.Abs(totalRefund), 2, MidpointRounding.AwayFromZero)} to {wallet.Customer}'s {wallet.Id} Wallet. Current Balance is {currency.Abbr} {decimal.Round(wallet.Balance, 2, MidpointRounding.AwayFromZero)}.",
                        TransactionDate = DateTime.Now
                    });

                    var dispatchUsedAmount = await _dispatchUsedAmountRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatch.DispatchNo));

                    if (dispatchUsedAmount is not null)
                    {
                        dispatchUsedAmount.Amount -= Math.Abs(totalRefund);
                        dispatchUsedAmount.DateTime = DateTime.Now;
                        dispatchUsedAmount.Description = custTransaction.TransactionType;

                        await _dispatchUsedAmountRepository.UpdateAsync(dispatchUsedAmount).ConfigureAwait(false);
                    }
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

            if (waBags.Count > 0)
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
                        Amount = Math.Abs(totalSurchargePrice),
                        DateTime = DateTime.UtcNow,
                        Description = "Post Check Under Declare",
                        ReferenceNo = dispatch.DispatchNo,
                        UserId = 0,
                        Weight = totalSurchargeWeight
                    });
                    await _weightAdjustmentRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                    var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                    wallet.Balance -= totalSurchargePrice;
                    await _walletRepository.UpdateAsync(wallet);
                    await _walletRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                    var eWallet = await _ewalletTypeRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.EWalletType));
                    var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.Currency));

                    var custTransaction = await _customerTransactionRepository.InsertAsync(new CustomerTransaction()
                    {
                        Wallet = wallet.Id,
                        Customer = wallet.Customer,
                        PaymentMode = eWallet.Type,
                        Currency = currency.Abbr,
                        TransactionType = "Surcharge Amount",
                        Amount = -totalSurchargePrice,
                        ReferenceNo = dispatch.DispatchNo,
                        Description = $"Deducted {currency.Abbr} {decimal.Round(Math.Abs(totalSurchargePrice), 2, MidpointRounding.AwayFromZero)} from {wallet.Customer}'s {wallet.Id} Wallet. Remaining {currency.Abbr} {decimal.Round(wallet.Balance, 2, MidpointRounding.AwayFromZero)}.",
                        TransactionDate = DateTime.Now
                    });

                    var dispatchUsedAmount = await _dispatchUsedAmountRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatch.DispatchNo));

                    if (dispatchUsedAmount is not null)
                    {
                        dispatchUsedAmount.Amount += Math.Abs(totalSurchargePrice);
                        dispatchUsedAmount.DateTime = DateTime.Now;
                        dispatchUsedAmount.Description = custTransaction.TransactionType;

                        await _dispatchUsedAmountRepository.UpdateAsync(dispatchUsedAmount).ConfigureAwait(false);
                    }
                }


                if (totalRefundPrice < 0)
                {
                    await _weightAdjustmentRepository.InsertAsync(new WeightAdjustment()
                    {
                        Amount = Math.Abs(totalRefundPrice),
                        DateTime = DateTime.UtcNow,
                        Description = "Post Check Over Declare",
                        ReferenceNo = dispatch.DispatchNo,
                        UserId = 0,
                        Weight = totalRefundWeight
                    });
                    await _weightAdjustmentRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                    var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Customer.Equals(customer.Code) && x.Currency.Equals(rateItem.CurrencyId));
                    wallet.Balance += Math.Abs(totalRefundPrice);
                    await _walletRepository.UpdateAsync(wallet);
                    await _walletRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                    var eWallet = await _ewalletTypeRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.EWalletType));
                    var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.Currency));

                    var custTransaction = await _customerTransactionRepository.InsertAsync(new CustomerTransaction()
                    {
                        Wallet = wallet.Id,
                        Customer = wallet.Customer,
                        PaymentMode = eWallet.Type,
                        Currency = currency.Abbr,
                        TransactionType = "Refund Amount",
                        Amount = Math.Abs(totalRefundPrice),
                        ReferenceNo = dispatch.DispatchNo,
                        Description = $"Credited {currency.Abbr} {decimal.Round(Math.Abs(totalRefundPrice), 2, MidpointRounding.AwayFromZero)} to {wallet.Customer}'s {wallet.Id} Wallet. Current Balance is {currency.Abbr} {decimal.Round(wallet.Balance, 2, MidpointRounding.AwayFromZero)}.",
                        TransactionDate = DateTime.Now
                    });

                    var dispatchUsedAmount = await _dispatchUsedAmountRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatch.DispatchNo));

                    if (dispatchUsedAmount is not null)
                    {
                        dispatchUsedAmount.Amount -= Math.Abs(totalRefundPrice);
                        dispatchUsedAmount.DateTime = DateTime.Now;
                        dispatchUsedAmount.Description = custTransaction.TransactionType;

                        await _dispatchUsedAmountRepository.UpdateAsync(dispatchUsedAmount).ConfigureAwait(false);
                    }
                }
            }
            return true;
        }

        public async Task<DispatchTracking> GetDispatchesForTracking(string filter = null, string countryCode = null)
        {
            List<DispatchInfo> result = [];
            List<string> countries = [];

            var dispatches = await Repository
                                    .GetAll()
                                    .Where(x => !x.DispatchNo.Contains("Temp"))
                                    .OrderByDescending(x => x.Id)
                                    .WhereIf(!string.IsNullOrWhiteSpace(filter), x => x.DispatchNo.ToLower().Contains(filter.ToLower()))
                                    .Take(10)
                                    .ToListAsync();

            var bags = await _bagRepository.GetAllListAsync();

            foreach (var dispatch in dispatches)
            {
                DispatchInfo dt = new()
                {
                    Dispatch = dispatch.DispatchNo,
                    DispatchId = dispatch.Id,
                    DispatchDate = $"{dispatch.DispatchDate:dd/MM/yyyy}",
                    PostalCode = dispatch.PostalCode,
                    Status = (int)dispatch.Status,
                    Customer = dispatch.CustomerCode,
                    Open = false,
                };

                List<DispatchCountry> dc = [];

                var dispatchBags = bags.Where(x => x.DispatchId.Equals(dispatch.Id)).ToList();

                var distinctedCountryCode = dispatchBags.DistinctBy(x => x.CountryCode).ToList();

                foreach (var country in distinctedCountryCode) countries.Add(country.CountryCode);

                if (countryCode is not null) distinctedCountryCode = distinctedCountryCode.Where(x => x.CountryCode.Equals(countryCode)).ToList();

                if (distinctedCountryCode.Count != 0)
                {
                    foreach (var country in distinctedCountryCode)
                    {
                        var bagsInCountry = dispatchBags.Where(x => x.CountryCode.Equals(country.CountryCode)).ToList();

                        List<DispatchBag> db = [];
                        int noOfBagsStartedTracking = 0;
                        int maxStagesUpdated = 0;
                        var itemWithMostStagesUpdated = new Item();
                        foreach (var bag in bagsInCountry)
                        {
                            var item = await _itemRepository.FirstOrDefaultAsync(x => x.BagID.Equals(bag.Id));
                            bool hasStartedTracking = HasStartedTracking(item);

                            int stagesUpdated = CountUpdatedStages(item);

                            if (stagesUpdated > maxStagesUpdated)
                            {
                                maxStagesUpdated = stagesUpdated;
                                itemWithMostStagesUpdated = item;
                            }

                            Stage tempStage = new();

                            if (hasStartedTracking)
                            {
                                tempStage = CreateStage(item, dispatch.DispatchNo, country.CountryCode, bag.BagNo);

                                noOfBagsStartedTracking++;
                            }

                            db.Add(new DispatchBag()
                            {
                                BagId = bag.Id,
                                BagNo = bag.BagNo,
                                ItemCount = bag.ItemCountPost == null ? 0 : (int)bag.ItemCountPost,
                                Select = false,
                                Custom = false,
                                Stages = hasStartedTracking ? tempStage : null
                            });
                        }

                        bool allBagsTracked = noOfBagsStartedTracking == dispatchBags.Count;

                        Stage stage = new();
                        if (itemWithMostStagesUpdated.BagID is not null)
                        {
                            Bag majorityBag = bagsInCountry.FirstOrDefault(x => x.Id == itemWithMostStagesUpdated.BagID);
                            stage = CreateStage(itemWithMostStagesUpdated, dispatch.DispatchNo, country.CountryCode, majorityBag.BagNo);
                        }

                        dc.Add(new DispatchCountry()
                        {
                            CountryCode = country.CountryCode,
                            BagCount = bagsInCountry.Count,
                            DispatchBags = db,
                            Select = false,
                            Open = false,
                            Stages = stage
                        });
                    }

                    dt.DispatchCountries = dc;

                    result.Add(dt);
                }
            }



            return new DispatchTracking()
            {
                Dispatches = result,
                Countries = countries.Distinct().ToList()
            };
        }

        public async Task<List<string>> GetDispatchesByCustomer(string customerCode)
        {
            var dispatches = await Repository.GetAllListAsync(x => x.CustomerCode.Equals(customerCode));

            List<string> dispatchesList = [];
            if (dispatches.Count != 0)
            {
                foreach (var dispatch in dispatches)
                {
                    if (!dispatch.DispatchNo.Contains("Temp"))
                    {
                        dispatchesList.Add(dispatch.DispatchNo);
                    }
                }
            }

            return dispatchesList;
        }

        [HttpPost]
        public async Task<ItemWrapper> GetItemsByCurrency(InvoiceDispatches input)
        {
            ItemWrapper itemWrapper = new()
            {
                SurchargeItems = [],
                TotalAmount = 0.0m,
                TotalAmountWithSurcharge = 0.0m,
            };
            var dispatches = await Repository.GetAllListAsync();
            dispatches = dispatches.Where(x => input.Dispatches.Contains(x.DispatchNo)).ToList();
            var dispatches_id = dispatches.Select(x => x.Id).ToList();
            var items = await _itemRepository.GetAllListAsync();
            items = items.Where(x => dispatches_id.Contains((int)x.DispatchID)).ToList();

            //Group by Currency
            var dispatches_grouped_by_currency = dispatches
                .Where(d => !string.IsNullOrEmpty(d.CurrencyId)) // Ensuring CurrencyId is not null or empty
                .GroupBy(d => d.CurrencyId)
                .ToList();

            List<SimplifiedItem> items_by_currency = [];
            foreach (var group in dispatches_grouped_by_currency)
            {
                foreach (var dispatch in group)
                {
                    var dispatch_items = items.Where(x => x.DispatchID == dispatch.Id).ToList();

                    decimal ratePerKG = 0.00m;
                    decimal unitPrice = 0.00m;

                    var bags = await _bagRepository.GetAllListAsync(x => x.DispatchId == dispatch.Id);

                    if (input.GenerateBy.Equals(3)) //By Items
                    {
                        items_by_currency.AddRange(dispatch_items.Select(x =>
                        {
                            if (dispatch.ServiceCode == "TS")
                            {
                                // TS Rates
                                var rate_items = _rateItemRepository.GetAllList(x => x.ProductCode == dispatch.ProductCode);

                                return new SimplifiedItem()
                                {
                                    DispatchNo = dispatch.DispatchNo,
                                    Weight = (decimal)x.Weight,
                                    Country = x.CountryCode,
                                    Identifier = x.Id,
                                    Rate = (decimal)(rate_items.FirstOrDefault(z => z.CountryCode == x.CountryCode) is null ? 0.00m : rate_items.FirstOrDefault(z => z.CountryCode == x.CountryCode).Total),
                                    Quantity = 1,
                                    UnitPrice = (decimal)(rate_items.FirstOrDefault(z => z.CountryCode == x.CountryCode) is null ? 0.00m : rate_items.FirstOrDefault(z => z.CountryCode == x.CountryCode).Fee),
                                    Amount = (decimal)x.Price,
                                    ProductCode = x.ProductCode,
                                    Currency = group.Key,
                                };
                            }
                            else
                            {
                                // DE Rates
                                return new SimplifiedItem()
                                {
                                    DispatchNo = dispatch.DispatchNo,
                                    Weight = (decimal)x.Weight,
                                    Country = x.CountryCode,
                                    Identifier = x.Id,
                                    Rate = ratePerKG,
                                    Quantity = 1,
                                    UnitPrice = unitPrice,
                                    Amount = (decimal)x.Price,
                                    ProductCode = x.ProductCode,
                                    Currency = group.Key,
                                };
                            }
                        }));

                        itemWrapper.TotalAmount += items_by_currency.Sum(x => x.Amount);
                    }
                    else if (input.GenerateBy.Equals(2)) //By Bags
                    {
                        items_by_currency.AddRange(dispatch_items.GroupBy(x => x.BagNo).Select(y =>
                        {
                            var country_code = y.First().CountryCode;
                            var under_amount = bags.FirstOrDefault(x => x.BagNo == y.Key).UnderAmount ?? 0.00m;
                            var weight_variance = bags.FirstOrDefault(x => x.BagNo == y.Key).WeightVariance ?? 0.00m;
                            var bag_country_code = bags.FirstOrDefault(x => x.BagNo == y.Key).CountryCode ?? "";

                            if (!string.IsNullOrWhiteSpace(bag_country_code))
                            {
                                if (dispatch.ServiceCode == "TS")
                                {
                                    // TS Rates
                                    var rate_items = _rateItemRepository.FirstOrDefault(x => x.ProductCode == dispatch.ProductCode &&
                                                                             x.CountryCode == bag_country_code);

                                    ratePerKG = rate_items.Total;
                                    unitPrice = rate_items.Fee;
                                }
                                else
                                {
                                    // DE Rates
                                }
                            }


                            return new SimplifiedItem()
                            {
                                DispatchNo = dispatch.DispatchNo,
                                Weight = (decimal)(y.Sum(i => i.Weight) + weight_variance),
                                Country = country_code,
                                Identifier = under_amount == 0.00m ? y.Key : y.Key + " +" + under_amount,
                                Rate = ratePerKG,
                                Quantity = y.Count(),
                                UnitPrice = unitPrice,
                                Amount = (decimal)y.Sum(i => i.Price),
                                ProductCode = dispatch.ProductCode,
                                Currency = group.Key,
                            };
                        }));

                        itemWrapper.TotalAmount += items_by_currency.Sum(x => x.Amount);
                    }
                    else //By Dispatch
                    {
                        items_by_currency.AddRange(dispatch_items.GroupBy(x => x.DispatchID).Select(y =>
                        {
                            var country_codes = y.DistinctBy(z => z.CountryCode).ToList();
                            string all_country_code_string = "";
                            for (int i = 0; i < country_codes.Count; i++)
                            {
                                var code = country_codes[i];

                                if (i == items.Count - 1) all_country_code_string += code.CountryCode;
                                else all_country_code_string += code.CountryCode + ", ";
                            }

                            return new SimplifiedItem()
                            {
                                DispatchNo = dispatch.DispatchNo,
                                Weight = (decimal)dispatch.TotalWeight,
                                Country = all_country_code_string,
                                Identifier = dispatch.DispatchNo,
                                Rate = 0.00m,
                                Quantity = (int)dispatch.ItemCount,
                                UnitPrice = 0.00m,
                                Amount = (decimal)dispatch.TotalPrice,
                                ProductCode = dispatch.ProductCode,
                                Currency = group.Key,
                            };
                        }));

                        itemWrapper.TotalAmount += items_by_currency.Sum(x => x.Amount);
                    }

                    itemWrapper.DispatchItems = items_by_currency;

                    var surcharge = await _weightAdjustmentRepository.FirstOrDefaultAsync(u => u.ReferenceNo == dispatch.DispatchNo && u.Description.Contains("Under Declare"));

                    if (surcharge != null)
                    {
                        var surcharge_item = new SimplifiedItem()
                        {
                            DispatchNo = string.Format("{0} under declared {1}KG", surcharge.ReferenceNo, surcharge.Weight.ToString("N3")),
                            Weight = 0.00m,
                            Country = "",
                            Identifier = "",
                            Rate = 0.00m,
                            Quantity = 0,
                            UnitPrice = 0.00m,
                            Amount = surcharge.Amount,
                            ProductCode = "",
                            Currency = group.Key,
                        };

                        itemWrapper.SurchargeItems.Add(surcharge_item);
                        itemWrapper.TotalAmountWithSurcharge += itemWrapper.TotalAmount + surcharge_item.Amount;
                    }
                }
            }

            return itemWrapper;
        }


        public async Task<CustomerDispatchDetails> GetDispatchesByCustomerAndMonth(string customerCode, string monthYear, bool custom)
        {
            List<DispatchDetails> dispatchesList = [];
            var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Code.Equals(customerCode));
            string address =
                customer.CompanyName + "\n" +
                customer.AddressLine1 + "\n" +
                customer.AddressLine2 + "\n" +
                customer.City + "\n" +
                customer.State + ", " + customer.Country;

            if (!custom)
            {
                DateTime startDateTime = DateTime.ParseExact(monthYear, "MMM yyyy", null);
                DateOnly startDate = DateOnly.FromDateTime(startDateTime);
                DateOnly endDate = DateOnly.FromDateTime(startDateTime.AddMonths(1));

                var dispatches = await Repository.GetAllListAsync(x => x.CustomerCode.Equals(customerCode) &&
                                                                       x.DispatchDate >= startDate &&
                                                                       x.DispatchDate < endDate);

                if (dispatches.Count != 0)
                {
                    foreach (var dispatch in dispatches)
                    {
                        if (!dispatch.DispatchNo.Contains("Temp"))
                        {
                            dispatchesList.Add(new DispatchDetails()
                            {
                                Date = (DateOnly)dispatch.DispatchDate,
                                Name = dispatch.DispatchNo,
                                Weight = (decimal)dispatch.TotalWeight,
                                Debit = 0.00m,
                                Credit = (decimal)dispatch.TotalPrice,
                                ItemCount = (int)dispatch.ItemCount,
                            });
                        }
                    }

                    if (dispatchesList.Count != 0)
                    {
                        var all_weightAdjustments = await _weightAdjustmentRepository.GetAllListAsync();
                        var weightAdjustments = all_weightAdjustments.Where(x => dispatchesList.Any(y => y.Name.Equals(x.ReferenceNo))).ToList();

                        if (weightAdjustments.Count != 0)
                        {
                            foreach (var weightAdjustment in weightAdjustments)
                            {
                                dispatchesList.Add(new DispatchDetails()
                                {
                                    Date = DateOnly.FromDateTime(weightAdjustment.DateTime),
                                    Name = weightAdjustment.ReferenceNo,
                                    Weight = 0.000m,
                                    Debit = weightAdjustment.Amount,
                                    Credit = 0.00m,
                                    ItemCount = 0
                                });
                            }
                        }
                    }
                }
            }
            return new CustomerDispatchDetails(){
                Details = dispatchesList,
                Address = address
            };
        }

        private static Stage CreateStage(Item item, string dispatchNo, string countryCode, string bagNo)
        {
            return new Stage()
            {
                DispatchNo = dispatchNo,
                CountryCode = countryCode,
                BagNo = bagNo,
                Stage1Desc = item.Stage1StatusDesc,
                Stage1DateTime = item.DateStage1 ?? DateTime.MinValue,
                Stage2Desc = item.Stage2StatusDesc,
                Stage2DateTime = item.DateStage2 ?? DateTime.MinValue,
                Stage3Desc = item.Stage3StatusDesc,
                Stage3DateTime = item.DateStage3 ?? DateTime.MinValue,
                Stage4Desc = item.Stage4StatusDesc,
                Stage4DateTime = item.DateStage4 ?? DateTime.MinValue,
                Stage5Desc = item.Stage5StatusDesc,
                Stage5DateTime = item.DateStage5 ?? DateTime.MinValue,
                Stage6Desc = item.Stage6StatusDesc,
                Stage6DateTime = item.DateStage6 ?? DateTime.MinValue,
                Airport = item.Stage7StatusDesc,
                AirportDateTime = item.DateStage7 ?? DateTime.MinValue,
                Stage7Desc = item.Stage8StatusDesc,
                Stage7DateTime = item.DateStage8 ?? DateTime.MinValue
            };
        }

        private static bool HasStartedTracking(Item item)
        {
            bool hasStartedTracking = false;
            if (
                item.DateStage1 != null ||
                item.DateStage2 != null ||
                item.DateStage3 != null ||
                item.DateStage4 != null ||
                item.DateStage5 != null ||
                item.DateStage6 != null ||
                item.DateStage7 != null ||
                item.DateStage8 != null ||
                item.DateStage9 != null
            )
            {
                hasStartedTracking = true;
            }

            return hasStartedTracking;
        }

        private static int CountUpdatedStages(Item item)
        {
            int count = 0;
            if (item.DateStage1 != null) count++;
            if (item.DateStage2 != null) count++;
            if (item.DateStage3 != null) count++;
            if (item.DateStage4 != null) count++;
            if (item.DateStage5 != null) count++;
            if (item.DateStage6 != null) count++;
            if (item.DateStage7 != null) count++;
            if (item.DateStage8 != null) count++;
            return count;
        }
    }
}

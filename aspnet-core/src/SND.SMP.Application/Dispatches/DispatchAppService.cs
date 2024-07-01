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

using ExcelDataReader;

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
        IRepository<DispatchValidation, string> dispatchValidationRepository
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
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet 1");
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
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet 1");
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
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet 1");

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
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet 1");

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
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet 1");
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
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet 1");
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
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet 1");

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
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet 1");

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
        private async Task<List<KGManifest>> GetKGManifest(int dispatchId, Dispatch dispatch, bool isPreCheckWeight, string countryCode = null)
        {
            List<KGManifest> kgManifest = [];
            countryCode = countryCode.ToUpper().Trim();
            string postalCode = "";

            postalCode = dispatch.PostalCode;

            var items = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatchId));

            if (!string.IsNullOrWhiteSpace(countryCode))
                items = items.Where(x => x.CountryCode.Equals(countryCode)).ToList();

            var bags = items.GroupBy(x => x.BagNo).Select(x => x.Key).ToList();

            List<BagWeights> bagWeightsInGram = [];

            if (isPreCheckWeight)
            {
                bagWeightsInGram = items
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
                var _bags = await _bagRepository.GetAllListAsync(u => u.DispatchId.Equals(dispatch.Id));
                bagWeightsInGram = _bags
                    .Select(u => new BagWeights
                    {
                        BagNo = u.BagNo,
                        Weight = u.WeightPost == null ? u.WeightPre.Value * 1000 : u.WeightPost.Value * 1000
                    })
                    .ToList();
            }

            var tare = 110m;
            var random = new Random();

            var listDeductedTare = await GetDeductTare(dispatchId, tare, !isPreCheckWeight, 3m, 1);
            var listKGCos = await _impcRepository.GetAllListAsync(x => x.Type.Equals("KG"));

            var kgc = listKGCos.FirstOrDefault(p => p.CountryCode.Equals(countryCode));
            var impcToCode = kgc != null ? kgc.IMPCCode : "";
            var logisticCode = kgc != null ? kgc.LogisticCode : "";

            foreach (var u in items)
            {
                var bagNo = bags.IndexOf(u.BagNo) + 1;

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
        private async Task<List<GQManifest>> GetGQManifest(int dispatchId, Dispatch dispatch, bool isPreCheckWeight, string countryCode = null)
        {
            List<GQManifest> gqManifest = [];
            countryCode = countryCode.ToUpper().Trim();
            string postalCode = "";

            postalCode = dispatch.PostalCode;

            var items = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatchId));

            if (!string.IsNullOrWhiteSpace(countryCode))
                items = items.Where(x => x.CountryCode.Equals(countryCode)).ToList();

            var bags = items.GroupBy(x => x.BagNo).Select(x => x.Key).ToList();

            List<BagWeights> bagWeightsInGram = [];

            if (isPreCheckWeight)
            {
                bagWeightsInGram = items
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
                var _bags = await _bagRepository.GetAllListAsync();
                bagWeightsInGram = _bags
                    .Where(u => u.DispatchId.Equals(dispatch.Id))
                    .Select(u => new BagWeights
                    {
                        BagNo = u.BagNo,
                        Weight = u.WeightPost == null ? 0 : u.WeightPost.Value * 1000
                    })
                    .ToList();
            }

            var tare = 110m;
            var random = new Random();

            var listDeductedTare = await GetDeductTare(dispatchId, tare, !isPreCheckWeight, 3m, 1);
            var listGQCos = await _impcRepository.GetAllListAsync(x => x.Type.Equals("GQ"));

            var gqc = listGQCos.FirstOrDefault(p => p.CountryCode.Equals(countryCode));
            string impcToCode = gqc != null ? gqc.IMPCCode : "";
            string logisticCode = gqc != null ? gqc.LogisticCode : "";

            foreach (var u in items)
            {
                var bagNo = bags.IndexOf(u.BagNo) + 1;

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
        private async Task<List<SLManifest>> GetSLManifest(int dispatchId, Dispatch dispatch, bool isPreCheckWeight)
        {
            List<SLManifest> slManifest = [];

            string postalCode = "";

            postalCode = dispatch.PostalCode;

            var items = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatchId));

            var bags = items.GroupBy(x => x.BagNo).Select(x => x.Key).ToList();

            List<BagWeights> bagWeightsInGram = [];

            if (isPreCheckWeight)
            {
                bagWeightsInGram = items
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
                var _bags = await _bagRepository.GetAllListAsync();
                bagWeightsInGram = _bags
                    .Where(u => u.DispatchId.Equals(dispatch.Id))
                    .Select(u => new BagWeights
                    {
                        BagNo = u.BagNo,
                        Weight = u.WeightPost == null ? 0 : u.WeightPost.Value * 1000
                    })
                    .ToList();
            }

            var tare = 110m;
            var random = new Random();

            var listDeductedTare = await GetDeductTare(dispatchId, tare, !isPreCheckWeight, 3m, 1);

            foreach (var u in items)
            {
                var bagNo = bags.IndexOf(u.BagNo) + 1;

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
        private async Task<List<DOManifest>> GetDOManifest(int dispatchId, Dispatch dispatch, bool isPreCheckWeight, string countryCode = null)
        {
            List<DOManifest> doManifest = [];
            countryCode = countryCode.ToUpper().Trim();

            var items = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatchId));

            var bags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatchId));

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
                new() { CountryCode = "PR", Origin = "TPE", Destination = "SDQ", Service = "PP-105"},
                new() { CountryCode = "US", Origin = "TPE", Destination = "SDQ", Service = "PP-105"},
                new() { CountryCode = "VI", Origin = "TPE", Destination = "SDQ", Service = "PP-105"},
                new() { CountryCode = "CA", Origin = "TPE", Destination = "SDQ", Service = "PP-105"},
                new() { CountryCode = "MU", Origin = "SDQ", Destination = "MRU", Service = "PP-101"},
                new() { CountryCode = "MV", Origin = "SDQ", Destination = "MLE", Service = "PP-101"},
            };
            
            var tare = 110m;
            var listDeductedTare = await GetDeductTare(dispatchId, tare, !isPreCheckWeight, 3m, 1);

            foreach (var u in items)
            {
                var map = mapping.FirstOrDefault(x => x.CountryCode.Equals(u.CountryCode));

                var itemCountryBags = bagsGroupedByCountry.FirstOrDefault(x => x.CountryCode.Equals(u.CountryCode)).Bags;

                var foundBag = itemCountryBags.FirstOrDefault(x => x.BagNo.Equals(u.BagNo));

                var itemAfterWeight = listDeductedTare.FirstOrDefault(p => p.TrackingNo.Equals(u.Id)).Weight;

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
                    Gweight = itemAfterWeight, //u.Weight.GetValueOrDefault(),
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
        private async Task<List<KGBag>> GetKGBag(int dispatchId, Dispatch dispatch, bool isPreCheckWeight, string countryCode = null)
        {
            List<KGBag> kgBag = [];

            var items = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatchId));

            if (!string.IsNullOrWhiteSpace(countryCode)) items = items.Where(x => x.CountryCode.Equals(countryCode)).ToList();

            var dispatchDate = dispatch.DispatchDate.GetValueOrDefault().ToString("yyyy-MM-dd");

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
                var _bags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id));
                bagList = _bags
                            .Where(u => u.DispatchId.Equals(dispatch.Id))
                            .Select(u => new BagWeights
                            {
                                BagNo = u.BagNo,
                                Weight = u.WeightPost == null ? 0 : u.WeightPost.Value * 1000
                            })
                            .ToList();
            }

            var bags = items.GroupBy(u => u.BagNo).ToList();

            var destination = "";

            var listKGCos = await _impcRepository.GetAllListAsync(x => x.Type.Equals("KG"));
            var kgc = listKGCos.FirstOrDefault(u => u.CountryCode.Equals(countryCode));
            if (kgc is not null) destination = kgc.AirportCode;

            for (int i = 0; i < bags.Count; i++)
            {
                var bagNo = bags[i].Key;
                kgBag.Add(new KGBag
                {
                    RunningNo = i + 1,
                    BagNo = bagNo.ToString(),
                    Destination = destination,
                    Qty = bags[i].Count(),
                    Weight = bagList.FirstOrDefault(p => p.BagNo.Equals(bags[i].Key)).Weight,
                    DispatchDate = dispatchDate
                });
            }

            return kgBag;
        }
        private async Task<List<GQBag>> GetGQBag(int dispatchId, Dispatch dispatch, bool isPreCheckWeight, string countryCode = null)
        {
            List<GQBag> gqBag = [];

            var items = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatchId));

            if (!string.IsNullOrWhiteSpace(countryCode)) items = items.Where(x => x.CountryCode.Equals(countryCode)).ToList();

            var dispatchDate = dispatch.DispatchDate.GetValueOrDefault().ToString("yyyy-MM-dd");

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
                var _bags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id));
                bagList = _bags
                            .Where(u => u.DispatchId.Equals(dispatch.Id))
                            .Select(u => new BagWeights
                            {
                                BagNo = u.BagNo,
                                Weight = u.WeightPost == null ? 0 : u.WeightPost.Value * 1000
                            })
                            .ToList();
            }

            var destination = "";
            var bags = items.GroupBy(u => u.BagNo).ToList();
            var listGQCos = await _impcRepository.GetAllListAsync(x => x.Type.Equals("GQ"));
            var kgc = listGQCos.FirstOrDefault(u => u.CountryCode.Equals(countryCode));
            if (kgc != null) destination = kgc.AirportCode;

            for (int i = 0; i < bags.Count; i++)
            {
                var bagNo = bags[i].Key;

                gqBag.Add(new GQBag
                {
                    RunningNo = i + 1,
                    BagNo = bagNo.ToString(),
                    Destination = destination,
                    Qty = bags[i].Count(),
                    Weight = bagList.FirstOrDefault(p => p.BagNo.Equals(bags[i].Key)).Weight,
                    DispatchDate = dispatchDate
                });
            }
            return gqBag;
        }
        private async Task<List<SLBag>> GetSLBag(int dispatchId, Dispatch dispatch, bool isPreCheckWeight)
        {
            List<SLBag> slBag = [];

            var items = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatchId));

            var dispatchDate = dispatch.DispatchDate.GetValueOrDefault().ToString("yyyy-MM-dd");

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
                var _bags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id));
                bagList = _bags
                            .Where(u => u.DispatchId.Equals(dispatch.Id))
                            .Select(u => new BagWeights
                            {
                                BagNo = u.BagNo,
                                Weight = u.WeightPost == null ? 0 : u.WeightPost.Value * 1000
                            })
                            .ToList();
            }

            var bags = items.GroupBy(u => u.BagNo).ToList();

            for (int i = 0; i < bags.Count; i++)
            {
                var bagNo = bags[i].Key;

                slBag.Add(new SLBag
                {
                    RunningNo = i + 1,
                    BagNo = bagNo.ToString(),
                    Destination = "NRT",
                    Qty = bags[i].Count(),
                    Weight = bagList.FirstOrDefault(p => p.BagNo.Equals(bags[i].Key)).Weight,
                    DispatchDate = dispatchDate
                });
            }
            return slBag;
        }
        /// <summary>
        /// Function to prepare DO Bag list. Keep in mind that DO bag weights are in KG
        /// </summary>
        /// <param name="dispatchId"></param>
        /// <param name="dispatch">Entire Dispatch Object</param>
        /// <param name="isPreCheckWeight">indicator to get pre-check / post-check weight</param>
        /// <param name="countryCode">nullable countryCode</param>
        /// <returns>
        /// Returns a list of DOBag Object
        /// </returns>
        private async Task<List<DOBag>> GetDOBag(int dispatchId, Dispatch dispatch, bool isPreCheckWeight, string countryCode = null)
        {
            List<DOBag> doBag = [];

            var items = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatchId));

            if (!string.IsNullOrWhiteSpace(countryCode)) items = items.Where(x => x.CountryCode.Equals(countryCode)).ToList();

            var dispatchDate = dispatch.DispatchDate.GetValueOrDefault().ToString("yyyy-MM-dd");

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
                var _bags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id));
                bagList = _bags
                            .Where(u => u.DispatchId.Equals(dispatch.Id))
                            .Select(u => new BagWeights
                            {
                                BagNo = u.BagNo,
                                Weight = u.WeightPost == null ? 0 : u.WeightPost.Value
                            })
                            .ToList();
            }

            var bags = items
                            .GroupBy(u => u.BagNo)
                            .Select(g => new
                            {
                                g.Key,
                                Items = g
                            })
                            .OrderBy(u => u.Key)
                            .ToList();


            var destination = countryCode ?? "";

            for (int i = 0; i < bags.Count; i++)
            {
                var bagNo = bags[i].Key;
                doBag.Add(new DOBag
                {
                    RunningNo = i + 1,
                    BagNo = bagNo.ToString(),
                    Destination = destination,
                    Qty = bags[i].Items.Count(),
                    Weight = Math.Round(bagList.FirstOrDefault(p => p.BagNo.Equals(bags[i].Key)).Weight, 3),
                    DispatchDate = dispatchDate
                });
            }

            return doBag;
        }
        private async Task<List<DeductTare>> GetDeductTare(int dispatchId, decimal deductAmount, bool usePostCheckWeight = true, decimal minWeight = 3, decimal deductFactor = 1)
        {
            List<DeductTare> result = [];

            var dispatch = await _dispatchRepository.FirstOrDefaultAsync(u => u.Id.Equals(dispatchId));
            if (dispatch != null)
            {
                var task_bags = await _bagRepository.GetAllListAsync(u => u.DispatchId.Equals(dispatchId));
                var task_items = await _itemRepository.GetAllListAsync(u => u.DispatchID.Equals(dispatchId));

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

                #region Pre-populate result
                result = items.Select(u => new DeductTare
                {
                    TrackingNo = u.Id,
                    BagNo = u.BagNo,
                    Weight = u.Weight.GetValueOrDefault() * 1000
                }).ToList();
                #endregion

                foreach (var bag in bags)
                {
                    decimal weightBefore = bag.WeightPre.GetValueOrDefault() * 1000;
                    decimal weightAfter = (bag.WeightPre.GetValueOrDefault() * 1000) - deductAmount;

                    if (usePostCheckWeight)
                    {
                        weightBefore = bag.WeightPost.GetValueOrDefault() * 1000;
                        weightAfter = (bag.WeightPost.GetValueOrDefault() * 1000) - deductAmount;
                    }

                    #region Set bag weight after
                    _ = result.Where(u => u.BagNo == bag.BagNo).All(u => { u.BagWeightAfter = weightAfter; return true; });
                    #endregion

                    var bagItems = result.Where(u => u.BagNo.Equals(bag.BagNo));
                    var bagItemsWeight = bagItems.Sum(u => u.Weight);

                    var isEnough = false;
                    decimal totalDeductionRequired = bagItemsWeight - weightAfter;
                    decimal totalAdditionRequired = weightAfter - bagItemsWeight;

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
            }

            var g = result
                        .GroupBy(u => new { u.BagNo, u.BagWeightAfter })
                        .Select(u => new { u.Key.BagNo, u.Key.BagWeightAfter, Sum = u.Sum(p => p.Weight) })
                        .ToList();

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
            }

            var items = await _itemRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatch.Id));
            foreach (var item in items)
            {
                item.DateStage2 = DateTime.MinValue;
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
                        }
                        _itemRepository.GetDbContext().AttachRange(bagItems);
                        _itemRepository.GetDbContext().UpdateRange(bagItems);
                        await _itemRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                        dispatchBags[i].WeightPost = bag.WeightPost;
                        dispatchBags[i].WeightVariance = bag.WeightVariance;
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

            var countries = bags.GroupBy(x => x.CountryCode).Select(u => u.Key).OrderBy(u => u).ToList();

            var Cos = await _impcRepository.GetAllListAsync(x => x.Type.Equals($"{code}"));

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
                    var model = await GetKGManifest(dispatch.Id, dispatch, isPreCheckWeight, country);

                    if (model.Count != 0)
                    {
                        var airportCode = "";

                        var kgc = Cos.FirstOrDefault(u => u.CountryCode.Equals(country));
                        if (kgc != null)
                        {
                            airportCode = kgc.AirportCode;
                        }

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
                    var model = await GetGQManifest(dispatch.Id, dispatch, isPreCheckWeight, country);

                    if (model.Count != 0)
                    {
                        var airportCode = "";

                        var kgc = Cos.FirstOrDefault(u => u.CountryCode.Equals(country));
                        if (kgc != null)
                        {
                            airportCode = kgc.AirportCode;
                        }

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
                    var model = await GetDOManifest(dispatch.Id, dispatch, isPreCheckWeight, country);

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
                var model = await GetSLManifest(dispatch.Id, dispatch, isPreCheckWeight);

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

            var countries = bags.GroupBy(x => x.CountryCode).Select(u => u.Key).OrderBy(u => u).ToList();

            var Cos = await _impcRepository.GetAllListAsync(x => x.Type.Equals($"{code}"));

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
                    var model = await GetKGBag(dispatch.Id, dispatch, isPreCheckWeight, country);

                    if (model.Count != 0)
                    {
                        var airportCode = "";

                        var kgc = Cos.FirstOrDefault(u => u.CountryCode.Equals(country));
                        if (kgc != null) airportCode = kgc.AirportCode;

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
                    var model = await GetGQBag(dispatch.Id, dispatch, isPreCheckWeight, country);

                    if (model.Count != 0)
                    {
                        var airportCode = "";

                        var gqc = Cos.FirstOrDefault(u => u.CountryCode.Equals(country));
                        if (gqc != null) airportCode = gqc.AirportCode;

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
                    var model = await GetDOBag(dispatch.Id, dispatch, isPreCheckWeight, country);

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
                var model = await GetSLBag(dispatch.Id, dispatch, isPreCheckWeight);

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

        public static async Task<Stream> GetFileStream(string url)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = Timeout.InfiniteTimeSpan;
            using var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var contentByteArray = await response.Content.ReadAsByteArrayAsync();
                return new MemoryStream(contentByteArray);
            }
            return null;
        }

        public async Task TestExcel(string FilePath)
        {
            var stream = await GetFileStream(FilePath);

            var listItemIds = new List<string>();
            var listBagNos = new List<string>();

            var rowTouched = 0;
            var ran = new Random();
            var milestoneCount = 4;
            var milestones = new List<int>();
            var g = 0;
            var percHistory = new List<int>();

            for (var i = 1; i <= milestoneCount; i++)
            {
                g += 1 * ran.Next(15, 25);
                milestones.Add(g);
            }

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var encoding = System.Text.Encoding.UTF8; // Use UTF-8


            using (var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration()
            {
                FallbackEncoding = encoding,
                AutodetectSeparators = [',', ';', '\t'], // Specify separators based on your file format
                LeaveOpen = false // Close the stream after reading
            }))
            {
                // Read the header row
                reader.Read(); // Move to the first row (header row)
                rowTouched = 1;
                // Read until the end of the file
                while (reader.Read())
                {
                    var strPostalCode = reader.GetValue(reader.GetOrdinal("Postal")).ToString()!;
                    DateTime.TryParseExact(reader.GetValue(reader.GetOrdinal("Dispatch Date")).ToString()!, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTimeCell);
                    var dispatchDate = DateOnly.FromDateTime(dateTimeCell);
                    var strServiceCode = reader.GetValue(reader.GetOrdinal("Service")).ToString()!;
                    var strProductCode = reader.GetValue(reader.GetOrdinal("Product Code")).ToString()!;
                    var bagNo = reader.GetValue(reader.GetOrdinal("Bag No")).ToString()!;
                    var countryCode = reader.GetValue(reader.GetOrdinal("Country")).ToString()!;
                    var weight = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("Weight")));
                    var itemId = reader.GetValue(reader.GetOrdinal("Tracking Number")).ToString()!;
                    var sealNo = reader.GetValue(reader.GetOrdinal("Seal Number")).ToString()!;
                    var strDispatchNo = reader.GetValue(reader.GetOrdinal("Dispatch Name")).ToString()!;
                    var itemValue = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("Item Value")));
                    var itemDesc = reader.GetValue(reader.GetOrdinal("Item Desc")).ToString()!;
                    var recpName = reader.GetValue(reader.GetOrdinal("Recp Name")).ToString()!;
                    var telNo = reader.GetValue(reader.GetOrdinal("Tel No")).ToString()!;
                    var email = reader.GetValue(reader.GetOrdinal("Email")) == null ? "" : reader.GetValue(reader.GetOrdinal("Email")).ToString()!;
                    var address = reader.GetValue(reader.GetOrdinal("Address")).ToString()!;
                    var postcode = reader.GetValue(reader.GetOrdinal("Postcode")).ToString()!;
                    var city = reader.GetValue(reader.GetOrdinal("City")).ToString()!;
                    var addressLine2 = reader.GetValue(reader.GetOrdinal("Address Line 2")) == null ? "" : reader.GetValue(reader.GetOrdinal("Address Line 2")).ToString()!;
                    var addressNo = reader.GetValue(reader.GetOrdinal("Address No")) == null ? "" : reader.GetValue(reader.GetOrdinal("Address No")).ToString()!;
                    var identityNo = reader.GetValue(reader.GetOrdinal("Identity No")) == null ? "" : reader.GetValue(reader.GetOrdinal("Identity No")).ToString()!;
                    var identityType = reader.GetValue(reader.GetOrdinal("Identity Type")) == null ? "" : reader.GetValue(reader.GetOrdinal("Identity Type")).ToString()!;
                    var state = reader.GetValue(reader.GetOrdinal("State")).ToString()!;
                    var length = reader.GetValue(reader.GetOrdinal("Length")) == null ? 0 : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("Length")));
                    var width = reader.GetValue(reader.GetOrdinal("Width")) == null ? 0 : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("Width")));
                    var height = reader.GetValue(reader.GetOrdinal("Height")) == null ? 0 : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("Height")));
                    var taxPaymentMethod = reader.GetValue(reader.GetOrdinal("Tax Payment Method")) == null ? "" : reader.GetValue(reader.GetOrdinal("Tax Payment Method")).ToString()!;
                    var hsCode = reader.GetValue(reader.GetOrdinal("HS Code")) == null ? "" : reader.GetValue(reader.GetOrdinal("HS Code")).ToString()!;
                    var qty = reader.GetValue(reader.GetOrdinal("Qty")) == null ? 0 : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("Qty")));

                    listItemIds.Add(itemId);
                    if (!listBagNos.Contains(bagNo)) listBagNos.Add(bagNo);

                    var blockMilestone = rowTouched % 100;
                    if (blockMilestone == 0)
                    {
                        //Block validation
                        Parallel.Invoke(new ParallelOptions
                        { MaxDegreeOfParallelism = Environment.ProcessorCount },
                            () => Console.WriteLine("Checking"));

                        listItemIds.Clear();
                    }

                    rowTouched++;

                    #region Validation Progress
                    var perc = Convert.ToInt32(Convert.ToDecimal(rowTouched) / Convert.ToDecimal(rowTouched) * 100);

                    if (perc > 0)
                    {
                        if (milestones.Contains(perc) && !percHistory.Contains(perc))
                        {
                            percHistory.Add(perc);

                            Parallel.Invoke(async () =>
                            {
                                Console.WriteLine("Milestone Reached");
                            });
                        }
                    }
                    #endregion
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
    }
}

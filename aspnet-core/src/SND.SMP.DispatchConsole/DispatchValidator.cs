using System;
using ExcelDataReader;
using static SND.SMP.DispatchConsole.WorkerDispatchImport;
using System.Globalization;
using static SND.SMP.Shared.EnumConst;
using SND.SMP.DispatchConsole.Dto;
using Humanizer;
using System.Collections.Generic;
using SND.SMP.DispatchConsole.EF;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using SND.SMP.Chibis;
using SND.SMP.CustomerTransactions;
using SND.SMP.DispatchUsedAmounts;
using System.Text;
using OfficeOpenXml;
using SND.SMP.ItemTrackingReviews;
using Abp.Extensions;
using Abp.Collections.Extensions;

namespace SND.SMP.DispatchConsole
{
    public class DispatchValidator
    {
        private const string SERVICE_TS = "TS";
        private const string SERVICE_DE = "DE";
        private const int ID_LENGTH = 13;
        private const decimal totalWeightLimitForCountry_NG = 30;
        private static readonly string[] expectedHeaders = new string[]
        {
            "Postal", "Dispatch Date", "Service", "Product Code", "Bag No", "Country", "Weight",
            "Tracking Number", "Seal Number", "Dispatch Name", "Item Value", "Item Desc", "Recp Name",
            "Tel No", "Email", "Address", "Postcode", "City", "Address Line 2", "Address No",
            "Identity No.", "Identity Type", "State", "Length", "Width", "Height",
            "Tax Payment Method", "HS Code", "Qty"
        };
        private uint QueueId { get; set; }
        private string FilePath { get; set; }
        private string FileType { get; set; }
        private string CustomerCode { get; set; }
        private string ServiceCode { get; set; }
        private string PostalCode { get; set; }
        private long CurrencyId { get; set; }
        private int BlockSize { get; set; } = 50;

        private int itemRow { get; set; }

        private List<string> ListPostalCountry { get; set; }
        private DispatchProfileDto DispatchProfile { get; set; }

        private string Currency { get; set; }




        public DispatchValidator() { }

        public async Task DiscoverAndValidate(string fileType, int blockSize = 50)
        {
            FileType = fileType;
            BlockSize = blockSize;

            using db db = new();
            var hasRunning = db.Queues
                .Where(u => u.EventType == QueueEnumConst.EVENT_TYPE_DISPATCH_VALIDATE)
                .Where(u => u.Status == QueueEnumConst.STATUS_RUNNING)
                .Any();

            if (hasRunning) return;

            var newTask = db.Queues
                .Where(u => u.EventType == QueueEnumConst.EVENT_TYPE_DISPATCH_VALIDATE)
                .Where(u => u.Status == QueueEnumConst.STATUS_NEW)
                .OrderBy(u => u.DateCreated)
                .FirstOrDefault();

            if (newTask is not null)
            {
                QueueId = newTask.Id;
                FilePath = newTask.FilePath;

                newTask.Status = QueueEnumConst.STATUS_RUNNING;
                newTask.ErrorMsg = null;
                newTask.TookInSec = 0;

                await db.SaveChangesAsync().ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                var fileProfile = "";
                using db chibiDB = new();
                var dispatchFile = chibiDB.Chibis.FirstOrDefault(x => x.URL.Equals(FilePath));
                if (dispatchFile is not null)
                {
                    var dispatchFilePair = chibiDB.Chibis.Where(x => x.OriginalName.Equals(dispatchFile.OriginalName)).ToList();
                    var jsonDispatchFile = dispatchFilePair.FirstOrDefault(x => x.URL.Contains("json"));
                    fileProfile = jsonDispatchFile.URL;
                }
                var fileString = await FileServer.GetFileStreamAsString(fileProfile);

                if (fileString is not null)
                {
                    DispatchProfile = JsonConvert.DeserializeObject<DispatchProfileDto>(fileString);

                    if (DispatchProfile != null)
                    {
                        CustomerCode = DispatchProfile.AccNo;
                        ServiceCode = DispatchProfile.ServiceCode;
                        PostalCode = DispatchProfile.PostalCode;

                        db.Dispatchvalidations
                            .RemoveRange(db.Dispatchvalidations
                            .Where(u => u.DispatchNo == DispatchProfile.DispatchNo));

                        await db.Dispatchvalidations.AddAsync(new EF.Dispatchvalidation
                        {
                            DispatchNo = DispatchProfile.DispatchNo,
                            CustomerCode = DispatchProfile.AccNo,
                            PostalCode = DispatchProfile.PostalCode,
                            ServiceCode = DispatchProfile.ServiceCode,
                            ProductCode = DispatchProfile.ProductCode,
                            DateStarted = DateTime.Now,
                            DateCompleted = DateTime.MinValue,
                            FilePath = FilePath,
                            IsFundLack = 0,
                            IsValid = 0,
                            Status = DispatchValidationEnumConst.STATUS_RUNNING,
                            TookInSec = 0,
                            ValidationProgress = 0
                        }).ConfigureAwait(false);

                        await db.SaveChangesAsync();

                        if (FileType == DispatchEnumConst.ImportFileType.Excel.ToString())
                        {
                            await ValidateFromExcel();
                        }
                    }
                }
            }
        }

        private async Task ValidateFromExcel()
        {
            bool isValid = true;
            bool isFundLack = false;
           
            DateTime dateValidationStart = DateTime.Now;

            List<DispatchValidateDto> validations = [];

            DispatchValidateDto validationResult_dispatch_IsDuplicate = new() { Category = "Duplicated Dispatch No." };
            DispatchValidateDto validationResult_dispatch_IsParticularsNotTally = new() { Category = "Dispatch Particulars Not Tally" };
            DispatchValidateDto validationResult_dispatch_Overweight = new() { Category = "Dispatch Overweight" };
            DispatchValidateDto validationResult_bag_IsDuplicate = new() { Category = "Duplicated Bag No" };
            DispatchValidateDto validationResult_id_IsDuplicate = new() { Category = "Duplicated Item ID" };
            DispatchValidateDto validationResult_id_HasInvalidLength = new() { Category = "Invalid Length" };
            DispatchValidateDto validationResult_id_HasInvalidPrefixSuffix = new() { Category = "Invalid Prefix & Suffix" };
            DispatchValidateDto validationResult_id_HasInvalidCheckDigit = new() { Category = "Invalid Check Digit" };
            DispatchValidateDto validationResult_country_HasInvalidCountry = new() { Category = "Invalid Country Code" };
            DispatchValidateDto validationResult_wallet_InsufficientBalance = new() { Category = "Insufficient Wallet Balance" };
            DispatchValidateDto validationResult_ioss_missing = new() { Category = "Missing IOSS" };
            DispatchValidateDto validationResult_trackingno_IsPreRegistered = new() { Category = "Unregistered Tracking Number(s)" };
            DispatchValidateDto validationResult_trackingno_IsInvalid = new() { Category = "Invalid Tracking Number(s)" };
            DispatchValidateDto validationResult_Others = new() { Category = "Caught Error" };

            using db db = new();
            try
            {
                ListPostalCountry = [.. db.Postalcountries
                    .Where(u => u.PostalCode == PostalCode)
                    .Select(u => u.CountryCode)];

                var dispatchNoDuplicate = db.Dispatches.Where(u => u.DispatchNo == DispatchProfile.DispatchNo).Any();

                if (dispatchNoDuplicate)
                {
                    validationResult_dispatch_IsDuplicate.Message = $"{DispatchProfile.DispatchNo} already existed in system";
                }

                #region Validation

                var stream = await FileServer.GetFileStream(FilePath);

                using var reader = ExcelReaderFactory.CreateReader(stream);

                var rowCount = reader.RowCount;
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

                var pricer = new DispatchPricer(
                                accNo: DispatchProfile.AccNo,
                                postalCode: DispatchProfile.PostalCode,
                                serviceCode: DispatchProfile.ServiceCode,
                                productCode: DispatchProfile.ProductCode,
                                rateOptionId: DispatchProfile.RateOptionId,
                                paymentMode: DispatchProfile.PaymentMode);

                if (pricer.ErrorMsg != "") throw new Exception(pricer.ErrorMsg);

                var currency = await db.Currencies.FirstOrDefaultAsync(c => c.Id == pricer.CurrencyId);
                CurrencyId = currency.Id;
                Currency = currency.Abbr;

                var rowTouched = 0;
                var listItems = new List<DispatchItemDto>();

                var itemCount = 0;
                var totalWeight = 0m;
                var totalPrice = 0m;

                var month = Convert.ToInt32($"{DispatchProfile.DateDispatch.Year}{DispatchProfile.DateDispatch.Month.ToString().PadLeft(2, '0')}");
                var listItemIds = new List<string>();
                var listBagNos = new List<string>();
                var listIOSSChecking = new List<DispatchValidateIOSSDto>();
                var listCountryCodes = new List<DispatchValidateCountryDto>();
                var listParticulars = new List<DispatchValidateParticularsDto>();
                var listItemIdsForUpdate = new List<string>();
                do
                {
                    while (reader.Read())
                    {
                        if (rowTouched > 0)
                        {
                        if (reader[0] is null || ((reader[3] == null ? "" : reader[3].ToString()) != SERVICE_TS || (reader[3] == null ? "" : reader[3].ToString()) != SERVICE_DE)) break;                            
                            var strPostalCode = reader[0] == null ? "" : reader[0].ToString();
                            DateTime.TryParseExact(reader[1].ToString()!, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTimeCell);
                            var dispatchDate = DateOnly.FromDateTime(dateTimeCell);
                            var strServiceCode = reader[2] == null ? "" : reader[2].ToString();
                            var strProductCode = reader[3] == null ? "" : reader[3].ToString();
                            var bagNo = reader[4] == null ? "" : reader[4].ToString();
                            var countryCode = reader[5] == null ? "" : reader[5].ToString();
                            var weight = (reader[6] == null || reader[6].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[6]);
                            var itemId = reader[7] == null ? "" : reader[7].ToString();
                            var sealNo = reader[8] == null ? "" : reader[8].ToString();
                            var strDispatchNo = reader[9] == null ? "" : reader[9].ToString();
                            var itemValue = (reader[10] == null || reader[10].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[10]);
                            var itemDesc = reader[11] == null ? "" : reader[11].ToString();
                            var recpName = reader[12] == null ? "" : reader[12].ToString();
                            var telNo = reader[13] == null ? "" : reader[13].ToString();
                            var email = reader[14] == null ? "" : reader[14].ToString();
                            var address = reader[15] == null ? "" : reader[15].ToString();
                            var postcode = reader[16] == null ? "" : reader[16].ToString();
                            var city = reader[17] == null ? "" : reader[17].ToString();
                            var addressLine2 = reader[18] == null ? "" : reader[18].ToString();
                            var addressNo = reader[19] == null ? "" : reader[19].ToString();
                            var identityNo = reader[20] == null ? "" : reader[20].ToString();
                            var identityType = reader[21] == null ? "" : reader[21].ToString();
                            var state = reader[22] == null ? "" : reader[22].ToString();
                            var length = (reader[23] == null || reader[23].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[23]);
                            var width =  (reader[24] == null || reader[24].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[24]);
                            var height = (reader[25] == null || reader[25].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[25]);
                            var taxPaymentMethod = reader[26] == null ? "" : reader[26].ToString();
                            var hsCode = reader[27] == null ? "" : reader[27].ToString();
                            var qty = reader[28] == null ? 0 : Convert.ToInt32(reader[28]);

                            var price = pricer.CalculatePrice(countryCode: countryCode, weight: weight, state: state, city: city, postcode: postcode);

                            if (pricer.ErrorMsg != "") throw new Exception(pricer.ErrorMsg);

                            itemCount++;

                            totalWeight += weight;
                            totalPrice += price;

                            listItemIds.Add(itemId);
                            listItemIdsForUpdate.Add(itemId);
                            if (!listBagNos.Contains(bagNo)) listBagNos.Add(bagNo);
                            listIOSSChecking.Add(new DispatchValidateIOSSDto { Row = rowTouched, TrackingNo = itemId, CountryCode = countryCode, IOSS = taxPaymentMethod });
                            listCountryCodes.Add(new DispatchValidateCountryDto { Id = itemId, CountryCode = countryCode });
                            listParticulars.Add(new DispatchValidateParticularsDto { Id = itemId, DispatchNo = strDispatchNo, PostalCode = strPostalCode, ServiceCode = strServiceCode, ProductCode = strProductCode });

                            var blockMilestone = rowTouched % BlockSize;
                            if (blockMilestone == 0)
                            {
                                //Block validation
                                Parallel.Invoke(new ParallelOptions
                                { MaxDegreeOfParallelism = Environment.ProcessorCount },
                                    () => Id_IsDuplicate(ref validationResult_id_IsDuplicate, listItemIds),
                                    () => Bag_IsDuplicate(ref validationResult_bag_IsDuplicate, listBagNos),
                                    () => IOSS_Missing(ref validationResult_ioss_missing, listIOSSChecking, DispatchProfile.PostalCode[..2]),
                                    () => Id_HasInvalidPrefixSuffix(ref validationResult_id_HasInvalidPrefixSuffix, listItemIds),
                                    () => {
                                        if (listCountryCodes.Any(u => u.CountryCode != "NG") && ServiceCode == SERVICE_DE) // bypass checking for Country Code NG
                                        {
                                            Id_HasInvalidCheckDigit(ref validationResult_id_HasInvalidCheckDigit, listItemIds);
                                            Id_HasInvalidLength(ref validationResult_id_HasInvalidLength, listItemIds);
                                        }
                                    },
                                    () => {
                                        if (listCountryCodes.Any(u => u.CountryCode == "NG") && ServiceCode == SERVICE_DE) // checking for Country Code NG
                                        {
                                            Dispatch_IsOverweight(ref validationResult_dispatch_Overweight, totalWeight);
                                        }
                                    },
                                    () => Country_IsInvalidCountry(ref validationResult_country_HasInvalidCountry, listCountryCodes),
                                    () => TrackingNo_IsPreRegistered(ref validationResult_trackingno_IsPreRegistered, listItemIds, CustomerCode, DispatchProfile.ProductCode),
                                    () => TrackingNo_IsInvalid(ref validationResult_trackingno_IsInvalid, listItemIds, CustomerCode, DispatchProfile.ProductCode, DispatchProfile.PostalCode),
                                    () => Dispatch_IsParticularsNotTally(ref validationResult_dispatch_IsParticularsNotTally, listParticulars));

                                listItemIds.Clear();
                                listCountryCodes.Clear();
                                listParticulars.Clear();
                            }
                            // else
                            // {
                            //     Single validation
                            //     Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                            //        () => Id_HasInvalidCheckDigit(ref validationResult_id_HasInvalidCheckDigit, itemId));
                            // }
                        }

                        rowTouched++;
                        itemRow = rowTouched;

                        #region Validation Progress
                        var perc = Convert.ToInt32(Convert.ToDecimal(rowTouched) / Convert.ToDecimal(rowCount) * 100);

                        if (perc > 0)
                        {
                            if (milestones.Contains(perc) && !percHistory.Contains(perc))
                            {
                                percHistory.Add(perc);

                                Parallel.Invoke(async () =>
                                {
                                    using db db = new();
                                    var dispatchValidation = db.Dispatchvalidations
                                        .Where(u => u.DispatchNo == DispatchProfile.DispatchNo)
                                        .FirstOrDefault();

                                    if (dispatchValidation is not null)
                                    {
                                        dispatchValidation.ValidationProgress = perc;
                                    }

                                    await db.SaveChangesAsync().ConfigureAwait(false);
                                });
                            }
                        }
                        #endregion
                    }
                } while (reader.NextResult());



                if (listItemIds.Count != 0)
                {
                    //Block validation
                    Parallel.Invoke(new ParallelOptions
                    { MaxDegreeOfParallelism = Environment.ProcessorCount },
                        () => Id_IsDuplicate(ref validationResult_id_IsDuplicate, listItemIds),
                        () => Bag_IsDuplicate(ref validationResult_bag_IsDuplicate, listBagNos),
                        () => IOSS_Missing(ref validationResult_ioss_missing, listIOSSChecking, DispatchProfile.PostalCode[..2]),
                        () => Id_HasInvalidPrefixSuffix(ref validationResult_id_HasInvalidPrefixSuffix, listItemIds),
                        () =>
                        {
                            if (listCountryCodes.Any(u => u.CountryCode != "NG") && ServiceCode == SERVICE_DE) // bypass checking for Country Code NG
                            {
                                Id_HasInvalidCheckDigit(ref validationResult_id_HasInvalidCheckDigit, listItemIds);
                                Id_HasInvalidLength(ref validationResult_id_HasInvalidLength, listItemIds);
                            }
                        },
                        () =>
                        {
                            if (listCountryCodes.Any(u => u.CountryCode == "NG") && ServiceCode == SERVICE_DE) // checking for Country Code NG
                            {
                                Dispatch_IsOverweight(ref validationResult_dispatch_Overweight, totalWeight);
                            }
                        },
                        () => Country_IsInvalidCountry(ref validationResult_country_HasInvalidCountry, listCountryCodes),
                        () => TrackingNo_IsPreRegistered(ref validationResult_trackingno_IsPreRegistered, listItemIds, CustomerCode, DispatchProfile.ProductCode),
                        () => TrackingNo_IsInvalid(ref validationResult_trackingno_IsInvalid, listItemIds, CustomerCode, DispatchProfile.ProductCode, DispatchProfile.PostalCode),
                        () => Dispatch_IsParticularsNotTally(ref validationResult_dispatch_IsParticularsNotTally, listParticulars));
                }

                #region Check Wallet Balance

                var wallet = db.Wallets
                    .Where(u => u.Customer == CustomerCode)
                    .Where(u => u.Currency == CurrencyId)
                    .FirstOrDefault();

                if (wallet.Balance < totalPrice)
                {
                    isFundLack = true;
                    var lack = totalPrice - wallet.Balance;
                    validationResult_wallet_InsufficientBalance.Message = $"Insufficient fund by {Currency} {lack:N2}. Your wallet balance is {Currency} {wallet.Balance:N2}. Total payment is {Currency} {totalPrice:N2}.";
                }

                #endregion

                bool allowUploadIfInsufficientFund = false;
                bool allowUploadIfUnregisteredIds = true;

                var appSetting_insufficientFund = db.ApplicationSettings.FirstOrDefault(u => u.Name.Equals("AllowUploadIfInsufficientFund"));
                var appSetting_unregisteredId = db.ApplicationSettings.FirstOrDefault(u => u.Name.Equals("AllowUploadIfUnregisteredIds"));

                if (appSetting_insufficientFund is not null) allowUploadIfInsufficientFund = appSetting_insufficientFund.Value.ToString() == "true";
                if (appSetting_unregisteredId is not null) allowUploadIfUnregisteredIds = appSetting_unregisteredId.Value.ToString() == "true";

                #region Dispatch Validation

                //----- Write Error Details -----//
                Parallel.Invoke(async () =>
                {
                    var dateValidationEnd = DateTime.Now;
                    var tookInSec = Math.Round(dateValidationEnd.Subtract(dateValidationStart).TotalSeconds, 0);
                    #region Validation Result JSON

                    if (!string.IsNullOrWhiteSpace(validationResult_dispatch_IsDuplicate.Message))
                    {
                        isValid = false;
                        validations.Add(validationResult_dispatch_IsDuplicate);
                    }

                    if (validationResult_dispatch_IsParticularsNotTally.ItemIds.Count != 0 || !string.IsNullOrWhiteSpace(validationResult_dispatch_IsParticularsNotTally.Message))
                    {
                        isValid = false;
                        validations.Add(validationResult_dispatch_IsParticularsNotTally);
                    }

                    if (!string.IsNullOrWhiteSpace(validationResult_dispatch_Overweight.Message))
                    {
                        isValid = false;
                        validations.Add(validationResult_dispatch_Overweight);
                    }

                    if (validationResult_id_IsDuplicate.ItemIds.Count != 0 || !string.IsNullOrWhiteSpace(validationResult_id_IsDuplicate.Message))
                    {
                        isValid = false;
                        validations.Add(validationResult_id_IsDuplicate);
                    }

                    if (validationResult_bag_IsDuplicate.ItemIds.Count != 0 || !string.IsNullOrWhiteSpace(validationResult_bag_IsDuplicate.Message))
                    {
                        isValid = false;
                        validations.Add(validationResult_bag_IsDuplicate);
                    }

                    if (validationResult_id_HasInvalidLength.ItemIds.Count != 0 || !string.IsNullOrWhiteSpace(validationResult_id_HasInvalidLength.Message))
                    {
                        isValid = false;
                        validations.Add(validationResult_id_HasInvalidLength);
                    }

                    if (validationResult_id_HasInvalidPrefixSuffix.ItemIds.Count != 0 || !string.IsNullOrWhiteSpace(validationResult_id_HasInvalidPrefixSuffix.Message))
                    {
                        isValid = false;
                        validations.Add(validationResult_id_HasInvalidPrefixSuffix);
                    }

                    if (validationResult_id_HasInvalidCheckDigit.ItemIds.Count != 0 || !string.IsNullOrWhiteSpace(validationResult_id_HasInvalidCheckDigit.Message))
                    {
                        isValid = false;
                        validations.Add(validationResult_id_HasInvalidCheckDigit);
                    }

                    if (validationResult_country_HasInvalidCountry.ItemIds.Count != 0 || !string.IsNullOrWhiteSpace(validationResult_country_HasInvalidCountry.Message))
                    {
                        isValid = false;
                        validations.Add(validationResult_country_HasInvalidCountry);
                    }

                    if (validationResult_ioss_missing.ItemIds.Count != 0 || !string.IsNullOrWhiteSpace(validationResult_ioss_missing.Message))
                    {
                        isValid = false;
                        validations.Add(validationResult_ioss_missing);
                    }

                    if (validationResult_trackingno_IsInvalid.ItemIds.Count != 0 || !string.IsNullOrWhiteSpace(validationResult_trackingno_IsInvalid.Message))
                    {
                        isValid = false;
                        validations.Add(validationResult_trackingno_IsInvalid);
                    }

                    if (validationResult_wallet_InsufficientBalance.ItemIds.Count != 0 || !string.IsNullOrWhiteSpace(validationResult_wallet_InsufficientBalance.Message))
                    {
                        isValid = allowUploadIfInsufficientFund;
                        validations.Add(validationResult_wallet_InsufficientBalance);
                    }

                    if (validationResult_trackingno_IsPreRegistered.ItemIds.Count != 0 || !string.IsNullOrWhiteSpace(validationResult_trackingno_IsPreRegistered.Message))
                    {
                        isValid = allowUploadIfUnregisteredIds;
                        validations.Add(validationResult_trackingno_IsPreRegistered);
                    }

                    if (!isValid)
                    {
                        await ValidationsHandling(validations, DispatchProfile.DispatchNo, CustomerCode);
                    }
                    else //---- Deduct Amount If Valid ----//
                    {

                        if (validations.Count == 1 && validations[0].Category == "Insufficient Wallet Balance")
                        {
                            await ValidationsHandling(validations, DispatchProfile.DispatchNo, CustomerCode, true);
                        }

                        if (validations.Any(u => u.Category.Equals("Unregistered Tracking Number(s)")))
                        {
                            await ValidationsHandling(validations, DispatchProfile.DispatchNo, CustomerCode, true);
                        }

                        using var dbconn = new db();
                        var wallet = dbconn.Wallets
                                                .Where(u => u.Customer == CustomerCode)
                                                .Where(u => u.Currency == CurrencyId)
                                                .FirstOrDefault();

                        var initialBalance = wallet.Balance;

                        wallet.Balance -= totalPrice;
wait dbconn.CustomerTransactions.AddAsync(new CustomerTransaction()
                        {
                            Wallet = wallet.Id,
                            Customer = wallet.Customer,
                            PaymentMode = eWallet.Type,
                            Currency = currency.Abbr,
                            TransactionType = "Pre-Alert",
                            Amount = -totalPrice,
                            ReferenceNo = DispatchProfile.DispatchNo,
                            Description = $"Initial Balance: {Currency} {initialBalance}. Deducted {Currency} {decimal.Round(totalPrice, 2, MidpointRounding.AwayFromZero)} from {wallet.Customer}'s {wallet.Id} Wallet.",
                            TransactionDate = DateTime.Now
                        }).ConfigureAwait(false);
                        var eWallet = await dbconn.EWalletTypes.FirstOrDefaultAsync(x => x.Id.Equals(wallet.EWalletType));

                        a

                        await dbconn.DispatchUsedAmounts.AddAsync(new DispatchUsedAmount()
                        {
                            CustomerCode = wallet.Customer,
                            Wallet = wallet.Id,
                            Amount = totalPrice,
                            DispatchNo = DispatchProfile.DispatchNo,
                            DateTime = DateTime.Now,
                            Description = "Pre-Alert"
                        }).ConfigureAwait(false);

                        await dbconn.SaveChangesAsync();
                    }


                    #endregion


                    using db db = new();
                    var dispatchValidation = db.Dispatchvalidations
                                                .Where(u => u.DispatchNo == DispatchProfile.DispatchNo)
                                                .FirstOrDefault();

                    if (dispatchValidation is not null)
                    {
                        dispatchValidation.TookInSec = tookInSec;
                        dispatchValidation.IsValid = isValid ? 1u : 0u;
                        dispatchValidation.FilePath = FilePath;
                        dispatchValidation.DateCompleted = dateValidationEnd;
                        dispatchValidation.Status = DispatchValidationEnumConst.STATUS_FINISH;
                        dispatchValidation.ValidationProgress = 100;
                        dispatchValidation.IsFundLack = isFundLack ? 1u : 0u;
                    }

                    await db.SaveChangesAsync().ConfigureAwait(false);
                });
                #endregion

                var queueTask = db.Queues.Find(QueueId);
                if (queueTask is not null)
                {
                    DateTime dateImportCompleted = DateTime.Now;
                    var tookInSec = dateImportCompleted.Subtract(dateValidationStart).TotalSeconds;

                    queueTask.Status = QueueEnumConst.STATUS_FINISH;
                    queueTask.ErrorMsg = null;
                    queueTask.TookInSec = Math.Round(tookInSec, 0);
                    queueTask.StartTime = dateValidationStart;
                    queueTask.EndTime = dateImportCompleted;
                    queueTask.DeleteFileOnFailed = 0;
                    queueTask.DeleteFileOnSuccess = 0;

                    if (isValid)
                    {
                        if (isFundLack == false || allowUploadIfInsufficientFund == true)
                        {
                            await db.Queues.AddAsync(new Queue
                            {
                                EventType = "Upload Dispatch",
                                FilePath = FilePath,
                                Status = QueueEnumConst.STATUS_NEW,
                                DateCreated = DateTime.Now,
                                DeleteFileOnFailed = 0,
                                DeleteFileOnSuccess = 0,
                                StartTime = DateTime.Now,
                                EndTime = DateTime.MinValue,
                                TookInSec = 0,
                            }).ConfigureAwait(false);
                        }
                    }
                }

                await db.SaveChangesAsync();
                #endregion
            }
            catch (Exception ex)
            {
                validationResult_Others.Message = ex.InnerException != null ? ex.InnerException.Message : string.Format("Row : {0} - {1}", itemRow, ex.Message);
                validations.Add(validationResult_Others);

                await LogQueueError(new QueueErrorEventArg
                {
                    FilePath = FilePath,
                    ErrorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message,
                    Validations = validations,
                });
            }
        }

        private static async Task ValidationsHandling(List<DispatchValidateDto> validations, string dispatchNo, string customerCode, bool successEmail = false)
        {
            string validationJSON = JsonConvert.SerializeObject(validations);

            using var dbconn = new db();
            var ChibiKey = await dbconn.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var ChibiURL = await dbconn.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", ChibiKey.Value);
            var formData = new MultipartFormDataContent();

            var jsonContent = new StringContent(validationJSON);
            jsonContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            formData.Add(jsonContent, "file", dispatchNo + ".json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ChibiURL.Value + "upload"),
                Content = formData,
            };

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<ChibiUpload>(body);

            if (result != null)
            {
                result.originalName = dispatchNo;
                //Insert to DB
                Chibi entity = new()
                {
                    FileName = result.name == null ? "" : DateTime.Now.ToString("yyyyMMdd") + "_" + result.name,
                    UUID = result.uuid ?? "",
                    URL = result.url ?? "",
                    OriginalName = result.originalName,
                    GeneratedName = result.name ?? ""
                };

                await dbconn.Chibis.AddAsync(entity).ConfigureAwait(false);
                await dbconn.SaveChangesAsync();

                await FileServer.InsertFileToAlbum(result.uuid, true, dbconn);
            }

            var apiUrl = await dbconn.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("APIURL"));

            if (apiUrl != null)
            {
                var emailclient = new HttpClient();
                emailclient.DefaultRequestHeaders.Clear();

                PreAlertFailureEmail preAlertFailureEmail = new()
                {
                    customerCode = customerCode,
                    dispatchNo = dispatchNo,
                    validations = validations
                };

                var urlEndpoint = successEmail ? "services/app/EmailContent/SendPreAlertSuccessWithErrorsEmailAsync" : "services/app/EmailContent/SendPreAlertFailureEmail";
                var content = new StringContent(JsonConvert.SerializeObject(preAlertFailureEmail), Encoding.UTF8, "application/json");
                var emailrequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(apiUrl.Value + urlEndpoint),
                    Content = content,
                };
                await emailclient.SendAsync(emailrequest).ConfigureAwait(false);
            }
        }

        private static async Task LogQueueError(QueueErrorEventArg arg)
        {
            using db db = new();
            #region Queue
            var q = db.Queues
                .Where(u => u.FilePath == arg.FilePath)
                .Where(u => u.Status == QueueEnumConst.STATUS_RUNNING)
                .FirstOrDefault();

            if (q != null)
            {
                q.Status = QueueEnumConst.STATUS_ERROR;
                q.ErrorMsg = arg.ErrorMsg;
                q.TookInSec = 0;

                await db.SaveChangesAsync().ConfigureAwait(false);
            }
            #endregion

            #region DispatchValidation
            var dv = db.Dispatchvalidations
                .Where(u => u.FilePath == arg.FilePath)
                .Where(u => u.Status == DispatchValidationEnumConst.STATUS_RUNNING)
                .FirstOrDefault();

            if (dv != null)
            {
                dv.Status = DispatchValidationEnumConst.STATUS_FINISH;
                dv.IsValid = 0u;
                dv.DateCompleted = DateTime.Now;
                dv.ValidationProgress = 100;
                dv.IsFundLack = 0u;
                dv.TookInSec = 0;

                await db.SaveChangesAsync().ConfigureAwait(false);

                await ValidationsHandling(arg.Validations, dv.DispatchNo, dv.CustomerCode);
            }
            #endregion
        }

        private static bool Id_IsDuplicate(ref Dto.DispatchValidateDto validationResult, List<string> ids)
        {
            var result = false;

            using db db = new();
            db.ChangeTracker.AutoDetectChangesEnabled = false;

            var existingDispatchNo = db.Itemmins.Where(u => ids.Contains(u.Id.ToUpper().Trim())).Select(u => u.Id + " (" + u.Dispatch.DispatchNo + ")").ToList();

            if (existingDispatchNo is not null)
            {
                validationResult.ItemIds.AddRange(existingDispatchNo);
            }

            return result;
        }

        private static bool Id_HasInvalidLength(ref Dto.DispatchValidateDto validationResult, List<string> ids)
        {
            var result = ids.Where(u => u.Trim().Length != ID_LENGTH).Select(u => u + " (" + u.Length + ")");
            var hasAny = result.Any();

            if (hasAny)
            {
                validationResult.ItemIds.AddRange(result);
            }

            return hasAny;
        }

        private static bool Id_HasInvalidPrefixSuffix(ref Dto.DispatchValidateDto validationResult, List<string> ids)
        {
            var result = false;

            return result;
        }

        private static int GenerateCheckDigit(string serialNo)
        {
            int checkDigit;

            int[] multiplier = [8, 6, 4, 2, 3, 5, 9, 7];

            var charArr = serialNo.ToCharArray();
            if (charArr.Length == 8)
            {
                int sum = 0;
                for (var i = 0; i < charArr.Length; i++)
                {
                    var x = int.Parse(charArr[i].ToString());
                    var m = multiplier[i];

                    sum += x * m;
                }

                var remainder = sum % 11;

                if (remainder == 0)
                {
                    checkDigit = 5;
                }
                else if (remainder == 1)
                {
                    checkDigit = 0;
                }
                else
                {
                    checkDigit = 11 - remainder;
                }
            }
            else
            {
                throw new Exception("Serial number must be in 8 digit: " + serialNo);
            }

            return checkDigit;
        }

        private static bool Id_HasInvalidCheckDigit(ref Dto.DispatchValidateDto validationResult, List<string> ids)
        {
            var result = false;

            var list = ids.Where(id =>
            {
                string serialNo = id.Substring(2, 8);
                int checkDigit = Convert.ToInt32(id.Substring(10, 1));

                var r = GenerateCheckDigit(serialNo) != checkDigit;

                return r;
            }).Select(u => u).ToList();

            if (result) validationResult.ItemIds.AddRange(list);

            result = list.Count != 0;

            return result;
        }

        private static bool Id_HasInvalidCheckDigit(ref Dto.DispatchValidateDto validationResult, string id)
        {
            var result = false;

            if (id.Length == 13)
            {
                string serialNo = id.Substring(2, 8);
                int checkDigit = Convert.ToInt32(id.Substring(10, 1));

                result = GenerateCheckDigit(serialNo) != checkDigit;
            }
            else
            {
                result = true;
            }

            if (result)
            {
                validationResult.ItemIds.Add(id);
            }

            return result;
        }

        private bool Country_IsInvalidCountry(ref Dto.DispatchValidateDto validationResult, List<Dto.DispatchValidateCountryDto> model)
        {
            var result = false;

            if (ServiceCode == SERVICE_DE)
            {
                var postalId = PostalCode[..2];
                var list = model.Where(u => u.CountryCode != postalId).Select(u => u.Id + " (" + u.CountryCode + ")").ToList();
                validationResult.ItemIds.AddRange(list);
                result = list.Count != 0;
            }
            else
            {
                var list = model.Where(u => !ListPostalCountry.Contains(u.CountryCode)).Select(u => u.Id + " (" + u.CountryCode + ")").ToList();
                validationResult.ItemIds.AddRange(list);
                result = list.Count != 0;
            }

            return result;
        }

        private bool Dispatch_IsParticularsNotTally(ref Dto.DispatchValidateDto validationResult, List<Dto.DispatchValidateParticularsDto> model)
        {
            var result = false;

            var list = model.Where(u =>
                                        u.DispatchNo != DispatchProfile.DispatchNo ||
                                        u.PostalCode != DispatchProfile.PostalCode ||
                                        u.ServiceCode != DispatchProfile.ServiceCode ||
                                        u.ProductCode != DispatchProfile.ProductCode
                                  ).Select(u => $"{u.Id} [{u.DispatchNo} / {u.PostalCode} / {u.ServiceCode} / {u.ProductCode}]").ToList();

            validationResult.ItemIds.AddRange(list);
            if (list.Count != 0) validationResult.Message = $"Dispatch Particulars [{DispatchProfile.DispatchNo} / {DispatchProfile.PostalCode} / {DispatchProfile.ServiceCode} / {DispatchProfile.ProductCode}]";

            result = list.Count != 0;

            return result;
        }

        private bool Dispatch_IsOverweight(ref Dto.DispatchValidateDto validationResult, decimal weight)
        {   
            var result = false;

            if (weight >= totalWeightLimitForCountry_NG)
            {
                validationResult.Message = "$ The total weight of the items in the dispatch exceeds the maximum";
                result = true;
            } 
            return result;
        }

        private static bool Bag_IsDuplicate(ref Dto.DispatchValidateDto validationResult, List<string> bags)
        {
            var result = false;

            using db db = new();
            db.ChangeTracker.AutoDetectChangesEnabled = false;

            var existingBagNo = db.Bags.FirstOrDefault(u => bags.Contains(u.BagNo));
            if (existingBagNo is not null)
            {
                var dispatch = db.Dispatches.FirstOrDefault(u => u.Id.Equals(existingBagNo.DispatchId));
                validationResult.Message = $"Bag No. {existingBagNo.BagNo} already exists in the dispatch {dispatch.DispatchNo}";
                result = true;
            }

            return result;
        }

        private static bool IOSS_Missing(ref Dto.DispatchValidateDto validationResult, List<DispatchValidateIOSSDto> model, string postalCode)
        {
            var result = false;

            if (postalCode == "KG" || postalCode == "GQ")
            {
                // List<string> listEuropeCountries = ["NO", "FR", "GR", "DE", "ES", "IT", "HU", "IE", "DK", "BE", "RO", "PL", "NL", "LU"];
                List<string> listEuropeCountries = ["IE", "HR", "MT", "CZ"];

                var list = model.Where(u =>
                                            listEuropeCountries.Contains(u.CountryCode.Trim().ToUpper()) &&
                                            string.IsNullOrWhiteSpace(u.IOSS)
                                      ).Select(u => $"Row no {u.Row} IOSS Tax is empty ({u.TrackingNo} {u.CountryCode})").ToList();

                validationResult.ItemIds.AddRange(list);

                result = list.Count != 0;
            }
            return result;
        }

        private static bool TrackingNo_IsPreRegistered(ref Dto.DispatchValidateDto validationResult, List<string> model, string customerCode, string productCode)
        {
            var result = false;

            using db db = new();
            db.ChangeTracker.AutoDetectChangesEnabled = false;

            var registeredItems = db.ItemTrackings.Where(u => u.CustomerCode.Equals(customerCode) &&
                                                              u.ProductCode.Equals(productCode)
                                                        ).ToList();

            var unregisteredItems = model.Where(a => !registeredItems.Any(b => b.TrackingNo.Equals(a)))
                                        .Select(u => $"Tracking No {u} has not been registered.").ToList();

            validationResult.ItemIds.AddRange(unregisteredItems);

            result = unregisteredItems.Count != 0;

            return result;
        }

        private static bool TrackingNo_IsInvalid(ref Dto.DispatchValidateDto validationResult, List<string> model, string customerCode, string productCode, string postalCode)
        {
            var result = false;

            using db db = new();
            db.ChangeTracker.AutoDetectChangesEnabled = false;

            Task<List<string>> asyncTask = GetItemTrackingIds(customerCode, postalCode, productCode);
            asyncTask.Wait();
            List<string> poolItemIds = asyncTask.Result;

            var unidentifiedItems = model.Where(a => !poolItemIds.Any(b => b.Equals(a))).ToList();

            var registeredItems = db.ItemTrackings.Where(u => u.CustomerCode.Equals(customerCode) &&
                                                              u.ProductCode.Equals(productCode)).ToList();

            var invalidItems = unidentifiedItems.Where(a => !registeredItems.Any(b => b.TrackingNo.Equals(a)))
                                                .Select(u => $"Tracking No {u} is Invalid.").ToList();

            validationResult.ItemIds.AddRange(invalidItems);

            result = invalidItems.Count != 0;

            return result;
        }

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

        private static async Task<Stream> GetFileStream(string url)
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var contentByteArray = await response.Content.ReadAsByteArrayAsync();
                return new MemoryStream(contentByteArray);
            }
            return null;
        }

        private static async Task<List<string>> GetItemTrackingIds(string customerCode, string postalCode, string productCode)
        {
            using db db = new();
            db.ChangeTracker.AutoDetectChangesEnabled = false;


            List<string> paths = [];
            List<string> item = [];

            var applications = db.ItemTrackingApplications.Where(x => x.CustomerCode.Equals(customerCode) &&
                                                                      x.PostalCode.Equals(postalCode) &&
                                                                      x.ProductCode.Equals(productCode));

            foreach (var application in applications) paths.Add(application.Path);

            if (!paths.Count.Equals(0))
            {
                //---- Gets all Excel files and retrieves its info to create the object ItemIds ----//
                foreach (var path in paths)
                {
                    ItemTrackingWithPath itemWithPath = new();
                    Stream excel_stream = await FileServer.GetFileStream(path);
                    DataTable dataTable = ConvertToDatatable(excel_stream);
                    dataTable.TableName = path;

                    foreach (DataRow dr in dataTable.Rows)
                    {
                        if (dr.ItemArray[0].ToString() != "")
                        {
                            item.Add(dr.ItemArray[0].ToString());
                        }
                    }
                }
            }

            return item;
        }
    }
}


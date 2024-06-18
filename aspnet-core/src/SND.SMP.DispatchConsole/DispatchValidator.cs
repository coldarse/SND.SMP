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

namespace SND.SMP.DispatchConsole
{
    public class DispatchValidator
    {
        private const string SERVICE_TS = "TS";
        private const string SERVICE_DE = "DE";
        private const int ID_LENGTH = 13;

        private uint QueueId { get; set; }
        private string DirPath { get; set; }
        private string FilePath { get; set; }
        private string FileType { get; set; }
        private string CustomerCode { get; set; }
        private string ServiceCode { get; set; }
        private string PostalCode { get; set; }
        private long CurrencyId { get; set; }
        private int BlockSize { get; set; } = 50;

        private List<string> ListPostalCountry { get; set; }
        private DispatchProfileDto DispatchProfile { get; set; }

        private string Currency { get; set; }
        private int BatchSize { get; set; }


        public DispatchValidator() { }

        public async Task DiscoverAndValidate(string dirPath, string fileType, int batchSize = 750, int blockSize = 50)
        {
            DirPath = dirPath;
            FileType = fileType;
            BatchSize = batchSize;
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

                await db.SaveChangesAsync();
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
                        });

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
            DispatchValidateDto validationResult_bag_IsDuplicate = new() { Category = "Duplicated Bag No" };
            DispatchValidateDto validationResult_id_IsDuplicate = new() { Category = "Duplicated Item ID" };
            DispatchValidateDto validationResult_id_HasInvalidLength = new() { Category = "Invalid Length" };
            DispatchValidateDto validationResult_id_HasInvalidPrefixSuffix = new() { Category = "Invalid Prefix & Suffix" };
            DispatchValidateDto validationResult_id_HasInvalidCheckDigit = new() { Category = "Invalid Check Digit" };
            DispatchValidateDto validationResult_country_HasInvalidCountry = new() { Category = "Invalid Country Code" };
            DispatchValidateDto validationResult_wallet_InsufficientBalance = new() { Category = "Insufficient Wallet Balance" };

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
                var listCountryCodes = new List<DispatchValidateCountryDto>();
                var listParticulars = new List<DispatchValidateParticularsDto>();

                do
                {
                    while (reader.Read())
                    {
                        if (rowTouched > 0)
                        {
                            if(reader[0] is null) break;
                            var strPostalCode = reader[0].ToString()!;
                            var dispatchDate = DateOnly.ParseExact(reader[1].ToString()!, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            var strServiceCode = reader[2].ToString()!;
                            var strProductCode = reader[3].ToString()!;
                            var bagNo = reader[4].ToString()!;
                            var countryCode = reader[5].ToString()!;
                            var weight = Math.Round(Convert.ToDecimal(reader[6]), 3);
                            var itemId = reader[7].ToString()!;
                            var sealNo = reader[8].ToString()!;
                            var strDispatchNo = reader[9].ToString()!;
                            var itemValue = Convert.ToDecimal(reader[10]);
                            var itemDesc = reader[11].ToString()!;
                            var recpName = reader[12].ToString()!;
                            var telNo = reader[13].ToString()!;
                            var email = reader[14] == null ? "" : reader[14].ToString()!;
                            var address = reader[15].ToString()!;
                            var postcode = reader[16].ToString()!;
                            var city = reader[17].ToString()!;
                            var addressLine2 = reader[18] == null ? "" : reader[18].ToString()!;
                            var addressNo = reader[19] == null ? "" : reader[19].ToString()!;
                            var identityNo = reader[20] == null ? "" : reader[20].ToString()!;
                            var identityType = reader[21] == null ? "" : reader[21].ToString()!;
                            var state = reader[22].ToString()!;
                            var length = reader[23] == null ? 0 : Convert.ToDecimal(reader[23]);
                            var width = reader[24] == null ? 0 : Convert.ToDecimal(reader[24]);
                            var height = reader[25] == null ? 0 : Convert.ToDecimal(reader[25]);
                            var taxPaymentMethod = reader[26] == null ? "" : reader[26].ToString()!;
                            var hsCode = reader[27] == null ? "" : reader[27].ToString()!;
                            var qty = reader[28] == null ? 0 : Convert.ToInt32(reader[28]);

                            var price = pricer.CalculatePrice(countryCode: countryCode, weight: weight);

                            itemCount++;

                            totalWeight += weight;
                            totalPrice += price;

                            listItemIds.Add(itemId);
                            if(!listBagNos.Contains(bagNo)) listBagNos.Add(bagNo);
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
                                    () => Id_HasInvalidLength(ref validationResult_id_HasInvalidLength, listItemIds),
                                    () => Id_HasInvalidPrefixSuffix(ref validationResult_id_HasInvalidPrefixSuffix, listItemIds),
                                    () => Id_HasInvalidCheckDigit(ref validationResult_id_HasInvalidCheckDigit, listItemIds),
                                    () => Country_IsInvalidCountry(ref validationResult_country_HasInvalidCountry, listCountryCodes),
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

                                    await db.SaveChangesAsync();
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
                        () => Id_HasInvalidLength(ref validationResult_id_HasInvalidLength, listItemIds),
                        () => Id_HasInvalidPrefixSuffix(ref validationResult_id_HasInvalidPrefixSuffix, listItemIds),
                        () => Id_HasInvalidCheckDigit(ref validationResult_id_HasInvalidCheckDigit, listItemIds),
                        () => Country_IsInvalidCountry(ref validationResult_country_HasInvalidCountry, listCountryCodes),
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

                #region Dispatch Validation

                //----- Write Error Details -----//
                Parallel.Invoke(async () =>
                {
                    var dateValidationEnd = DateTime.Now;
                    var tookInSec = Math.Round(dateValidationEnd.Subtract(dateValidationStart).TotalSeconds, 0);
                    var filePath = "";
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

                    if (validationResult_id_IsDuplicate.ItemIds.Count != 0 || !string.IsNullOrWhiteSpace(validationResult_id_IsDuplicate.Message))
                    {
                        isValid = false;
                        validations.Add(validationResult_id_IsDuplicate);
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

                    if (validationResult_wallet_InsufficientBalance.ItemIds.Count != 0 || !string.IsNullOrWhiteSpace(validationResult_wallet_InsufficientBalance.Message))
                    {
                        isValid = false;
                        validations.Add(validationResult_wallet_InsufficientBalance);
                    }

                    if (!isValid)
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
                        formData.Add(jsonContent, "file", DispatchProfile.DispatchNo + ".json");

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
                        filePath = result.url;

                        if (result != null)
                        {
                            result.originalName = DispatchProfile.DispatchNo;
                            //Insert to DB
                            Chibi entity = new()
                            {
                                FileName = result.name == null ? "" : DateTime.Now.ToString("yyyyMMdd") + "_" + result.name,
                                UUID = result.uuid ?? "",
                                URL = result.url ?? "",
                                OriginalName = result.originalName,
                                GeneratedName = result.name ?? ""
                            };

                            await dbconn.Chibis.AddAsync(entity);
                            await dbconn.SaveChangesAsync();

                            await FileServer.InsertFileToAlbum(result.uuid, true, dbconn);
                        }
                    }
                    else //---- Deduct Amount If Valid ----//
                    {
                        using var dbconn = new db();
                        var wallet = dbconn.Wallets
                                                .Where(u => u.Customer == CustomerCode)
                                                .Where(u => u.Currency == CurrencyId)
                                                .FirstOrDefault();

                        var initialBalance = wallet.Balance;

                        wallet.Balance -= totalPrice;

                        DateTime DateTimeUTC = DateTime.UtcNow;
                        TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                        DateTime cstDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTimeUTC, cstZone);

                        var eWallet = await dbconn.EWalletTypes.FirstOrDefaultAsync(x => x.Id.Equals(wallet.EWalletType));

                        await dbconn.CustomerTransactions.AddAsync(new CustomerTransaction()
                        {
                            Wallet = wallet.Id,
                            Customer = wallet.Customer,
                            PaymentMode = eWallet.Type,
                            Currency = currency.Abbr,
                            TransactionType = "Pre-Alert",
                            Amount = -totalPrice,
                            ReferenceNo = DispatchProfile.DispatchNo,
                            Description = $"Initial Balance: {Currency} {initialBalance}. Deducted {Currency} {decimal.Round(totalPrice, 2, MidpointRounding.AwayFromZero)} from {wallet.Customer}'s {wallet.Id} Wallet. Remaining {Currency} {decimal.Round(wallet.Balance, 2, MidpointRounding.AwayFromZero)}.",
                            TransactionDate = cstDateTime
                        });
                        
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

                    await db.SaveChangesAsync();
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
                        if (isFundLack == false)
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
                            });
                        }
                    }
                }

                await db.SaveChangesAsync();
                #endregion
            }
            catch (Exception ex)
            {
                if (ex.InnerException is not null)
                {
                    await LogQueueError(new QueueErrorEventArg
                    {
                        FilePath = FilePath,
                        ErrorMsg = ex.InnerException.Message
                    });
                }
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

                await db.SaveChangesAsync();
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

            result = list.Count != 0;

            if (result)
            {
                validationResult.ItemIds.AddRange(list);
            }

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
            if(list.Count != 0) validationResult.Message = $"Dispatch Particulars [{DispatchProfile.DispatchNo} / {DispatchProfile.PostalCode} / {DispatchProfile.ServiceCode} / {DispatchProfile.ProductCode}]";

            result = list.Count != 0;

            return result;
        }

        private static bool Bag_IsDuplicate(ref Dto.DispatchValidateDto validationResult, List<string> bags)
        {
            var result = false;

            using db db = new();
            db.ChangeTracker.AutoDetectChangesEnabled = false;

            var existingBagNo = db.Bags.Where(u => bags.Contains(u.BagNo.ToUpper().Trim())).Select(u => u.Id + " (" + u.Dispatch.DispatchNo + ")").ToList();

            if (existingBagNo is not null)
            {
                validationResult.ItemIds.AddRange(existingBagNo);
            }

            return result;
        }
    }
}


using System;
using ExcelDataReader;
using static SND.SMP.DispatchConsole.WorkerDispatchImport;
using System.Globalization;
using static SND.SMP.Shared.EnumConst;
using SND.SMP.DispatchConsole.Dto;
//using SND.SMP.Shared.Modules.Dispatch;
using Humanizer;
using System.Collections.Generic;
using SND.SMP.DispatchConsole.EF;
using System.Data;

namespace SND.SMP.DispatchConsole
{
    public class DispatchValidator
    {
        private const string SERVICE_TS = "TS";
        private const string SERVICE_DE = "DE";

        private const int ID_LENGTH = 13;

        private uint _queueId { get; set; }
        private string _dirPath { get; set; }
        private string _filePath { get; set; }
        private int _batchSize { get; set; }
        private string _fileType { get; set; }
        private DispatchProfileDto _dispatchProfile { get; set; }

        private string _customerCode { get; set; }
        private string _serviceCode { get; set; }
        private string _postalCode { get; set; }
        private string _currencyId { get; set; }

        private int _blockSize { get; set; } = 50;

        private List<string> _listPostalCountry { get; set; }

        public DispatchValidator()
        {
        }
        public async Task DiscoverAndValidate(string dirPath, string fileType, int batchSize = 750, int blockSize = 50)
        {
            _dirPath = dirPath;
            _fileType = fileType;
            _batchSize = batchSize;
            _blockSize = blockSize;

            using (EF.db db = new EF.db())
            {
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

                if (newTask != null)
                {
                    _queueId = newTask.Id;
                    _filePath = newTask.FilePath;

                    newTask.Status = QueueEnumConst.STATUS_RUNNING;
                    newTask.ErrorMsg = null;
                    newTask.TookInSec = null;

                    await db.SaveChangesAsync();
                }
            }

            if (!string.IsNullOrWhiteSpace(_filePath))
            {
                var fileProfile = $"{_filePath}.profile.json";
                //var filesExist = File.Exists(fileProfile) && File.Exists(_filePath);
                var fileString = await FileServer.GetFileStreamAsString(fileProfile);

                if (fileString is not null)
                {
                    // var t = await File.ReadAllTextAsync(fileProfile);
                    _dispatchProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<DispatchProfileDto>(fileString);

                    if (_dispatchProfile != null)
                    {
                        _customerCode = _dispatchProfile.AccNo;
                        _serviceCode = _dispatchProfile.ServiceCode;
                        _postalCode = _dispatchProfile.PostalCode;

                        using (EF.db db = new EF.db())
                        {
                            db.Dispatchvalidations.RemoveRange(db.Dispatchvalidations.Where(u => u.DispatchNo == _dispatchProfile.DispatchNo));

                            await db.Dispatchvalidations.AddAsync(new EF.Dispatchvalidation
                            {
                                DispatchNo = _dispatchProfile.DispatchNo,
                                CustomerCode = _dispatchProfile.AccNo,
                                PostalCode = _dispatchProfile.PostalCode,
                                ServiceCode = _dispatchProfile.ServiceCode,
                                ProductCode = _dispatchProfile.ProductCode,
                                DateStarted = DateTime.Now,
                                Status = DispatchValidationEnumConst.STATUS_RUNNING,
                                ValidationProgress = 0
                            });

                            await db.SaveChangesAsync();
                        }

                        if (_fileType == DispatchEnumConst.ImportFileType.Excel.ToString())
                        {
                            await ValidateFromExcel();
                        }
                    }
                }
            }
        }

        private async Task ValidateFromExcel()
        {
            var isValid = true;
            var isFundLack = false;
            DateTime dateValidationStart = DateTime.Now;

            DispatchValidateDto validationResult_dispatch_IsDuplicate = new DispatchValidateDto { Category = "Duplicated Dispatch No." };
            DispatchValidateDto validationResult_dispatch_IsParticularsNotTally = new DispatchValidateDto { Category = "Dispatch Particulars Not Tally" };
            DispatchValidateDto validationResult_id_IsDuplicate = new DispatchValidateDto { Category = "Duplicated Item ID" };
            DispatchValidateDto validationResult_id_HasInvalidLength = new DispatchValidateDto { Category = "Invalid Length" };
            DispatchValidateDto validationResult_id_HasInvalidPrefixSuffix = new DispatchValidateDto { Category = "Invalid Prefix & Suffix" };
            DispatchValidateDto validationResult_id_HasInvalidCheckDigit = new DispatchValidateDto { Category = "Invalid Check Digit" };
            DispatchValidateDto validationResult_country_HasInvalidCountry = new DispatchValidateDto { Category = "Invalid Country Code" };
            DispatchValidateDto validationResult_wallet_InsufficientBalance = new DispatchValidateDto { Category = "Insufficient Wallet Balance" };

            using (EF.db db = new EF.db())
            {
                try
                {
                    _listPostalCountry = db.Postalcountries
                        .Where(u => u.PostalCode == _postalCode)
                        .Select(u => u.CountryCode)
                        .ToList();

                    var dispatchNoDuplicate = db.Dispatches.Where(u => u.DispatchNo == _dispatchProfile.DispatchNo).Any();
                    if (dispatchNoDuplicate)
                    {
                        validationResult_dispatch_IsDuplicate.Message = $"{_dispatchProfile.DispatchNo} already existed in system";
                    }

                    #region Validation
                    // using (var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read))
                    var stream = await FileServer.GetFileStream(_filePath);
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var rowCount = reader.RowCount;
                        var ran = new Random();
                        var milestoneCount = 4;
                        var milestones = new List<int>();
                        var g = 0;
                        var percHistory = new List<int>();
                        for (var i = 1; i <= milestoneCount; i++)
                        {
                            g += 1 * (ran.Next(15, 25));
                            milestones.Add(g);
                        }

                        var pricer = new DispatchPricer(accNo: _dispatchProfile.AccNo,
                                        postalCode: _dispatchProfile.PostalCode,
                                        serviceCode: _dispatchProfile.ServiceCode,
                                        productCode: _dispatchProfile.ProductCode,
                                        rateOptionId: _dispatchProfile.RateOptionId,
                                        paymentMode: _dispatchProfile.PaymentMode);

                        _currencyId = pricer.CurrencyId;

                        var rowTouched = 0;
                        var listItems = new List<DispatchItemDto>();

                        var itemCount = 0;
                        var totalWeight = 0m;
                        var totalPrice = 0m;

                        var month = Convert.ToInt32($"{_dispatchProfile.DateDispatch.Year}{_dispatchProfile.DateDispatch.Month.ToString().PadLeft(2, '0')}");
                        var listItemIds = new List<string>();
                        var listCountryCodes = new List<Dto.DispatchValidateCountryDto>();
                        var listParticulars = new List<Dto.DispatchValidateParticularsDto>();

                        do
                        {
                            while (reader.Read())
                            {
                                if (rowTouched > 0)
                                {
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
                                    listCountryCodes.Add(new DispatchValidateCountryDto { Id = itemId, CountryCode = countryCode });
                                    listParticulars.Add(new DispatchValidateParticularsDto { Id = itemId, DispatchNo = strDispatchNo, PostalCode = strPostalCode, ServiceCode = strServiceCode, ProductCode = strProductCode });

                                    var blockMilestone = rowTouched % _blockSize;
                                    if (blockMilestone == 0)
                                    {
                                        //Block validation
                                        Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                                            () => Id_IsDuplicate(ref validationResult_id_IsDuplicate, listItemIds),
                                            () => Id_HasInvalidLength(ref validationResult_id_HasInvalidLength, listItemIds),
                                            () => Id_HasInvalidPrefixSuffix(ref validationResult_id_HasInvalidPrefixSuffix, listItemIds),
                                            () => Id_HasInvalidCheckDigit(ref validationResult_id_HasInvalidCheckDigit, listItemIds),
                                            () => Country_IsInvalidCountry(ref validationResult_country_HasInvalidCountry, listCountryCodes),
                                            () => Dispatch_IsParticularsNotTally(ref validationResult_dispatch_IsParticularsNotTally, listParticulars));

                                        listItemIds.Clear();
                                        listCountryCodes.Clear();
                                        listParticulars.Clear();
                                    }
                                    else
                                    {
                                        //Single validation
                                        //Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                                        //    () => Id_HasInvalidCheckDigit(ref validationResult_id_HasInvalidCheckDigit, itemId));
                                    }
                                }

                                rowTouched++;

                                #region Validation Progress
                                var perc = Convert.ToInt32((Convert.ToDecimal(rowTouched) / Convert.ToDecimal(rowCount)) * 100);

                                if (perc > 0)
                                {
                                    if (milestones.Contains(perc) && !percHistory.Contains(perc))
                                    {
                                        percHistory.Add(perc);

                                        Parallel.Invoke(async () =>
                                        {
                                            using (EF.db db = new EF.db())
                                            {
                                                var dispatchValidation = db.Dispatchvalidations
                                                    .Where(u => u.DispatchNo == _dispatchProfile.DispatchNo)
                                                    .FirstOrDefault();

                                                if (dispatchValidation != null)
                                                {
                                                    dispatchValidation.ValidationProgress = perc;
                                                }

                                                await db.SaveChangesAsync();
                                            }
                                        });
                                    }
                                }
                                #endregion
                            }
                        } while (reader.NextResult());

                        if (listItemIds.Any())
                        {
                            //Block validation
                            Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                                () => Id_IsDuplicate(ref validationResult_id_IsDuplicate, listItemIds),
                                () => Id_HasInvalidLength(ref validationResult_id_HasInvalidLength, listItemIds),
                                () => Id_HasInvalidPrefixSuffix(ref validationResult_id_HasInvalidPrefixSuffix, listItemIds),
                                () => Id_HasInvalidCheckDigit(ref validationResult_id_HasInvalidCheckDigit, listItemIds),
                                () => Country_IsInvalidCountry(ref validationResult_country_HasInvalidCountry, listCountryCodes),
                                () => Dispatch_IsParticularsNotTally(ref validationResult_dispatch_IsParticularsNotTally, listParticulars));
                        }

                        #region Wallet Balance
                        var walletBalance = db.Customercurrencies
                            .Where(u => u.CustomerCode == _customerCode)
                            .Where(u => u.CurrencyId == _currencyId)
                            .Select(u => u.Balance)
                            .FirstOrDefault();

                        if (walletBalance.GetValueOrDefault() < totalPrice)
                        {
                            isFundLack = true;
                            var lack = totalPrice - walletBalance.GetValueOrDefault();
                            validationResult_wallet_InsufficientBalance.Message = $"Insufficient fund by {_currencyId} {lack.ToString("N2")}. Your wallet balance is {_currencyId} {walletBalance.GetValueOrDefault().ToString("N2")}. Total payment is {_currencyId} {totalPrice.ToString("N2")}.";
                        }
                        #endregion

                        #region Dispatch Validation
                        if (true)
                        {
                            Parallel.Invoke(async () =>
                            {
                                var dateValidationEnd = DateTime.Now;
                                var tookInSec = Math.Round(dateValidationEnd.Subtract(dateValidationStart).TotalSeconds, 0);

                                #region Validation Result File
                                var filePath = Path.Combine(_dirPath, $"_{_dispatchProfile.DispatchNo}");
                                var fi = new FileInfo(filePath);
                                if (!fi.Directory.Exists)
                                {
                                    fi.Directory.Create();
                                }
                                if (fi.Exists)
                                {
                                    fi.Delete();
                                }

                                using (var sw = fi.CreateText())
                                {
                                    if (!string.IsNullOrWhiteSpace(validationResult_dispatch_IsDuplicate.Message))
                                    {
                                        isValid = false;

                                        await sw.WriteLineAsync($"{validationResult_dispatch_IsDuplicate.Category}:");
                                        await sw.WriteLineAsync(validationResult_dispatch_IsDuplicate.Message);
                                        await sw.WriteLineAsync();
                                    }

                                    if (validationResult_id_IsDuplicate.ItemIds.Any() || !string.IsNullOrWhiteSpace(validationResult_id_IsDuplicate.Message))
                                    {
                                        isValid = false;

                                        await sw.WriteLineAsync($"{validationResult_id_IsDuplicate.Category}:");
                                        foreach (var line in validationResult_id_IsDuplicate.ItemIds)
                                        {
                                            await sw.WriteLineAsync($"{line}");
                                        }
                                        await sw.WriteLineAsync(validationResult_id_IsDuplicate.Message);
                                        await sw.WriteLineAsync();
                                    }

                                    if (validationResult_id_HasInvalidLength.ItemIds.Any() || !string.IsNullOrWhiteSpace(validationResult_id_HasInvalidLength.Message))
                                    {
                                        isValid = false;

                                        await sw.WriteLineAsync($"{validationResult_id_HasInvalidLength.Category}:");
                                        foreach (var line in validationResult_id_HasInvalidLength.ItemIds)
                                        {
                                            await sw.WriteLineAsync($"{line}");
                                        }
                                        await sw.WriteLineAsync(validationResult_id_HasInvalidLength.Message);
                                        await sw.WriteLineAsync();
                                    }

                                    if (validationResult_id_HasInvalidPrefixSuffix.ItemIds.Any() || !string.IsNullOrWhiteSpace(validationResult_id_HasInvalidPrefixSuffix.Message))
                                    {
                                        isValid = false;

                                        await sw.WriteLineAsync($"{validationResult_id_HasInvalidPrefixSuffix.Category}:");
                                        foreach (var line in validationResult_id_HasInvalidPrefixSuffix.ItemIds)
                                        {
                                            await sw.WriteLineAsync($"{line}");
                                        }
                                        await sw.WriteLineAsync(validationResult_id_HasInvalidPrefixSuffix.Message);
                                        await sw.WriteLineAsync();
                                    }

                                    if (validationResult_id_HasInvalidCheckDigit.ItemIds.Any() || !string.IsNullOrWhiteSpace(validationResult_id_HasInvalidCheckDigit.Message))
                                    {
                                        isValid = false;

                                        await sw.WriteLineAsync($"{validationResult_id_HasInvalidCheckDigit.Category}:");
                                        foreach (var line in validationResult_id_HasInvalidCheckDigit.ItemIds)
                                        {
                                            await sw.WriteLineAsync($"{line}");
                                        }
                                        await sw.WriteLineAsync(validationResult_id_HasInvalidCheckDigit.Message);
                                        await sw.WriteLineAsync();
                                    }

                                    if (validationResult_country_HasInvalidCountry.ItemIds.Any() || !string.IsNullOrWhiteSpace(validationResult_country_HasInvalidCountry.Message))
                                    {
                                        isValid = false;

                                        await sw.WriteLineAsync($"{validationResult_country_HasInvalidCountry.Category}:");
                                        foreach (var line in validationResult_country_HasInvalidCountry.ItemIds)
                                        {
                                            await sw.WriteLineAsync($"{line}");
                                        }
                                        await sw.WriteLineAsync(validationResult_country_HasInvalidCountry.Message);
                                        await sw.WriteLineAsync();
                                    }

                                    if (validationResult_dispatch_IsParticularsNotTally.ItemIds.Any() || !string.IsNullOrWhiteSpace(validationResult_dispatch_IsParticularsNotTally.Message))
                                    {
                                        isValid = false;

                                        await sw.WriteLineAsync($"{validationResult_dispatch_IsParticularsNotTally.Category}:");
                                        foreach (var line in validationResult_dispatch_IsParticularsNotTally.ItemIds)
                                        {
                                            await sw.WriteLineAsync($"{line}");
                                        }
                                        await sw.WriteLineAsync(validationResult_dispatch_IsParticularsNotTally.Message);
                                        await sw.WriteLineAsync();
                                    }

                                    if (validationResult_wallet_InsufficientBalance.ItemIds.Any() || !string.IsNullOrWhiteSpace(validationResult_wallet_InsufficientBalance.Message))
                                    {
                                        isValid = false;

                                        await sw.WriteLineAsync($"{validationResult_wallet_InsufficientBalance.Category}:");
                                        await sw.WriteLineAsync(validationResult_wallet_InsufficientBalance.Message);
                                        await sw.WriteLineAsync();
                                    }
                                }

                                if (isValid)
                                {
                                    fi.Delete();
                                }
                                #endregion

                                using (EF.db db = new EF.db())
                                {
                                    var dispatchValidation = db.Dispatchvalidations
                                        .Where(u => u.DispatchNo == _dispatchProfile.DispatchNo)
                                        .FirstOrDefault();

                                    if (dispatchValidation != null)
                                    {
                                        dispatchValidation.TookInSec = tookInSec;
                                        dispatchValidation.IsValid = isValid ? 1u : 0u;
                                        dispatchValidation.FilePath = isValid ? null : filePath;
                                        dispatchValidation.DateCompleted = dateValidationEnd;
                                        dispatchValidation.Status = DispatchValidationEnumConst.STATUS_FINISH;
                                        dispatchValidation.ValidationProgress = 100;
                                        dispatchValidation.IsFundLack = isFundLack ? 1u : 0u;
                                    }

                                    await db.SaveChangesAsync();
                                }
                            });
                        }
                        #endregion

                        var queueTask = db.Queues.Find(_queueId);
                        if (queueTask != null)
                        {
                            DateTime dateImportCompleted = DateTime.Now;
                            var tookInSec = dateImportCompleted.Subtract(dateValidationStart).TotalSeconds;

                            queueTask.Status = QueueEnumConst.STATUS_FINISH;
                            queueTask.ErrorMsg = null;
                            queueTask.TookInSec = Math.Round(tookInSec, 0);
                            queueTask.DateStart = dateValidationStart;
                            queueTask.DateEnd = dateImportCompleted;

                            if (isValid)
                            {
                                await db.Queues.AddAsync(new Queue
                                {
                                    EventType = "Upload Dispatch",
                                    FilePath = _filePath,
                                    Status = QueueEnumConst.STATUS_NEW,
                                    DateCreated = DateTime.Now
                                });
                            }
                        }

                        await db.SaveChangesAsync();
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        await LogQueueError(new QueueErrorEventArg
                        {
                            FilePath = _filePath,
                            ErrorMsg = ex.InnerException.Message
                        });
                    }
                }
            }
        }

        private async Task LogQueueError(QueueErrorEventArg arg)
        {
            using (EF.db db = new EF.db())
            {
                #region Queue
                var q = db.Queues
                    .Where(u => u.FilePath == arg.FilePath)
                    .Where(u => u.Status == QueueEnumConst.STATUS_RUNNING)
                    .FirstOrDefault();

                if (q != null)
                {
                    q.Status = QueueEnumConst.STATUS_ERROR;
                    q.ErrorMsg = arg.ErrorMsg;
                    q.TookInSec = null;

                    await db.SaveChangesAsync();
                }
                #endregion
            }
        }

        private bool Id_IsDuplicate(ref Dto.DispatchValidateDto validationResult, List<string> ids)
        {
            var result = false;

            using (EF.db db = new EF.db())
            {
                db.ChangeTracker.AutoDetectChangesEnabled = false;

                var existingDispatchNo = db.Itemmins.Where(u => ids.Contains(u.Id.ToUpper().Trim())).Select(u => u.Id + " (" + u.Dispatch.DispatchNo + ")").ToList();

                if (existingDispatchNo != null)
                {
                    validationResult.ItemIds.AddRange(existingDispatchNo);
                }
            }

            return result;
        }

        private bool Id_HasInvalidLength(ref Dto.DispatchValidateDto validationResult, List<string> ids)
        {
            var result = ids.Where(u => u.Trim().Length != ID_LENGTH).Select(u => u + " (" + u.Length + ")");
            var hasAny = result.Any();

            if (hasAny)
            {
                validationResult.ItemIds.AddRange(result);
            }

            return hasAny;
        }

        private bool Id_HasInvalidPrefixSuffix(ref Dto.DispatchValidateDto validationResult, List<string> ids)
        {
            var result = false;

            return result;
        }

        private int GenerateCheckDigit(string serialNo)
        {
            int checkDigit = 0;

            int[] multiplier = new int[] { 8, 6, 4, 2, 3, 5, 9, 7 };

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

        private bool Id_HasInvalidCheckDigit(ref Dto.DispatchValidateDto validationResult, List<string> ids)
        {
            var result = false;

            var list = ids.Where(id =>
            {
                string serialNo = id.Substring(2, 8);
                int checkDigit = Convert.ToInt32(id.Substring(10, 1));

                var r = GenerateCheckDigit(serialNo) != checkDigit;

                return r;
            }).Select(u => u).ToList();

            result = list.Any();

            if (result)
            {
                validationResult.ItemIds.AddRange(list);
            }

            return result;
        }

        private bool Id_HasInvalidCheckDigit(ref Dto.DispatchValidateDto validationResult, string id)
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

            if (_serviceCode == SERVICE_DE)
            {
                var postalId = _postalCode.Substring(0, 2);
                var list = model.Where(u => u.CountryCode != postalId).Select(u => u.Id + " (" + u.CountryCode + ")").ToList();
                validationResult.ItemIds.AddRange(list);
                result = list.Any();
            }
            else
            {
                var list = model.Where(u => !_listPostalCountry.Contains(u.CountryCode)).Select(u => u.Id + " (" + u.CountryCode + ")").ToList();
                validationResult.ItemIds.AddRange(list);
                result = list.Any();
            }

            return result;
        }

        private bool Dispatch_IsParticularsNotTally(ref Dto.DispatchValidateDto validationResult, List<Dto.DispatchValidateParticularsDto> model)
        {
            var result = false;

            var list = model.Where(u => u.DispatchNo != _dispatchProfile.DispatchNo || u.PostalCode != _dispatchProfile.PostalCode || u.ServiceCode != _dispatchProfile.ServiceCode || u.ProductCode != _dispatchProfile.ProductCode).Select(u => u.Id + " (" + u.DispatchNo + " / " + u.PostalCode + " / " + u.ServiceCode + " / " + u.ProductCode + ")").ToList();
            validationResult.ItemIds.AddRange(list);

            result = list.Any();

            return result;
        }
    }
}


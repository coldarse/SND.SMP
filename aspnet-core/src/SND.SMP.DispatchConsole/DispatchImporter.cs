using System;
using ExcelDataReader;
using static SND.SMP.DispatchConsole.WorkerDispatchImport;
using System.Globalization;
using static SND.SMP.Shared.EnumConst;
using SND.SMP.DispatchConsole.Dto;
//using SND.SMP.Shared.Modules.Dispatch;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using SND.SMP.DispatchConsole.EF;
using SND.SMP.ItemTrackings;
using Abp.Extensions;

namespace SND.SMP.DispatchConsole
{
    public class DispatchImporter
    {
        private const string SERVICE_TS = "TS";

        private const string SERVICE_DE = "DE";
        private uint _queueId { get; set; }
        private string _filePath { get; set; }
        private int _batchSize { get; set; }
        private string _fileType { get; set; }
        private long _currencyId { get; set; }

        private DispatchProfileDto _dispatchProfile { get; set; }

        private string _currency { get; set; }

        private int BlockSize { get; set; } = 50;

        public DispatchImporter() { }

        public async Task DiscoverAndImport(string fileType, int batchSize = 750, int blockSize = 50)
        {
            _fileType = fileType;
            _batchSize = batchSize;
            BlockSize = blockSize;

            using (EF.db db = new())
            {
                var hasRunning = db.Queues
                    .Where(u => u.EventType == QueueEnumConst.EVENT_TYPE_DISPATCH_UPLOAD)
                    .Where(u => u.Status == QueueEnumConst.STATUS_RUNNING)
                    .Any();

                if (hasRunning) return;

                var newTask = db.Queues
                    .Where(u => u.EventType == QueueEnumConst.EVENT_TYPE_DISPATCH_UPLOAD)
                    .Where(u => u.Status == QueueEnumConst.STATUS_NEW)
                    .OrderBy(u => u.DateCreated)
                    .FirstOrDefault();

                if (newTask != null)
                {
                    _queueId = newTask.Id;
                    _filePath = newTask.FilePath;

                    newTask.Status = QueueEnumConst.STATUS_RUNNING;
                    newTask.ErrorMsg = null;
                    newTask.TookInSec = 0;

                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            if (!string.IsNullOrWhiteSpace(_filePath))
            {
                var fileProfile = "";
                using EF.db chibiDB = new();
                var dispatchFile = chibiDB.Chibis.FirstOrDefault(x => x.URL.Equals(_filePath));
                if (dispatchFile is not null)
                {
                    var dispatchFilePair = chibiDB.Chibis.Where(x => x.OriginalName.Equals(dispatchFile.OriginalName)).ToList();
                    var jsonDispatchFile = dispatchFilePair.FirstOrDefault(x => x.URL.Contains("json"));
                    fileProfile = jsonDispatchFile.URL;
                }
                var fileString = await FileServer.GetFileStreamAsString(fileProfile);

                if (fileString is not null)
                {
                    _dispatchProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<DispatchProfileDto>(fileString);

                    if (_dispatchProfile != null)
                    {
                        if (_fileType == DispatchEnumConst.ImportFileType.Excel.ToString())
                        {
                            await ImportFromExcel();
                        }
                    }
                }
            }
        }

        private async Task ImportFromExcel()
        {
            DateTime dateImportStart = DateTime.Now;

            using (EF.db db = new())
            {
                try
                {
                    #region By Chunk
                    dateImportStart = DateTime.Now;
                    decimal avgItemValue = 0m;

                    var stream = await FileServer.GetFileStream(_filePath);
                    using var reader = ExcelReaderFactory.CreateReader(stream);
                    var rowCount = reader.RowCount;

                    var pricer = new DispatchPricer(
                                    accNo: _dispatchProfile.AccNo,
                                    postalCode: _dispatchProfile.PostalCode,
                                    serviceCode: _dispatchProfile.ServiceCode,
                                    productCode: _dispatchProfile.ProductCode,
                                    rateOptionId: _dispatchProfile.RateOptionId,
                                    paymentMode: _dispatchProfile.PaymentMode);

                    if (pricer.ErrorMsg != "") throw new Exception(pricer.ErrorMsg);

                    var currency = await db.Currencies.FirstOrDefaultAsync(c => c.Id == pricer.CurrencyId);
                    _currencyId = currency.Id;
                    _currency = currency.Abbr;

                    var dispatch = new EF.Dispatch
                    {
                        DispatchNo = _dispatchProfile.DispatchNo,
                        CustomerCode = _dispatchProfile.AccNo,
                        PostalCode = _dispatchProfile.PostalCode,
                        ServiceCode = _dispatchProfile.ServiceCode,
                        ProductCode = _dispatchProfile.ProductCode,
                        DispatchDate = _dispatchProfile.DateDispatch,
                        CorateOptionId = _dispatchProfile.RateOptionId,
                        PaymentMode = _dispatchProfile.PaymentMode,
                        CurrencyId = _currency,
                        IsActive = 0,
                        ImportProgress = 0,
                        Status = (int)DispatchEnumConst.Status.Stage1,
                    };

                    await db.Dispatches.AddAsync(dispatch);
                    await db.SaveChangesAsync().ConfigureAwait(false);

                    var savedDispatch = await db.Dispatches.FirstOrDefaultAsync(x => x.DispatchNo.Equals(_dispatchProfile.DispatchNo));
                    var customer = await db.Customers.FirstOrDefaultAsync(x => x.Code.Equals(_dispatchProfile.AccNo));

                    var rowTouched = 0;
                    var listItems = new List<DispatchItemDto>();

                    var itemCount = 0;
                    var totalWeight = 0m;
                    var totalPrice = 0m;

                    var listBags = new List<EF.Bag>();

                    var month = Convert.ToInt32($"{_dispatchProfile.DateDispatch.Year}{_dispatchProfile.DateDispatch.Month.ToString().PadLeft(2, '0')}");

                    do
                    {
                        while (reader.Read())
                        {
                            if (rowTouched > 0)
                            {
                                if (reader[0] is null && ((reader[3] == null ? "" : reader[3].ToString()) != SERVICE_TS || (reader[3] == null ? "" : reader[3].ToString()) != SERVICE_DE)) break;
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
                                var width = (reader[24] == null || reader[24].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[24]);
                                var height = (reader[25] == null || reader[25].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[25]);
                                var taxPaymentMethod = reader[26] == null ? "" : reader[26].ToString();
                                var hsCode = reader[27] == null ? "" : reader[27].ToString();
                                var qty = reader[28] == null ? 0 : Convert.ToInt32(reader[28]);

                                var price = pricer.CalculatePrice(countryCode: countryCode, weight: weight, state: state, city: city, postcode: postcode);

                                if (pricer.ErrorMsg != "") throw new Exception(pricer.ErrorMsg);

                                avgItemValue += price;

                                listItems.Add(new DispatchItemDto
                                {
                                    PostalCode = strPostalCode,
                                    DispatchDate = dispatchDate,
                                    ServiceCode = strServiceCode,
                                    ProductCode = strProductCode,
                                    BagNo = bagNo,
                                    CountryCode = countryCode,
                                    Weight = weight,
                                    TrackingNo = itemId,
                                    SealNo = sealNo,
                                    ItemValue = itemValue,
                                    ItemDesc = itemDesc,
                                    RecipientName = recpName,
                                    TelNo = telNo,
                                    Email = email,
                                    Address = address,
                                    Postcode = postcode,
                                    City = city,
                                    AddressLine2 = addressLine2,
                                    AddressNo = addressNo,
                                    IdentityNo = identityNo,
                                    IdentityType = identityType,
                                    State = state,
                                    Length = length,
                                    Width = width,
                                    Height = height,
                                    TaxPaymentMethod = taxPaymentMethod,
                                    HSCode = hsCode,
                                    Qty = qty,
                                    Price = price
                                });

                                itemCount++;
                                totalWeight += weight;
                                totalPrice += price;

                                var bag = listBags.Where(u => u.BagNo == bagNo).FirstOrDefault();
                                if (bag != null)
                                {
                                    bag.WeightPre += weight;
                                    bag.ItemCountPre += 1;
                                }
                                else
                                {
                                    listBags.Add(new EF.Bag
                                    {
                                        BagNo = bagNo,
                                        CountryCode = countryCode,
                                        WeightPre = weight,
                                        ItemCountPre = 1,
                                        DispatchId = dispatch.Id
                                    });
                                }

                                var blockMilestone = rowTouched % BlockSize;
                                if (blockMilestone == 0)
                                {
                                    await db.Bags.AddRangeAsync(listBags.Where(u => u.Id == 0)).ConfigureAwait(false);
                                    await db.SaveChangesAsync();

                                    await db.Items.AddRangeAsync(listItems.Select(u => new EF.Item
                                    {
                                        Id = u.TrackingNo,
                                        DispatchId = dispatch.Id,
                                        BagId = listBags.Find(p => p.BagNo == u.BagNo).Id,
                                        DispatchDate = dispatch.DispatchDate,
                                        Month = month,
                                        PostalCode = u.PostalCode,
                                        ServiceCode = u.ServiceCode,
                                        ProductCode = u.ProductCode,
                                        CountryCode = u.CountryCode,
                                        Weight = u.Weight,
                                        BagNo = u.BagNo,
                                        SealNo = u.SealNo,
                                        Price = u.Price,
                                        ItemValue = u.ItemValue,
                                        ItemDesc = u.ItemDesc,
                                        RecpName = u.RecipientName,
                                        TelNo = u.TelNo,
                                        Email = u.Email,
                                        Address = u.Address,
                                        Postcode = u.Postcode,
                                        City = u.City,
                                        Address2 = u.AddressLine2,
                                        AddressNo = u.AddressNo,
                                        State = u.State,
                                        Length = u.Length,
                                        Width = u.Width,
                                        Height = u.Height,
                                        Hscode = u.HSCode,
                                        Qty = u.Qty,
                                        TaxPayMethod = u.TaxPaymentMethod,
                                        IdentityType = u.IdentityType,
                                        PassportNo = u.IdentityNo,
                                        Stage1StatusDesc = "Pre Check",
                                        DateStage1 = DateTime.Now,
                                        Status = (int)DispatchEnumConst.Status.Stage1
                                    })).ConfigureAwait(false);

                                    await db.Itemmins.AddRangeAsync(listItems.Select(u => new EF.Itemmin
                                    {
                                        Id = u.TrackingNo,
                                        DispatchId = dispatch.Id,
                                        BagId = listBags.Find(p => p.BagNo == u.BagNo).Id,
                                        DispatchDate = dispatch.DispatchDate,
                                        Month = month,
                                        CountryCode = u.CountryCode,
                                        Weight = u.Weight,
                                        ItemValue = u.ItemValue,
                                        ItemDesc = u.ItemDesc.Truncate(60, ".."),
                                        RecpName = u.RecipientName.Truncate(30, ".."),
                                        TelNo = u.TelNo.Truncate(15, ".."),
                                        Address = u.Address.Truncate(100, ".."),
                                        City = u.City.Truncate(30, ".."),
                                        Status = (int)DispatchEnumConst.Status.Stage1
                                    })).ConfigureAwait(false);

                                    await db.TrackingNoForUpdates.AddRangeAsync(listItems.Select(u => new TrackingNoForUpdates.TrackingNoForUpdate
                                    {
                                        TrackingNo = u.TrackingNo,
                                        DispatchNo = _dispatchProfile.DispatchNo,
                                        ProcessType = TrackingNoForUpdateConst.STATUS_UPDATE,
                                    })).ConfigureAwait(false);

                                    var itemTrackings = db.ItemTrackings.Where(u => u.CustomerCode.Equals(_dispatchProfile.AccNo) &&
                                                                                    u.ProductCode.Equals(_dispatchProfile.ProductCode)).ToList();

                                    var registeredItems = itemTrackings
                                                .Where(a => listItems.Any(b => b.TrackingNo == a.TrackingNo))
                                                .ToList();

                                    foreach (var item in registeredItems)
                                    {
                                        item.DateUsed = DateTime.Now;
                                        item.DispatchId = (int)savedDispatch.Id;
                                    }

                                    db.ItemTrackings.UpdateRange(registeredItems);

                                    var unregistered = listItems
                                                            .Where(b => !itemTrackings.Any(a => a.TrackingNo == b.TrackingNo))
                                                            .Select(a => new ItemTracking
                                                            {
                                                                TrackingNo = a.TrackingNo,
                                                                ApplicationId = 0,
                                                                ReviewId = 0,
                                                                CustomerCode = customer.Code,
                                                                CustomerId = customer.Id,
                                                                DateCreated = DateTime.MinValue,
                                                                DateUsed = DateTime.Now,
                                                                DispatchId = (int)savedDispatch.Id,
                                                                DispatchNo = _dispatchProfile.DispatchNo,
                                                                ProductCode = a.ProductCode
                                                            })
                                                            .ToList();

                                    await db.ItemTrackings.AddRangeAsync(unregistered);

                                    dispatch.ImportProgress = Convert.ToInt32(Convert.ToDecimal(itemCount / Convert.ToDecimal(rowCount)) * 100);

                                    await db.SaveChangesAsync().ConfigureAwait(false);

                                    //reset and next batch
                                    listItems.Clear();
                                }
                            }
                            rowTouched++;
                        }
                    } while (reader.NextResult());

                    #region Remaining
                    if (listItems.Count != 0)
                    {
                        await db.Bags.AddRangeAsync(listBags.Where(u => u.Id == 0)).ConfigureAwait(false);
                        await db.SaveChangesAsync();

                        await db.Items.AddRangeAsync(listItems.Select(u => new EF.Item
                        {
                            Id = u.TrackingNo,
                            DispatchId = dispatch.Id,
                            BagId = listBags.Find(p => p.BagNo == u.BagNo).Id,
                            DispatchDate = dispatch.DispatchDate,
                            Month = month,
                            PostalCode = u.PostalCode,
                            ServiceCode = u.ServiceCode,
                            ProductCode = u.ProductCode,
                            CountryCode = u.CountryCode,
                            Weight = u.Weight,
                            BagNo = u.BagNo,
                            SealNo = u.SealNo,
                            Price = u.Price,
                            ItemValue = u.ItemValue,
                            ItemDesc = u.ItemDesc,
                            RecpName = u.RecipientName,
                            TelNo = u.TelNo,
                            Email = u.Email,
                            Address = u.Address,
                            Postcode = u.Postcode,
                            City = u.City,
                            Address2 = u.AddressLine2,
                            AddressNo = u.AddressNo,
                            State = u.State,
                            Length = u.Length,
                            Width = u.Width,
                            Height = u.Height,
                            Hscode = u.HSCode,
                            Qty = u.Qty,
                            TaxPayMethod = u.TaxPaymentMethod,
                            IdentityType = u.IdentityType,
                            PassportNo = u.IdentityNo,
                            Stage1StatusDesc = "Pre Check",
                            DateStage1 = DateTime.Now,
                            Status = (int)DispatchEnumConst.Status.Stage1
                        })).ConfigureAwait(false);

                        await db.Itemmins.AddRangeAsync(listItems.Select(u => new EF.Itemmin
                        {
                            Id = u.TrackingNo,
                            DispatchId = dispatch.Id,
                            BagId = listBags.Find(p => p.BagNo == u.BagNo).Id,
                            DispatchDate = dispatch.DispatchDate,
                            Month = month,
                            CountryCode = u.CountryCode,
                            Weight = u.Weight,
                            ItemValue = u.ItemValue,
                            ItemDesc = u.ItemDesc.Truncate(60, ".."),
                            RecpName = u.RecipientName.Truncate(30, ".."),
                            TelNo = u.TelNo.Truncate(15, ".."),
                            Address = u.Address.Truncate(100, ".."),
                            City = u.City.Truncate(30, ".."),
                            Status = (int)DispatchEnumConst.Status.Stage1
                        })).ConfigureAwait(false);

                        await db.TrackingNoForUpdates.AddRangeAsync(listItems.Select(u => new TrackingNoForUpdates.TrackingNoForUpdate
                        {
                            TrackingNo = u.TrackingNo,
                            DispatchNo = _dispatchProfile.DispatchNo,
                            ProcessType = TrackingNoForUpdateConst.STATUS_UPDATE
                        })).ConfigureAwait(false);

                        var itemTrackings = db.ItemTrackings.Where(u => u.CustomerCode.Equals(_dispatchProfile.AccNo) &&
                                                                                    u.ProductCode.Equals(_dispatchProfile.ProductCode)).ToList();

                        var registeredItems = itemTrackings
                                                .Where(a => listItems.Any(b => b.TrackingNo == a.TrackingNo))
                                                .ToList();

                        foreach (var item in registeredItems)
                        {
                            item.DateUsed = DateTime.Now;
                            item.DispatchId = (int)savedDispatch.Id;
                            item.DispatchNo = _dispatchProfile.DispatchNo;
                        }

                        db.ItemTrackings.UpdateRange(registeredItems);

                        var unregistered = listItems
                                                .Where(b => !itemTrackings.Any(a => a.TrackingNo == b.TrackingNo))
                                                .Select(a => new ItemTracking
                                                {
                                                    TrackingNo = a.TrackingNo,
                                                    ApplicationId = 0,
                                                    ReviewId = 0,
                                                    CustomerCode = customer.Code,
                                                    CustomerId = customer.Id,
                                                    DateCreated = DateTime.MinValue,
                                                    DateUsed = DateTime.Now,
                                                    DispatchId = (int)savedDispatch.Id,
                                                    DispatchNo = _dispatchProfile.DispatchNo,
                                                    ProductCode = a.ProductCode
                                                })
                                                .ToList();

                        await db.ItemTrackings.AddRangeAsync(unregistered);

                        await db.SaveChangesAsync().ConfigureAwait(false);
                    }
                    #endregion

                    dispatch.Status = (int)DispatchEnumConst.Status.Stage1;
                    dispatch.IsActive = 1;
                    dispatch.NoofBag = listBags.Count;
                    dispatch.ItemCount = itemCount;
                    dispatch.TotalWeight = Math.Round(totalWeight, 3);
                    dispatch.TotalPrice = Math.Round(totalPrice, 2);
                    dispatch.ImportProgress = 100;

                    var queueTask = db.Queues.Find(_queueId);
                    if (queueTask != null)
                    {
                        DateTime dateImportCompleted = DateTime.Now;
                        var tookInSec = dateImportCompleted.Subtract(dateImportStart).TotalSeconds;

                        queueTask.Status = QueueEnumConst.STATUS_FINISH;
                        queueTask.ErrorMsg = null;
                        queueTask.TookInSec = Math.Round(tookInSec, 0);
                        queueTask.StartTime = dateImportStart;
                        queueTask.EndTime = dateImportCompleted;
                    }

                    await db.Queues.AddAsync(new Queue
                    {
                        EventType = "Update Tracking",
                        FilePath = _dispatchProfile.DispatchNo,
                        Status = QueueEnumConst.STATUS_NEW,
                        DateCreated = DateTime.Now,
                        DeleteFileOnFailed = 0,
                        DeleteFileOnSuccess = 0,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.MinValue,
                        TookInSec = 0,
                    }).ConfigureAwait(false);

                    var apiUrl = await db.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("APIURL"));

                    if (apiUrl != null)
                    {
                        var emailclient = new HttpClient();
                        emailclient.DefaultRequestHeaders.Clear();

                        PreAlertSuccessEmail preAlertFailureEmail = new()
                        {
                            customerCode = dispatch.CustomerCode,
                            dispatchNo = dispatch.DispatchNo,
                            totalWeight = dispatch.TotalWeight ?? 0m,
                            totalBags = dispatch.NoofBag ?? 0,
                            avgItemValue = Math.Round(avgItemValue / dispatch.ItemCount ?? 0, 2, MidpointRounding.AwayFromZero).ToString()
                        };

                        var content = new StringContent(JsonConvert.SerializeObject(preAlertFailureEmail), Encoding.UTF8, "application/json");
                        var emailrequest = new HttpRequestMessage
                        {
                            Method = HttpMethod.Post,
                            RequestUri = new Uri(apiUrl.Value + "services/app/EmailContent/SendPreAlertSuccessEmail"),
                            Content = content,
                        };
                        await emailclient.SendAsync(emailrequest).ConfigureAwait(false);
                    }

                    await db.SaveChangesAsync();
                    #endregion
                }
                catch (Exception ex)
                {
                    await LogQueueError(new QueueErrorEventArg
                    {
                        FilePath = _filePath,
                        ErrorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message,
                    });

                    await DeleteDispatch(dispatchNo: _dispatchProfile.DispatchNo).ConfigureAwait(false);
                }
            }
        }

        private async Task LogQueueError(QueueErrorEventArg arg)
        {
            using (EF.db db = new())
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
                    q.TookInSec = 0;

                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                #endregion
            }
        }

        private async Task DeleteDispatch(string dispatchNo)
        {
            using (EF.db db = new())
            {
                db.Dispatches.RemoveRange(db.Dispatches.Where(u => u.DispatchNo == dispatchNo).Where(u => u.IsActive == 0).ToList());

                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}


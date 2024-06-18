using System;
using ExcelDataReader;
using static SND.SMP.DispatchConsole.WorkerDispatchImport;
using System.Globalization;
using static SND.SMP.Shared.EnumConst;
using SND.SMP.DispatchConsole.Dto;
//using SND.SMP.Shared.Modules.Dispatch;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace SND.SMP.DispatchConsole
{
    public class DispatchImporter
    {
        private uint _queueId { get; set; }
        private string _filePath { get; set; }
        private int _batchSize { get; set; }
        private string _fileType { get; set; }
        private long _currencyId { get; set; }

        private DispatchProfileDto _dispatchProfile { get; set; }

        private string _currency { get; set; }

        public DispatchImporter(){}

        public async Task DiscoverAndImport(string fileType, int batchSize = 750)
        {
            _fileType = fileType;
            _batchSize = batchSize;

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
                    newTask.TookInSec = null;

                    await db.SaveChangesAsync();
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
                        ImportProgress = 0
                    };

                    await db.Dispatches.AddAsync(dispatch);
                    await db.SaveChangesAsync();

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
                            }

                            rowTouched++;

                            if ((rowTouched + 1) == _batchSize)
                            {
                                await db.Bags.AddRangeAsync(listBags.Where(u => u.Id == 0));
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
                                    DateStage1 = DateTime.Now,
                                    Status = (int)DispatchEnumConst.Status.Stage1
                                }));

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
                                }));

                                dispatch.ImportProgress = Convert.ToInt32(Convert.ToDecimal(itemCount / Convert.ToDecimal(rowCount)) * 100);

                                await db.SaveChangesAsync();

                                //reset and next batch
                                rowTouched = 0;
                                listItems = [];
                            }
                        }
                    } while (reader.NextResult());

                    #region Remaining
                    if (listItems.Count != 0)
                    {
                        await db.Bags.AddRangeAsync(listBags.Where(u => u.Id == 0));
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
                            DateStage1 = DateTime.Now,
                            Status = (int)DispatchEnumConst.Status.Stage1
                        }));

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
                        }));

                        await db.SaveChangesAsync();
                    }
                    #endregion

                    dispatch.Status = (int)DispatchEnumConst.Status.Stage1;
                    dispatch.IsActive = 1;
                    dispatch.NoofBag = listBags.Count;
                    dispatch.ItemCount = itemCount;
                    dispatch.TotalWeight = totalWeight;
                    dispatch.TotalPrice = totalPrice;
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

                    await db.SaveChangesAsync();
                    #endregion
                }
                catch (Exception ex)
                {
                    await DeleteDispatch(dispatchNo: _dispatchProfile.DispatchNo);

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
                    q.TookInSec = null;

                    await db.SaveChangesAsync();
                }
                #endregion
            }
        }

        private async Task DeleteDispatch(string dispatchNo)
        {
            using (EF.db db = new())
            {
                db.Dispatches.RemoveRange(db.Dispatches.Where(u => u.DispatchNo == dispatchNo).Where(u => u.IsActive == 0).ToList());

                await db.SaveChangesAsync();
            }
        }
    }
}


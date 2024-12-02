using Abp.Extensions;
using ExcelDataReader;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SND.SMP.DispatchImporter.Dto;
using SND.SMP.DispatchImporter.EF;
using SND.SMP.ItemTrackings;
using System.Globalization;
using System.Text;
using static SND.SMP.DispatchImporter.WorkerDispatchImport;
using static SND.SMP.Shared.EnumConst;

namespace SND.SMP.DispatchImporter
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

            using db dbConn = new();
            var hasRunning = await dbConn.Queues
                .Where(u => u.EventType == QueueEnumConst.EVENT_TYPE_DISPATCH_UPLOAD)
                .Where(u => u.Status == QueueEnumConst.STATUS_RUNNING)
                .AnyAsync();

            if (hasRunning) return;

            var newTask = await dbConn.Queues
                .Where(u => u.EventType == QueueEnumConst.EVENT_TYPE_DISPATCH_UPLOAD)
                .Where(u => u.Status == QueueEnumConst.STATUS_NEW)
                .OrderBy(u => u.DateCreated)
                .FirstOrDefaultAsync();

            if (newTask != null)
            {
                _queueId = newTask.Id;
                _filePath = newTask.FilePath;

                newTask.Status = QueueEnumConst.STATUS_RUNNING;
                newTask.ErrorMsg = null;
                newTask.TookInSec = 0;

                await dbConn.SaveChangesAsync().ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(_filePath))
            {
                var fileProfile = "";
                var dispatchFile = await dbConn.Chibis.FirstOrDefaultAsync(x => x.URL.Equals(_filePath));
                if (dispatchFile is not null)
                {
                    var dispatchFilePair = await dbConn.Chibis.Where(x => x.OriginalName.Equals(dispatchFile.OriginalName)).ToListAsync();
                    var jsonDispatchFile = dispatchFilePair.FirstOrDefault(x => x.URL.Contains("json"));
                    fileProfile = jsonDispatchFile.URL;
                }

                var fileString = fileProfile.Equals("") ? "" : await FileServer.GetFileStreamAsString(fileProfile);

                if (!string.IsNullOrEmpty(fileString))
                {
                    _dispatchProfile = JsonConvert.DeserializeObject<DispatchProfileDto>(fileString);

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

        private List<DispatchRow> ReadExcelFile(Stream fileStream)
        {
            var rows = new List<DispatchRow>();
            using var reader = ExcelReaderFactory.CreateReader(fileStream);

            while (reader.Read())
            {
                if (reader.Depth == 0) continue; // Skip header row

                var tempRow = new DispatchRow
                {
                    PostalCode = reader[0] == null ? "" : reader[0].ToString(),
                    DispatchDate = DateOnly.FromDateTime(
                            DateTime.TryParseExact(
                                reader[1].ToString()!,
                                "dd/MM/yyyy",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out DateTime dateTimeCell
                            ) ? dateTimeCell : DateTime.MinValue
                        ),
                    ServiceCode = reader[2] == null ? "" : reader[2].ToString(),
                    ProductCode = reader[3] == null ? "" : reader[3].ToString(),
                    BagNo = reader[4] == null ? "" : reader[4].ToString(),
                    CountryCode = reader[5] == null ? "" : reader[5].ToString(),
                    Weight = (reader[6] == null || reader[6].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[6]),
                    ItemId = reader[7] == null ? "" : reader[7].ToString(),
                    SealNo = reader[8] == null ? "" : reader[8].ToString(),
                    DispatchNo = reader[9] == null ? "" : reader[9].ToString(),
                    ItemValue = (reader[10] == null || reader[10].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[10]),
                    ItemDesc = reader[11] == null ? "" : reader[11].ToString(),
                    RecipientName = reader[12] == null ? "" : reader[12].ToString(),
                    Telephone = reader[13] == null ? "" : reader[13].ToString(),
                    Email = reader[14] == null ? "" : reader[14].ToString(),
                    Address = reader[15] == null ? "" : reader[15].ToString(),
                    Postcode = reader[16] == null ? "" : reader[16].ToString(),
                    City = reader[17] == null ? "" : reader[17].ToString(),
                    AddressLine2 = reader[18] == null ? "" : reader[18].ToString(),
                    AddressNo = reader[19] == null ? "" : reader[19].ToString(),
                    IdentityNo = reader[20] == null ? "" : reader[20].ToString(),
                    IdentityType = reader[21] == null ? "" : reader[21].ToString(),
                    State = reader[22] == null ? "" : reader[22].ToString(),
                    Length = (reader[23] == null || reader[23].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[23]),
                    Width = (reader[24] == null || reader[24].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[24]),
                    Height = (reader[25] == null || reader[25].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[25]),
                    TaxPaymentMethod = reader[26] == null ? "" : reader[26].ToString(),
                    HSCode = reader[27] == null ? "" : reader[27].ToString(),
                    Quantity = reader[28] == null ? 0 : Convert.ToInt32(reader[28])
                };
                rows.Add(tempRow);
            }
            return rows;
        }

        private async Task ImportFromExcel()
        {
            DateTime dateImportStart = DateTime.Now;

            using db dbConn = new();
            try
            {
                #region Importation

                var stream = await FileServer.GetFileStream(_filePath);

                dateImportStart = DateTime.Now;
                decimal avgItemValue = 0m;

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

                await dbConn.Dispatches.AddAsync(dispatch);
                await dbConn.SaveChangesAsync().ConfigureAwait(false);

                var savedDispatch = await dbConn.Dispatches.FirstOrDefaultAsync(x => x.DispatchNo.Equals(_dispatchProfile.DispatchNo));
                var customer = await dbConn.Customers.FirstOrDefaultAsync(x => x.Code.Equals(_dispatchProfile.AccNo));

                var rowTouched = 0;
                var itemCount = 0;
                var totalWeight = 0m;
                var totalPrice = 0m;

                var month = Convert.ToInt32($"{_dispatchProfile.DateDispatch.Year}{_dispatchProfile.DateDispatch.Month.ToString().PadLeft(2, '0')}");
                var listItems = new List<DispatchItemDto>();
                var listBags = new List<EF.Bag>();

                string[] external_tracking_countries = [""];

                var pricer = new DispatchPricer(
                                accNo: _dispatchProfile.AccNo,
                                postalCode: _dispatchProfile.PostalCode,
                                serviceCode: _dispatchProfile.ServiceCode,
                                productCode: _dispatchProfile.ProductCode,
                                rateOptionId: _dispatchProfile.RateOptionId,
                                paymentMode: _dispatchProfile.PaymentMode);

                if (pricer.ErrorMsg != "") throw new Exception(pricer.ErrorMsg);

                var currency = await dbConn.Currencies.FirstOrDefaultAsync(c => c.Id == pricer.CurrencyId);
                _currencyId = currency.Id;
                _currency = currency.Abbr;

                #region V2 Excel Import

                List<DispatchRow> dispatchRows = ReadExcelFile(stream);

                List<DispatchRow> preparedDispatchRows = dispatchRows
                                                            .GroupBy(row => row.ItemId)
                                                            .Select(group => new DispatchRow
                                                            {
                                                                Row = string.Join(", ", group.Select(row => row.Row)),
                                                                PostalCode = group.First().PostalCode,
                                                                DispatchDate = group.First().DispatchDate,
                                                                ServiceCode = group.First().ServiceCode,
                                                                ProductCode = group.First().ProductCode,
                                                                BagNo = group.First().BagNo,
                                                                CountryCode = group.First().CountryCode,
                                                                Weight = group.Sum(row => row.Weight),
                                                                ItemId = group.Key,
                                                                SealNo = group.First().SealNo,
                                                                DispatchNo = group.First().DispatchNo,
                                                                ItemValue = group.Sum(row => row.ItemValue),
                                                                ItemDesc = string.Join(", ", group.Select(row => row.ItemDesc)),
                                                                RecipientName = group.First().RecipientName,
                                                                Telephone = group.First().Telephone,
                                                                Email = group.First().Email,
                                                                Address = group.First().Address,
                                                                Postcode = group.First().Postcode,
                                                                City = group.First().City,
                                                                AddressLine2 = group.First().AddressLine2,
                                                                AddressNo = group.First().AddressNo,
                                                                IdentityNo = group.First().IdentityNo,
                                                                IdentityType = group.First().IdentityType,
                                                                State = group.First().State,
                                                                Length = group.First().Length,
                                                                Width = group.First().Width,
                                                                Height = group.First().Height,
                                                                TaxPaymentMethod = group.First().TaxPaymentMethod,
                                                                HSCode = group.First().HSCode,
                                                                Quantity = group.Sum(row => row.Quantity),
                                                                Price = group.First().Price
                                                            }).ToList();

                itemCount = preparedDispatchRows.Count;
                foreach (DispatchRow row in preparedDispatchRows)
                {
                    rowTouched++;

                    var price = pricer.CalculatePrice(countryCode: row.CountryCode, weight: row.Weight, state: row.State, city: row.City, postcode: row.PostalCode);

                    if (pricer.ErrorMsg != "") throw new Exception(pricer.ErrorMsg);

                    totalWeight += row.Weight;
                    totalPrice += price;

                    listItems.Add(new DispatchItemDto
                    {
                        PostalCode = row.PostalCode,
                        DispatchDate = row.DispatchDate,
                        ServiceCode = row.ServiceCode,
                        ProductCode = row.ProductCode,
                        BagNo = row.BagNo,
                        CountryCode = row.CountryCode,
                        Weight = row.Weight,
                        TrackingNo = row.ItemId,
                        SealNo = row.SealNo,
                        ItemValue = row.ItemValue,
                        ItemDesc = row.ItemDesc,
                        RecipientName = row.RecipientName,
                        TelNo = row.Telephone,
                        Email = row.Email,
                        Address = row.Address,
                        Postcode = row.Postcode,
                        City = row.City,
                        AddressLine2 = row.AddressLine2,
                        AddressNo = row.AddressNo,
                        IdentityNo = row.IdentityNo,
                        IdentityType = row.IdentityType,
                        State = row.State,
                        Length = row.Length,
                        Width = row.Width,
                        Height = row.Height,
                        TaxPaymentMethod = row.TaxPaymentMethod,
                        HSCode = row.HSCode,
                        Qty = row.Quantity,
                        Price = price,
                    });

                    var bag = listBags.FirstOrDefault(u => u.BagNo == row.BagNo) ?? new EF.Bag
                    {
                        BagNo = row.BagNo,
                        CountryCode = row.CountryCode,
                        WeightPre = 0, // Initialize for update
                        ItemCountPre = 0, // Initialize for update
                        DispatchId = dispatch.Id
                    };

                    bag.WeightPre += row.Weight;
                    bag.ItemCountPre += 1;

                    if (!listBags.Contains(bag)) listBags.Add(bag);

                    var blockMilestone = rowTouched % BlockSize;
                    if (blockMilestone == 0)
                    {
                        await dbConn.Bags.AddRangeAsync(listBags.Where(u => u.Id == 0));
                        await dbConn.Items.AddRangeAsync(listItems.Select(u => new EF.Item
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
                        }));

                        await dbConn.Itemmins.AddRangeAsync(listItems.Select(u => new EF.Itemmin
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

                        await dbConn.TrackingNoForUpdates.AddRangeAsync(listItems.Select(u => new TrackingNoForUpdates.TrackingNoForUpdate
                        {
                            TrackingNo = u.TrackingNo,
                            DispatchNo = _dispatchProfile.DispatchNo,
                            ProcessType = TrackingNoForUpdateConst.STATUS_UPDATE,
                        }));

                        var itemTrackings = await dbConn.ItemTrackings.Where(u => u.CustomerCode.Equals(_dispatchProfile.AccNo) &&
                                                                                  u.ProductCode.Equals(_dispatchProfile.ProductCode)
                                                                            ).ToListAsync();

                        var registeredItems = itemTrackings.Where(a => listItems.Any(b => b.TrackingNo == a.TrackingNo)).ToList();

                        foreach (var item in registeredItems)
                        {
                            item.DateUsed = DateTime.Now;
                            item.DispatchId = (int)savedDispatch.Id;
                        }

                        dbConn.ItemTrackings.UpdateRange(registeredItems);

                        bool isExternal = false;

                        if (external_tracking_countries.Any(x => x.Equals(row.CountryCode))) isExternal = true;

                        var unregistered = listItems.Where(b => !itemTrackings.Any(a => a.TrackingNo == b.TrackingNo))
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
                                                        ProductCode = a.ProductCode,
                                                        IsExternal = isExternal
                                                    }).ToList();

                        await dbConn.ItemTrackings.AddRangeAsync(unregistered);

                        dispatch.ImportProgress = Convert.ToInt32(Convert.ToDecimal(rowTouched) / Convert.ToDecimal(itemCount) * 100);

                        await dbConn.SaveChangesAsync().ConfigureAwait(false);

                        //reset and next batch
                        listItems.Clear();
                    }
                }

                #region Remaining
                if (listItems.Count != 0)
                {
                    await dbConn.Bags.AddRangeAsync(listBags.Where(u => u.Id == 0)).ConfigureAwait(false);
                    await dbConn.SaveChangesAsync();

                    await dbConn.Items.AddRangeAsync(listItems.Select(u => new EF.Item
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
                    }));

                    await dbConn.Itemmins.AddRangeAsync(listItems.Select(u => new EF.Itemmin
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

                    await dbConn.TrackingNoForUpdates.AddRangeAsync(listItems.Select(u => new TrackingNoForUpdates.TrackingNoForUpdate
                    {
                        TrackingNo = u.TrackingNo,
                        DispatchNo = _dispatchProfile.DispatchNo,
                        ProcessType = TrackingNoForUpdateConst.STATUS_UPDATE
                    }));

                    var itemTrackings = await dbConn.ItemTrackings.Where(u => u.CustomerCode.Equals(_dispatchProfile.AccNo) &&
                                                                        u.ProductCode.Equals(_dispatchProfile.ProductCode)).ToListAsync();

                    var registeredItems = itemTrackings.Where(a => listItems.Any(b => b.TrackingNo == a.TrackingNo)).ToList();

                    foreach (var item in registeredItems)
                    {
                        item.DateUsed = DateTime.Now;
                        item.DispatchId = (int)savedDispatch.Id;
                        item.DispatchNo = _dispatchProfile.DispatchNo;
                    }

                    dbConn.ItemTrackings.UpdateRange(registeredItems);

                    bool isExternal = false;

                    if (external_tracking_countries.Any(x => x.Equals(listItems[0].CountryCode))) isExternal = true;

                    var unregistered = listItems.Where(b => !itemTrackings.Any(a => a.TrackingNo == b.TrackingNo))
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
                                                    ProductCode = a.ProductCode,
                                                    IsExternal = isExternal
                                                }).ToList();

                    await dbConn.ItemTrackings.AddRangeAsync(unregistered);

                    await dbConn.SaveChangesAsync().ConfigureAwait(false);

                    listItems.Clear();
                }
                #endregion Remaining

                #endregion V2 Excel Import

                #region V1 Excel Import

                // using var reader = ExcelReaderFactory.CreateReader(stream);
                // var rowCount = reader.RowCount;

                // do
                // {
                //     while (reader.Read())
                //     {
                //         if (rowTouched > 0)
                //         {
                //             if (reader[0] is null && ((reader[3] == null ? "" : reader[3].ToString()) != SERVICE_TS || (reader[3] == null ? "" : reader[3].ToString()) != SERVICE_DE)) break;
                //             var strPostalCode = reader[0] == null ? "" : reader[0].ToString();
                //             DateTime.TryParseExact(reader[1].ToString()!, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTimeCell);
                //             var dispatchDate = DateOnly.FromDateTime(dateTimeCell);
                //             var strServiceCode = reader[2] == null ? "" : reader[2].ToString();
                //             var strProductCode = reader[3] == null ? "" : reader[3].ToString();
                //             var bagNo = reader[4] == null ? "" : reader[4].ToString();
                //             var countryCode = reader[5] == null ? "" : reader[5].ToString();
                //             var weight = (reader[6] == null || reader[6].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[6]);
                //             var itemId = reader[7] == null ? "" : reader[7].ToString();
                //             var sealNo = reader[8] == null ? "" : reader[8].ToString();
                //             var strDispatchNo = reader[9] == null ? "" : reader[9].ToString();
                //             var itemValue = (reader[10] == null || reader[10].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[10]);
                //             var itemDesc = reader[11] == null ? "" : reader[11].ToString();
                //             var recpName = reader[12] == null ? "" : reader[12].ToString();
                //             var telNo = reader[13] == null ? "" : reader[13].ToString();
                //             var email = reader[14] == null ? "" : reader[14].ToString();
                //             var address = reader[15] == null ? "" : reader[15].ToString();
                //             var postcode = reader[16] == null ? "" : reader[16].ToString();
                //             var city = reader[17] == null ? "" : reader[17].ToString();
                //             var addressLine2 = reader[18] == null ? "" : reader[18].ToString();
                //             var addressNo = reader[19] == null ? "" : reader[19].ToString();
                //             var identityNo = reader[20] == null ? "" : reader[20].ToString();
                //             var identityType = reader[21] == null ? "" : reader[21].ToString();
                //             var state = reader[22] == null ? "" : reader[22].ToString();
                //             var length = (reader[23] == null || reader[23].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[23]);
                //             var width = (reader[24] == null || reader[24].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[24]);
                //             var height = (reader[25] == null || reader[25].ToString().IsNullOrWhiteSpace()) ? 0 : Convert.ToDecimal(reader[25]);
                //             var taxPaymentMethod = reader[26] == null ? "" : reader[26].ToString();
                //             var hsCode = reader[27] == null ? "" : reader[27].ToString();
                //             var qty = reader[28] == null ? 0 : Convert.ToInt32(reader[28]);

                //             var price = pricer.CalculatePrice(countryCode: countryCode, weight: weight, state: state, city: city, postcode: postcode);

                //             if (pricer.ErrorMsg != "") throw new Exception(pricer.ErrorMsg);

                //             avgItemValue += price;

                //             listItems.Add(new DispatchItemDto
                //             {
                //                 PostalCode = strPostalCode,
                //                 DispatchDate = dispatchDate,
                //                 ServiceCode = strServiceCode,
                //                 ProductCode = strProductCode,
                //                 BagNo = bagNo,
                //                 CountryCode = countryCode,
                //                 Weight = weight,
                //                 TrackingNo = itemId,
                //                 SealNo = sealNo,
                //                 ItemValue = itemValue,
                //                 ItemDesc = itemDesc,
                //                 RecipientName = recpName,
                //                 TelNo = telNo,
                //                 Email = email,
                //                 Address = address,
                //                 Postcode = postcode,
                //                 City = city,
                //                 AddressLine2 = addressLine2,
                //                 AddressNo = addressNo,
                //                 IdentityNo = identityNo,
                //                 IdentityType = identityType,
                //                 State = state,
                //                 Length = length,
                //                 Width = width,
                //                 Height = height,
                //                 TaxPaymentMethod = taxPaymentMethod,
                //                 HSCode = hsCode,
                //                 Qty = qty,
                //                 Price = price
                //             });

                //             itemCount++;
                //             totalWeight += weight;
                //             totalPrice += price;

                //             var bag = listBags.Where(u => u.BagNo == bagNo).FirstOrDefault();
                //             if (bag != null)
                //             {
                //                 bag.WeightPre += weight;
                //                 bag.ItemCountPre += 1;
                //             }
                //             else
                //             {
                //                 listBags.Add(new EF.Bag
                //                 {
                //                     BagNo = bagNo,
                //                     CountryCode = countryCode,
                //                     WeightPre = weight,
                //                     ItemCountPre = 1,
                //                     DispatchId = dispatch.Id
                //                 });
                //             }

                //             var blockMilestone = rowTouched % BlockSize;
                //             if (blockMilestone == 0)
                //             {
                //                 await dbConn.Bags.AddRangeAsync(listBags.Where(u => u.Id == 0)).ConfigureAwait(false);
                //                 await dbConn.SaveChangesAsync();

                //                 await dbConn.Items.AddRangeAsync(listItems.Select(u => new EF.Item
                //                 {
                //                     Id = u.TrackingNo,
                //                     DispatchId = dispatch.Id,
                //                     BagId = listBags.Find(p => p.BagNo == u.BagNo).Id,
                //                     DispatchDate = dispatch.DispatchDate,
                //                     Month = month,
                //                     PostalCode = u.PostalCode,
                //                     ServiceCode = u.ServiceCode,
                //                     ProductCode = u.ProductCode,
                //                     CountryCode = u.CountryCode,
                //                     Weight = u.Weight,
                //                     BagNo = u.BagNo,
                //                     SealNo = u.SealNo,
                //                     Price = u.Price,
                //                     ItemValue = u.ItemValue,
                //                     ItemDesc = u.ItemDesc,
                //                     RecpName = u.RecipientName,
                //                     TelNo = u.TelNo,
                //                     Email = u.Email,
                //                     Address = u.Address,
                //                     Postcode = u.Postcode,
                //                     City = u.City,
                //                     Address2 = u.AddressLine2,
                //                     AddressNo = u.AddressNo,
                //                     State = u.State,
                //                     Length = u.Length,
                //                     Width = u.Width,
                //                     Height = u.Height,
                //                     Hscode = u.HSCode,
                //                     Qty = u.Qty,
                //                     TaxPayMethod = u.TaxPaymentMethod,
                //                     IdentityType = u.IdentityType,
                //                     PassportNo = u.IdentityNo,
                //                     Stage1StatusDesc = "Pre Check",
                //                     DateStage1 = DateTime.Now,
                //                     Status = (int)DispatchEnumConst.Status.Stage1
                //                 })).ConfigureAwait(false);

                //                 await dbConn.Itemmins.AddRangeAsync(listItems.Select(u => new EF.Itemmin
                //                 {
                //                     Id = u.TrackingNo,
                //                     DispatchId = dispatch.Id,
                //                     BagId = listBags.Find(p => p.BagNo == u.BagNo).Id,
                //                     DispatchDate = dispatch.DispatchDate,
                //                     Month = month,
                //                     CountryCode = u.CountryCode,
                //                     Weight = u.Weight,
                //                     ItemValue = u.ItemValue,
                //                     ItemDesc = u.ItemDesc.Truncate(60, ".."),
                //                     RecpName = u.RecipientName.Truncate(30, ".."),
                //                     TelNo = u.TelNo.Truncate(15, ".."),
                //                     Address = u.Address.Truncate(100, ".."),
                //                     City = u.City.Truncate(30, ".."),
                //                     Status = (int)DispatchEnumConst.Status.Stage1
                //                 })).ConfigureAwait(false);

                //                 await dbConn.TrackingNoForUpdates.AddRangeAsync(listItems.Select(u => new TrackingNoForUpdates.TrackingNoForUpdate
                //                 {
                //                     TrackingNo = u.TrackingNo,
                //                     DispatchNo = _dispatchProfile.DispatchNo,
                //                     ProcessType = TrackingNoForUpdateConst.STATUS_UPDATE,
                //                 })).ConfigureAwait(false);

                //                 var itemTrackings = dbConn.ItemTrackings.Where(u => u.CustomerCode.Equals(_dispatchProfile.AccNo) &&
                //                                                                 u.ProductCode.Equals(_dispatchProfile.ProductCode)).ToList();

                //                 var registeredItems = itemTrackings
                //                             .Where(a => listItems.Any(b => b.TrackingNo == a.TrackingNo))
                //                             .ToList();

                //                 foreach (var item in registeredItems)
                //                 {
                //                     item.DateUsed = DateTime.Now;
                //                     item.DispatchId = (int)savedDispatch.Id;
                //                 }

                //                 dbConn.ItemTrackings.UpdateRange(registeredItems);

                //                 bool isExternal = false;

                //                 if (external_tracking_countries.Any(x => x.Equals(countryCode))) isExternal = true;

                //                 var unregistered = listItems
                //                                         .Where(b => !itemTrackings.Any(a => a.TrackingNo == b.TrackingNo))
                //                                         .Select(a => new ItemTracking
                //                                         {
                //                                             TrackingNo = a.TrackingNo,
                //                                             ApplicationId = 0,
                //                                             ReviewId = 0,
                //                                             CustomerCode = customer.Code,
                //                                             CustomerId = customer.Id,
                //                                             DateCreated = DateTime.MinValue,
                //                                             DateUsed = DateTime.Now,
                //                                             DispatchId = (int)savedDispatch.Id,
                //                                             DispatchNo = _dispatchProfile.DispatchNo,
                //                                             ProductCode = a.ProductCode,
                //                                             IsExternal = isExternal
                //                                         })
                //                                         .ToList();

                //                 await dbConn.ItemTrackings.AddRangeAsync(unregistered);

                //                 dispatch.ImportProgress = Convert.ToInt32(Convert.ToDecimal(itemCount / Convert.ToDecimal(rowCount)) * 100);

                //                 await dbConn.SaveChangesAsync().ConfigureAwait(false);

                //                 //reset and next batch
                //                 listItems.Clear();
                //             }
                //         }
                //         rowTouched++;
                //     }
                // } while (reader.NextResult());

                // #region Remaining
                // if (listItems.Count != 0)
                // {
                //     await dbConn.Bags.AddRangeAsync(listBags.Where(u => u.Id == 0)).ConfigureAwait(false);
                //     await dbConn.SaveChangesAsync();

                //     await dbConn.Items.AddRangeAsync(listItems.Select(u => new EF.Item
                //     {
                //         Id = u.TrackingNo,
                //         DispatchId = dispatch.Id,
                //         BagId = listBags.Find(p => p.BagNo == u.BagNo).Id,
                //         DispatchDate = dispatch.DispatchDate,
                //         Month = month,
                //         PostalCode = u.PostalCode,
                //         ServiceCode = u.ServiceCode,
                //         ProductCode = u.ProductCode,
                //         CountryCode = u.CountryCode,
                //         Weight = u.Weight,
                //         BagNo = u.BagNo,
                //         SealNo = u.SealNo,
                //         Price = u.Price,
                //         ItemValue = u.ItemValue,
                //         ItemDesc = u.ItemDesc,
                //         RecpName = u.RecipientName,
                //         TelNo = u.TelNo,
                //         Email = u.Email,
                //         Address = u.Address,
                //         Postcode = u.Postcode,
                //         City = u.City,
                //         Address2 = u.AddressLine2,
                //         AddressNo = u.AddressNo,
                //         State = u.State,
                //         Length = u.Length,
                //         Width = u.Width,
                //         Height = u.Height,
                //         Hscode = u.HSCode,
                //         Qty = u.Qty,
                //         TaxPayMethod = u.TaxPaymentMethod,
                //         IdentityType = u.IdentityType,
                //         PassportNo = u.IdentityNo,
                //         Stage1StatusDesc = "Pre Check",
                //         DateStage1 = DateTime.Now,
                //         Status = (int)DispatchEnumConst.Status.Stage1
                //     })).ConfigureAwait(false);

                //     await dbConn.Itemmins.AddRangeAsync(listItems.Select(u => new EF.Itemmin
                //     {
                //         Id = u.TrackingNo,
                //         DispatchId = dispatch.Id,
                //         BagId = listBags.Find(p => p.BagNo == u.BagNo).Id,
                //         DispatchDate = dispatch.DispatchDate,
                //         Month = month,
                //         CountryCode = u.CountryCode,
                //         Weight = u.Weight,
                //         ItemValue = u.ItemValue,
                //         ItemDesc = u.ItemDesc.Truncate(60, ".."),
                //         RecpName = u.RecipientName.Truncate(30, ".."),
                //         TelNo = u.TelNo.Truncate(15, ".."),
                //         Address = u.Address.Truncate(100, ".."),
                //         City = u.City.Truncate(30, ".."),
                //         Status = (int)DispatchEnumConst.Status.Stage1
                //     })).ConfigureAwait(false);

                //     await dbConn.TrackingNoForUpdates.AddRangeAsync(listItems.Select(u => new TrackingNoForUpdates.TrackingNoForUpdate
                //     {
                //         TrackingNo = u.TrackingNo,
                //         DispatchNo = _dispatchProfile.DispatchNo,
                //         ProcessType = TrackingNoForUpdateConst.STATUS_UPDATE
                //     })).ConfigureAwait(false);

                //     var itemTrackings = dbConn.ItemTrackings.Where(u => u.CustomerCode.Equals(_dispatchProfile.AccNo) &&
                //                                                                 u.ProductCode.Equals(_dispatchProfile.ProductCode)).ToList();

                //     var registeredItems = itemTrackings
                //                             .Where(a => listItems.Any(b => b.TrackingNo == a.TrackingNo))
                //                             .ToList();

                //     foreach (var item in registeredItems)
                //     {
                //         item.DateUsed = DateTime.Now;
                //         item.DispatchId = (int)savedDispatch.Id;
                //         item.DispatchNo = _dispatchProfile.DispatchNo;
                //     }

                //     dbConn.ItemTrackings.UpdateRange(registeredItems);

                //     bool isExternal = false;

                //     if (external_tracking_countries.Any(x => x.Equals(listItems[0].CountryCode))) isExternal = true;

                //     var unregistered = listItems
                //                             .Where(b => !itemTrackings.Any(a => a.TrackingNo == b.TrackingNo))
                //                             .Select(a => new ItemTracking
                //                             {
                //                                 TrackingNo = a.TrackingNo,
                //                                 ApplicationId = 0,
                //                                 ReviewId = 0,
                //                                 CustomerCode = customer.Code,
                //                                 CustomerId = customer.Id,
                //                                 DateCreated = DateTime.MinValue,
                //                                 DateUsed = DateTime.Now,
                //                                 DispatchId = (int)savedDispatch.Id,
                //                                 DispatchNo = _dispatchProfile.DispatchNo,
                //                                 ProductCode = a.ProductCode,
                //                                 IsExternal = isExternal
                //                             })
                //                             .ToList();

                //     await dbConn.ItemTrackings.AddRangeAsync(unregistered);

                //     await dbConn.SaveChangesAsync().ConfigureAwait(false);
                // }
                // #endregion Remaining

                #endregion V1 Excel Import

                dispatch.Status = (int)DispatchEnumConst.Status.Stage1;
                dispatch.IsActive = 1;
                dispatch.NoofBag = listBags.Count;
                dispatch.ItemCount = itemCount;
                dispatch.TotalWeight = Math.Round(totalWeight, 3);
                dispatch.TotalPrice = Math.Round(totalPrice, 2);
                dispatch.ImportProgress = 100;

                var queueTask = dbConn.Queues.Find(_queueId);
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

                await dbConn.Queues.AddAsync(new Queue
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
                });

                var apiUrl = await dbConn.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("APIURL"));

                if (apiUrl != null)
                {
                    try
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
                    catch (Exception ex)
                    {
                        await LogQueueError(new QueueErrorEventArg
                        {
                            FilePath = _filePath,
                            ErrorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message,
                        });
                    }
                }

                await dbConn.SaveChangesAsync().ConfigureAwait(false);

                #endregion Importation
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

        private async Task LogQueueError(QueueErrorEventArg arg)
        {
            using db dbConn = new();
            #region Queue
            var q = dbConn.Queues
                .Where(u => u.FilePath == arg.FilePath)
                .Where(u => u.Status == QueueEnumConst.STATUS_RUNNING)
                .FirstOrDefault();

            if (q != null)
            {
                q.Status = QueueEnumConst.STATUS_ERROR;
                q.ErrorMsg = arg.ErrorMsg;
                q.TookInSec = 0;

                await dbConn.SaveChangesAsync().ConfigureAwait(false);
            }
            #endregion
        }

        private async Task DeleteDispatch(string dispatchNo)
        {
            using db dbConn = new();
            dbConn.Dispatches.RemoveRange(dbConn.Dispatches.Where(u => u.DispatchNo == dispatchNo).Where(u => u.IsActive == 0).ToList());

            await dbConn.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}


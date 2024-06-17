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
using SND.SMP.ItemTrackingReviews.Dto;
using SND.SMP.ItemTrackingApplications;
using Abp.EntityFrameworkCore.Repositories;
using SND.SMP.ItemTrackings;
using SND.SMP.ItemIdRunningNos;
using Microsoft.AspNetCore.Mvc;
using SND.SMP.Customers;
using SND.SMP.Dispatches;
using SND.SMP.Items;
using SND.SMP.Postals;
using System.Net.Http;
using System.IO;
using System.Data;
using OfficeOpenXml;
using System.Security.Cryptography;
using System.Text;


namespace SND.SMP.ItemTrackingReviews
{
    public class ItemTrackingReviewAppService(
        IRepository<ItemTrackingReview, int> repository,
        IRepository<ItemTrackingApplication, int> itemTrackingApplicationRepository,
        IRepository<ItemTracking, int> itemTrackingRepository,
        IRepository<ItemIdRunningNo, long> itemIdRunningNoRepository,
        IRepository<ItemTrackingReview, int> itemTrackingReviewRepository,
        IRepository<Customer, long> customerRepository,
        IRepository<Dispatch, int> dispatchRepository,
        IRepository<Item, string> itemRepository,
        IRepository<Postal, long> postalRepository
    ) : AsyncCrudAppService<ItemTrackingReview, ItemTrackingReviewDto, int, PagedItemTrackingReviewResultRequestDto>(repository)
    {
        private readonly IRepository<ItemTrackingApplication, int> _itemTrackingApplicationRepository = itemTrackingApplicationRepository;
        private readonly IRepository<ItemTracking, int> _itemTrackingRepository = itemTrackingRepository;
        private readonly IRepository<ItemIdRunningNo, long> _itemIdRunningNoRepository = itemIdRunningNoRepository;
        private readonly IRepository<ItemTrackingReview, int> _itemTrackingReviewRepository = itemTrackingReviewRepository;
        private readonly IRepository<Customer, long> _customerRepository = customerRepository;
        private readonly IRepository<Dispatch, int> _dispatchRepository = dispatchRepository;
        private readonly IRepository<Item, string> _itemRepository = itemRepository;
        private readonly IRepository<Postal, long> _postalRepository = postalRepository;

        protected override IQueryable<ItemTrackingReview> CreateFilteredQuery(PagedItemTrackingReviewResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.CustomerCode.Contains(input.Keyword) ||
                    x.PostalCode.Contains(input.Keyword) ||
                    x.PostalDesc.Contains(input.Keyword) ||
                    x.Status.Contains(input.Keyword) ||
                    x.Prefix.Contains(input.Keyword) ||
                    x.PrefixNo.Contains(input.Keyword) ||
                    x.Suffix.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword));
        }

        public override async Task<ItemTrackingReviewDto> CreateAsync(ItemTrackingReviewDto input)
        {
            CheckCreatePermission();

            var entity = MapToEntity(input);

            await Repository.InsertAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();

            var application = await _itemTrackingApplicationRepository.FirstOrDefaultAsync(x => x.Id.Equals(input.ApplicationId));
            application.Status = input.Status;
            await _itemTrackingApplicationRepository.UpdateAsync(application);
            await _itemTrackingApplicationRepository.GetDbContext().SaveChangesAsync();

            return MapToEntityDto(entity);
        }

        public async Task<bool> UndoReview(int applicationId)
        {
            var application = await _itemTrackingApplicationRepository.FirstOrDefaultAsync(x => x.Id.Equals(applicationId));

            application.Status = "Pending";
            application.TookInSec = 0;
            application.Range = "";

            await _itemTrackingApplicationRepository.UpdateAsync(application);

            var review = await _itemTrackingReviewRepository.FirstOrDefaultAsync(x => x.ApplicationId.Equals(applicationId));
            var runningNo = await _itemIdRunningNoRepository.FirstOrDefaultAsync(x =>
                                                                                    x.Customer.Equals(review.CustomerCode) &&
                                                                                    x.Prefix.Equals(review.Prefix) &&
                                                                                    x.PrefixNo.Equals(review.PrefixNo) &&
                                                                                    x.Suffix.Equals(review.Suffix)
                                                                                );

            runningNo.RunningNo = 0;

            await _itemIdRunningNoRepository.UpdateAsync(runningNo);

            await Repository.DeleteAsync(review);

            return true;
        }

        public async Task<ReviewAmount> GetReviewAmount(int applicationId)
        {
            var review = await Repository.FirstOrDefaultAsync(x => x.ApplicationId.Equals(applicationId));

            var tracking = await _itemTrackingRepository.GetAllListAsync(x => x.ApplicationId.Equals(applicationId));

            return new ReviewAmount()
            {
                Issued = review is null ? "0" : review.TotalGiven.ToString(),
                Remaining = review is null ? "0" : (review.TotalGiven - tracking.Count).ToString(),
                Uploaded = tracking.Count.ToString(),
            };
        }

        public static string GenerateMD5Hash(string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = MD5.HashData(inputBytes);

            // Convert the byte array to a hexadecimal string.
            StringBuilder sb = new();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }

            return sb.ToString();
        }

        [HttpPost]
        [Route("api/PreRegisterItem/KG")]
        public async Task<OutPreRegisterItem> PreRegisterItemKG(InPreRegisterItem input)
        {
            return await PreRegisterItem(input, "KG");
        }

        [HttpPost]
        [Route("api/PreRegisterItem/GQ")]
        public async Task<OutPreRegisterItem> PreRegisterItemGQ(InPreRegisterItem input)
        {
            return await PreRegisterItem(input, "GQ");
        }

        [HttpPost]
        [Route("api/PreRegisterItem/SL")]
        public async Task<OutPreRegisterItem> PreRegisterItemSL(InPreRegisterItem input)
        {
            return await PreRegisterItem(input, "SL");
        }

        [HttpGet]
        public async Task<OutPreRegisterItem> PreRegisterItem(InPreRegisterItem input, string postal)
        {
            const string SUCCESS = "success";
            const string FAILED = "failed";

            string auto = "auto";

            string accNo = "";
            string clientSecret = "";

            int threshold = 50000;

            var result = new OutPreRegisterItem();

            var newResponseIDRaw = Guid.NewGuid().ToString();

            result.ResponseID = GenerateMD5Hash(newResponseIDRaw);
            result.RefNo = input.RefNo;
            //result.ItemID = input.ItemID;
            result.Status = FAILED;
            result.Errors = [];
            result.APIItemID = "";

            string postalSupported = postal[..2];

            string postalCode = postalSupported;

            if (input.PostalCode.Contains(postalSupported))
            {
                var cust = await _customerRepository.FirstOrDefaultAsync(u => u.ClientKey == input.ClientKey);

                if (cust != null)
                {
                    accNo = cust.Code;
                    clientSecret = cust.ClientSecret;

                    string signHashRequest = input.SignatureHash.Trim().ToUpper();
                    string signHashRaw = string.Format("{0}-{1}-{2}-{3}", input.ItemID, input.RefNo, input.ClientKey, clientSecret);
                    string signHashServer = GenerateMD5Hash(signHashRaw).Trim().ToUpper();

                    var signMatched = signHashRequest == signHashServer;

                    if (signMatched)
                    {
                        #region Validation

                        #region Recipient Country
                        var willValidateCountry = false;
                        if (willValidateCountry)
                        {
                            if (input.RecipientCountry.ToUpper().Trim() == postalSupported)
                            {
                                result.Errors.Add($"Invalid country {input.RecipientCountry} for this service");
                            }
                        }
                        #endregion

                        #region Service Code
                        if (!string.Equals(input.ServiceCode.Trim(), "TS", StringComparison.OrdinalIgnoreCase))
                        {
                            result.Errors.Add("Invalid service code");
                        }
                        #endregion

                        if (postalSupported == "KG")
                        {
                            #region Product Code
                            if (!string.Equals(input.ProductCode.Trim(), "OMT", StringComparison.OrdinalIgnoreCase) && !string.Equals(input.ProductCode.Trim(), "PRT", StringComparison.OrdinalIgnoreCase))
                            {
                                result.Errors.Add("Invalid product code");
                            }
                            #endregion

                            #region IOSS Tax
                            var willValidateIOSS = true;
                            if (willValidateIOSS)
                            {
                                var countryListIOSS = new List<string> { "IE", "HU", "LU", "CZ" };

                                if (countryListIOSS.Contains(input.RecipientCountry.ToUpper().Trim()))
                                {
                                    if (string.IsNullOrWhiteSpace(input.IOSSTax))
                                    {
                                        result.Errors.Add($"IOSSTax is mandatory for {input.RecipientCountry}");
                                    }
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region Product Code
                            if (!string.Equals(input.ProductCode.Trim(), "OMT", StringComparison.OrdinalIgnoreCase))
                            {
                                result.Errors.Add("Invalid product code");
                            }
                            #endregion
                        }

                        var isItemIDAutoMandatory = true;
                        if (isItemIDAutoMandatory)
                        {
                            if ((string.IsNullOrWhiteSpace(input.ItemID) ? "" : input.ItemID.ToLower().Trim()) != auto.ToLower().Trim())
                            {
                                result.Errors.Add("Invalid ItemID value. ItemID must be set to 'auto'");
                            }
                        }

                        if (!postalSupported.Equals("GQ"))
                        {
                            var nextItemIdFromRange = await GetNextAvailableAnyAccountTrackingNumber(postalCode);
                            if (string.IsNullOrWhiteSpace(nextItemIdFromRange))
                            {
                                result.Errors.Add("Insufficient API Item ID. Please contact us for assistance");
                            }
                        }

                        #endregion

                        if (!string.IsNullOrWhiteSpace(input.PoolItemId))
                        {
                            var isOwned = await IsTrackingNoOwner(accNo, input.PoolItemId, input.ProductCode);
                            if (!isOwned)
                            {
                                result.Errors.Add("Invalid Pool Item ID");
                            }
                        }

                        if (result.Errors.Count == 0)
                        {
                            string dispNo = string.Format("TempDisp-{0}-{1}-{2}-{3}", accNo, input.PostalCode, input.ServiceCode, input.ProductCode);

                            var dispatchTemp = await _dispatchRepository.FirstOrDefaultAsync(x =>
                                                                                                x.DispatchNo.Equals(dispNo) &&
                                                                                                x.CustomerCode.Equals(accNo)
                                                                                            );

                            if (dispatchTemp == null)
                            {
                                dispatchTemp = await _dispatchRepository.InsertAsync(new Dispatch
                                {
                                    DispatchNo = dispNo,
                                    CustomerCode = accNo,
                                    PostalCode = input.PostalCode,
                                    ServiceCode = input.ServiceCode,
                                    ProductCode = input.ProductCode,
                                    DispatchDate = null,
                                    BatchId = "",
                                    TransactionDateTime = DateTime.Now
                                });

                                await _dispatchRepository.GetDbContext().SaveChangesAsync();
                            }

                            string newItemIdFromSPS = null;

                            if (!string.IsNullOrWhiteSpace(input.PoolItemId))
                            {
                                newItemIdFromSPS = input.PoolItemId;
                                input.ItemID = newItemIdFromSPS;
                                //Nothing to generate because the pool item ID already assigned to this account no.
                            }
                            else
                            {
                                #region New Item ID from SPS
                                if ((string.IsNullOrWhiteSpace(input.ItemID) ? "" : input.ItemID).ToLower().Trim() == auto.ToLower().Trim())
                                {
                                    newItemIdFromSPS = await GetNextAvailableAnyAccountTrackingNumber(postalCode, false, postalCode = postalSupported == "KG" ? input.ProductCode : null);

                                    if (!string.IsNullOrWhiteSpace(newItemIdFromSPS))
                                    {
                                        try
                                        {
                                            await InsertUpdateTrackingNumber(newItemIdFromSPS, accNo, cust.Id, input.PostalCode);

                                            await AlertIfLowThreshold(postalCode, threshold);

                                            result.ItemID = newItemIdFromSPS;
                                            result.Status = SUCCESS;
                                        }
                                        catch (Exception ex)
                                        {
                                            result.Status = FAILED;
                                            result.Errors.Add(ex.Message);
                                        }
                                    }

                                    input.ItemID = newItemIdFromSPS;
                                }
                                else
                                {
                                    if (isItemIDAutoMandatory)
                                    {
                                        newItemIdFromSPS = await GetNextAvailableAnyAccountTrackingNumber(postalCode, false, input.PostalCode);

                                        if (!string.IsNullOrWhiteSpace(newItemIdFromSPS))
                                        {
                                            try
                                            {
                                                await InsertUpdateTrackingNumber(newItemIdFromSPS, accNo, cust.Id, input.PostalCode);

                                                await AlertIfLowThreshold(postalCode, threshold);

                                                result.ItemID = newItemIdFromSPS;
                                                result.Status = SUCCESS;
                                            }
                                            catch (Exception ex)
                                            {
                                                result.Status = FAILED;
                                                result.Errors.Add(ex.Message);
                                            }
                                        }

                                        input.ItemID = newItemIdFromSPS;
                                    }
                                }
                                #endregion
                            }

                            var newItem = await _itemRepository.FirstOrDefaultAsync(x =>
                                                                                        x.DispatchID.Equals(dispatchTemp.Id) &&
                                                                                        x.Id.Equals(input.ItemID)
                                                                                   );

                            if (newItem is null)
                            {
                                newItem = await _itemRepository.InsertAsync(new Item
                                {
                                    Id = newItemIdFromSPS,
                                    DispatchID = dispatchTemp.Id,
                                    BagID = null,
                                    DispatchDate = dispatchTemp.DispatchDate,
                                    Month = 0,
                                    PostalCode = input.ServiceCode,
                                    ServiceCode = input.ServiceCode,
                                    ProductCode = input.ProductCode,
                                    CountryCode = input.RecipientCountry,
                                    Weight = input.Weight,
                                    BagNo = "",
                                    SealNo = "",
                                    Price = 0m,
                                    ItemValue = input.ItemValue,
                                    ItemDesc = input.ItemDesc,
                                    RecpName = input.RecipientName,
                                    TelNo = input.RecipientContactNo,
                                    Email = input.RecipientEmail,
                                    Address = input.RecipientAddress,
                                    Postcode = input.RecipientPostcode,
                                    City = input.RecipientCity,
                                    Address2 = "",
                                    AddressNo = "",
                                    State = input.RecipientState,
                                    Length = 0,
                                    Width = 0,
                                    Height = 0,
                                    Qty = 0,
                                    TaxPayMethod = "",
                                    IdentityType = "",
                                    PassportNo = input.IdentityNo
                                });

                                #region Item Topup Value
                                var itemTopupValue = await GetItemTopupValueFromPostalMaintenance(input.PostalCode, input.ServiceCode, input.ProductCode);
                                newItem.ItemValue = newItem.ItemValue is null ? 0m + itemTopupValue : (decimal)newItem.ItemValue + itemTopupValue;

                                #endregion

                            }
                            else
                            {
                                newItem.Weight = input.Weight;
                                newItem.ItemValue = input.ItemValue;
                                newItem.ItemDesc = input.ItemDesc;
                                newItem.RecpName = input.RecipientName;
                                newItem.TelNo = input.RecipientContactNo;
                                newItem.Email = input.RecipientEmail;
                                newItem.Address = input.RecipientAddress;
                                newItem.City = input.RecipientCity;
                                newItem.Postcode = input.RecipientPostcode;
                                newItem.CountryCode = input.RecipientCountry;
                                newItem.RefNo = input.RefNo;
                                newItem.HSCode = input.HSCode;
                                newItem.SenderName = input.SenderName;
                                newItem.IOSSTax = input.IOSSTax;
                                newItem.AddressNo = input.AddressNo;
                                newItem.PassportNo = input.IdentityNo;
                                newItem.IdentityType = input.IdentityType;

                                #region Item Topup Value
                                var itemTopupValue = await GetItemTopupValueFromPostalMaintenance(input.PostalCode, input.ServiceCode, input.ProductCode);
                                newItem.ItemValue = newItem.ItemValue is null ? 0m + itemTopupValue : (decimal)newItem.ItemValue + itemTopupValue;
                                #endregion


                            }

                            // var item = await _itemRepository.FirstOrDefaultAsync(u => u.Id == newItem.Id);

                            // if (item != null)
                            // {
                            string apiItemId = newItem.Id;

                            if (!string.IsNullOrWhiteSpace(apiItemId))
                            {
                                result.APIItemID = apiItemId;

                                result.Status = SUCCESS;
                                result.Errors.Clear();

                                //Log.Add($"PreReg-KG GetExisting for { input.RefNo } - { input.ItemID }", $"input [{ input.ItemID } - { input.ItemDesc } - { input.RecipientName } - { input.Weight }] get { result.APIItemID }");
                            }
                            else
                            {
                                var newId = newItem.Id;

                                apiItemId = newItem.Id;

                                result.APIItemID = apiItemId;

                                result.Status = SUCCESS;
                                result.Errors.Clear();

                                //Log.Add($"PreReg-KG Success for { input.RefNo } - { input.ItemID }", $"input [{ input.ItemID } - { input.ItemDesc } - { input.RecipientName } - { input.Weight }] get { result.APIItemID }");
                            }
                        }
                        else
                        {
                            result.Status = FAILED;
                        }
                    }
                    else
                    {
                        result.Errors.Add("Signature hash is invalid");
                    }
                }
                else
                {
                    result.Errors.Add("Client key is invalid");
                }
            }
            else
            {
                result.Errors.Add("Postal not supported");
            }

            string signHashRespRaw = string.Format("{0}-{1}-{2}-{3}-{4}", result.ResponseID, result.RefNo, result.APIItemID, input.ClientKey, clientSecret);
            string signHashRespServer = GenerateMD5Hash(signHashRespRaw);

            result.SignatureHash = signHashRespServer;

            #region Pool Item ID - Don't return API Item ID output
            if (!string.IsNullOrWhiteSpace(input.PoolItemId))
            {
                result.APIItemID = null;
            }
            #endregion

            return result;
        }

        public static async Task<Stream> GetFileStream(string url)
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

        public async Task<bool> IsTrackingNoOwner(string accNo, string trackingNo, string productCode)
        {
            var itemtrackingid = await _itemTrackingRepository.FirstOrDefaultAsync(x =>
                                                                                x.TrackingNo.Equals(trackingNo) &&
                                                                                x.ProductCode.Equals(productCode) &&
                                                                                x.CustomerCode.Equals(accNo)
                                                                             );

            return itemtrackingid != null;
        }
        public async Task<ItemIds> GetItemTrackingFile(string customerCode, string trackingNo = "", string postalCode = null, string productCode = null)
        {
            List<ItemTrackingReview> reviews = [];
            if (!string.IsNullOrWhiteSpace(trackingNo))
            {
                string prefix = trackingNo[..2];
                string prefixNo = trackingNo.Substring(2, 2);
                string suffix = trackingNo[^2..];

                reviews = await _itemTrackingReviewRepository.GetAllListAsync(x =>
                                                                                    x.Prefix.Equals(prefix) &&
                                                                                    x.PrefixNo.Equals(prefixNo) &&
                                                                                    x.Suffix.Equals(suffix) &&
                                                                                    x.CustomerCode.Equals(customerCode)
                                                                                 );
            }
            else
            {
                reviews = await _itemTrackingReviewRepository.GetAllListAsync();
                reviews = [.. reviews.WhereIf(!string.IsNullOrWhiteSpace(postalCode), x => x.PostalCode.Equals(postalCode))];
                reviews = [.. reviews.WhereIf(!string.IsNullOrWhiteSpace(productCode), x => x.ProductCode.Equals(productCode))];
            }

            List<string> paths = [];
            List<ItemTrackingWithPath> ItemWithPath = [];
            string itemIdFilePath = "";

            var applications = await _itemTrackingApplicationRepository.GetAllListAsync();

            if (reviews.Count > 0)
            {
                foreach (var review in reviews)
                {
                    var application = applications.FirstOrDefault(x => x.Id.Equals(review.ApplicationId));

                    if (application is not null) paths.Add(application.Path);
                }

                if (!paths.Count.Equals(0))
                {
                    foreach (var path in paths)
                    {
                        ItemTrackingWithPath itemWithPath = new();
                        Stream excel_stream = await GetFileStream(path);
                        DataTable dataTable = ConvertToDatatable(excel_stream);

                        List<ItemTrackingIdDto> items = [];
                        foreach (DataRow dr in dataTable.Rows)
                        {
                            if (dr.ItemArray[0].ToString() != "")
                            {
                                items.Add(new ItemTrackingIdDto()
                                {
                                    TrackingNo = dr.ItemArray[0].ToString(),
                                    DateCreated = dr.ItemArray[1].ToString(),
                                    DateUsed = dr.ItemArray[2].ToString(),
                                    DispatchNo = dr.ItemArray[3].ToString(),
                                });

                                if (dr.ItemArray[0].ToString().Equals(trackingNo)) itemIdFilePath = path;
                            }
                        }


                        var application = applications.FirstOrDefault(x => x.Path.Equals(path));
                        var review = reviews.FirstOrDefault(x => x.ApplicationId.Equals(application.Id));

                        itemWithPath.ItemIds = items;
                        itemWithPath.ExcelPath = path;
                        itemWithPath.ApplicationId = application.Id;
                        itemWithPath.ReviewId = review.Id;
                        itemWithPath.CustomerId = application.CustomerId;
                        itemWithPath.CustomerCode = application.CustomerCode;
                        itemWithPath.DateCreated = application.DateCreated;
                        itemWithPath.ProductCode = application.ProductCode;

                        ItemWithPath.Add(itemWithPath);
                    }
                }
            }

            return new ItemIds()
            {
                ItemWithPath = ItemWithPath,
                Path = itemIdFilePath
            };
        }

        public async Task<UnusedItemIds> GetUnusedAnyAccountTrackingId(string trackingNo = "", string postalCode = null, string productCode = null)
        {
            var anyAccountItemIds = await GetItemTrackingFile("Any Account", trackingNo, postalCode, productCode);

            var usedTrackingIdList = await _itemTrackingRepository.GetAllListAsync();

            usedTrackingIdList.Where(x => anyAccountItemIds.ItemWithPath.Any(y => y.ItemIds.Any(z => z.TrackingNo.Equals(x.TrackingNo, StringComparison.Ordinal)))).ToList();

            List<ItemTrackingWithPath> itemWithPath = [];
            List<string> unusedList = [];
            foreach (var itemIdWithPath in anyAccountItemIds.ItemWithPath)
            {
                List<ItemTrackingIdDto> ItemIds = [];
                foreach (var item in itemIdWithPath.ItemIds)
                {
                    bool contains = usedTrackingIdList.Any(x => x.TrackingNo.Equals(item.TrackingNo));
                    if (!contains)
                    {
                        ItemIds.Add(item);
                        unusedList.Add(item.TrackingNo);
                    }
                }
                itemIdWithPath.ItemIds = ItemIds;
                itemWithPath.Add(itemIdWithPath);
            }

            return new UnusedItemIds()
            {
                ItemWithPath = itemWithPath,
                UnusedList = unusedList,
            };
        }

        public async Task<string> GetNextAvailableAnyAccountTrackingNumber(string postalCode, bool? willUpdate = false, string productCode = null)
        {
            string result = null;

            var anyAccountLists = await GetUnusedAnyAccountTrackingId("", postalCode, productCode);

            var randomIndex = 0;
            var count = anyAccountLists.UnusedList.Count;

            if (count > 1)
            {
                var maxRan = count - 1;
                randomIndex = new Random().Next(0, maxRan);
            }

            var randomTrackingNo = anyAccountLists.UnusedList
                .OrderBy(u => u)
                .Skip(randomIndex)
                .Take(1)
                .FirstOrDefault();

            var itemPath = anyAccountLists.ItemWithPath.FirstOrDefault(x => x.ItemIds.Any(y => y.TrackingNo.Equals(randomTrackingNo)));

            if (randomTrackingNo is not null)
            {
                result = randomTrackingNo;

                if (willUpdate.GetValueOrDefault())
                {
                    await _itemTrackingRepository.InsertAsync(new ItemTracking()
                    {
                        TrackingNo = randomTrackingNo,
                        ApplicationId = itemPath.ApplicationId,
                        ReviewId = itemPath.ReviewId,
                        CustomerId = itemPath.CustomerId,
                        CustomerCode = itemPath.CustomerCode,
                        DateCreated = itemPath.DateCreated,
                        ProductCode = itemPath.ProductCode,
                    });
                }
            }

            return result;
        }

        public async Task<decimal> GetItemTopupValueFromPostalMaintenance(string postalCode, string serviceCode, string productCode)
        {
            var postal = await _postalRepository.FirstOrDefaultAsync(x =>
                                                                        x.PostalCode.Equals(postalCode) &&
                                                                        x.ServiceCode.Equals(serviceCode) &&
                                                                        x.ProductCode.Equals(productCode)
                                                                    );

            return postal is not null ? postal.ItemTopUpValue : 0m;
        }

        public async Task InsertUpdateTrackingNumber(string trackingNo, string customerCode, long customerId, string postalCode)
        {
            var item = await _itemTrackingRepository.FirstOrDefaultAsync(x => x.TrackingNo.Equals(trackingNo));

            if (item != null)
            {
                item.CustomerCode = customerCode;
                item.CustomerId = customerId;
                item.ProductCode = postalCode;
            }
            else
            {
                var itemIdDetails = await GetItemTrackingFile(customerCode, trackingNo);
                var matched = itemIdDetails.ItemWithPath.FirstOrDefault(x => x.ExcelPath.Equals(itemIdDetails.Path));

                await _itemTrackingRepository.InsertAsync(new ItemTracking()
                {
                    TrackingNo = trackingNo,
                    ApplicationId = matched.ApplicationId,
                    ReviewId = matched.ReviewId,
                    CustomerId = matched.CustomerId,
                    CustomerCode = matched.CustomerCode,
                    DateCreated = matched.DateCreated,
                    ProductCode = matched.ProductCode,
                });
            }
        }

        public static async Task AlertIfLowThreshold(string postalCode, int threshold, string productCode = null)
        {
            // threshold = 50000; //default

            // if (postalCode == "GE" && (productCode ?? "") == "R")
            // {
            //     threshold = 2500;
            // }
            // if (postalCode == "GE" && (productCode ?? "") == "PRT")
            // {
            //     threshold = 2500;
            // }
            // if (postalCode == "GE" && (productCode ?? "") == "OMT")
            // {
            //     threshold = 10000;
            // }
            // if (postalCode == "KG")
            // {
            //     threshold = 50000;
            // }
            // if (postalCode == "CO")
            // {
            //     threshold = 50000;
            // }
            // if (postalCode == "SL")
            // {
            //     threshold = 50000;
            // }
            // if (postalCode == "GQ")
            // {
            //     threshold = 50000;
            // }

            // var q = await _itemTrackingRepository.RegisterTrackingNumbers
            //     .Where(u => u.PostalID == postalCode)
            //     .Where(u => u.AccountNo == null)
            //     .Where(u => u.DateUsed == null);

            // if (!string.IsNullOrWhiteSpace(productCode))
            // {
            //     q = q.Where(u => u.ProductCode == productCode);
            // }

            // var count = q.Count();

            // if (count == threshold)
            // {
            //     //alert
            //     #region Email to Admins
            //     var enableEmailNotificationToAdmin = true;
            //     if (enableEmailNotificationToAdmin)
            //     {
            //         try
            //         {
            //             string strHost = System.Web.Configuration.WebConfigurationManager.AppSettings["EmailHost"];
            //             string strFromEmail = System.Web.Configuration.WebConfigurationManager.AppSettings["EmailFrom"];
            //             string strPassEmailFrom = System.Web.Configuration.WebConfigurationManager.AppSettings["EmailFromPss"];
            //             string strEmailPort = System.Web.Configuration.WebConfigurationManager.AppSettings["EmailFromPort"];
            //             string subject = $"{postalCode} API Item ID Low Threshold Hit";
            //             string emailContent = $"Dear admins, API Item ID range for {postalCode} {(string.IsNullOrWhiteSpace(productCode) ? "" : productCode)} has just hit the low threshold of {threshold.ToString()}. Kindly take action on this matter.";
            //             string strAdminEmail = "";

            //             strAdminEmail = Services.Common.SystemHelper.APIEmails;

            //             if (string.IsNullOrEmpty(strAdminEmail))
            //             {
            //                 strAdminEmail = string.Join(",", db.Users.Where(u => u.IsSuperAdmin == true).Where(u => u.Email != null && u.Email.Trim() != "").Select(u => u.Email).ToArray());
            //             }

            //             Services.Common.Email.SendMail(subject, emailContent, strFromEmail, strAdminEmail, strPassEmailFrom, strHost, strEmailPort, null, "", "");
            //         }
            //         catch (Exception exEmail)
            //         {

            //         }
            //     }
            //     #endregion
            // }
        }

    }
}


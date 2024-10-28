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
using Abp.UI;
using SND.SMP.ApplicationSettings;
using SND.SMP.APIRequestResponses;
using SND.SMP.Bags;
using SND.SMP.RateZones;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json;
using SND.SMP.SAParams;


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
        IRepository<Postal, long> postalRepository,
        IRepository<ApplicationSetting, int> applicationSettingRepository,
        IRepository<APIRequestResponse, long> apiRequestResponseRepository,
        IRepository<Bag, int> bagRepository,
        IRepository<RateZone, long> rateZoneRepository,
        IRepository<SAParam, long> saParamRepository
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
        private readonly IRepository<ApplicationSetting, int> _applicationSettingRepository = applicationSettingRepository;
        private readonly IRepository<APIRequestResponse, long> _apiRequestResponseRepository = apiRequestResponseRepository;
        private readonly IRepository<Bag, int> _bagRepository = bagRepository;
        private readonly IRepository<RateZone, long> _rateZoneRepository = rateZoneRepository;
        private readonly IRepository<SAParam, long> _saParamRepository = saParamRepository;
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
            await CurrentUnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            var application = await _itemTrackingApplicationRepository.FirstOrDefaultAsync(x => x.Id.Equals(input.ApplicationId));
            application.Status = input.Status;
            await _itemTrackingApplicationRepository.UpdateAsync(application);
            await _itemTrackingApplicationRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

            return MapToEntityDto(entity);
        }
        public async Task<bool> UndoReview(int applicationId)
        {
            var application = await _itemTrackingApplicationRepository.FirstOrDefaultAsync(x => x.Id.Equals(applicationId));

            application.Status = "Pending";
            application.TookInSec = 0;
            application.Range = "";

            await _itemTrackingApplicationRepository.UpdateAsync(application).ConfigureAwait(false);

            var review = await _itemTrackingReviewRepository.FirstOrDefaultAsync(x => x.ApplicationId.Equals(applicationId));
            var runningNo = await _itemIdRunningNoRepository.FirstOrDefaultAsync(x =>
                                                                                    x.Customer.Equals(review.CustomerCode) &&
                                                                                    x.Prefix.Equals(review.Prefix) &&
                                                                                    x.PrefixNo.Equals(review.PrefixNo) &&
                                                                                    x.Suffix.Equals(review.Suffix)
                                                                                );

            runningNo.RunningNo = 0;

            await _itemIdRunningNoRepository.UpdateAsync(runningNo).ConfigureAwait(false);

            await Repository.DeleteAsync(review).ConfigureAwait(false);

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
        public async Task<ItemInfo> GetItem(string trackingNo, bool details, bool tracking)
        {
            var itemtracking = await _itemTrackingRepository.FirstOrDefaultAsync(x => x.TrackingNo.Equals(trackingNo)) ?? throw new UserFriendlyException("No Tracking Found.");

            bool IsExternal = itemtracking.IsExternal;

            ItemInfo itemInfo = new()
            {
                itemDetails = null,
                trackingDetails = []
            };

            if (details)
            {
                var items = await _itemRepository.GetAllListAsync(x => x.Id.Equals(trackingNo));

                if (items.Count == 0) throw new UserFriendlyException("No Item under this Tracking Id is found.");

                Item foundItem = null;
                Bag bag = null;
                Dispatch dispatch = null;

                foreach (var item in items)
                {
                    var temp_dispatch = await _dispatchRepository.FirstOrDefaultAsync(x => x.Id.Equals(item.DispatchID));
                    if (temp_dispatch is not null)
                    {
                        bool isTemp = false;
                        if (temp_dispatch.DispatchNo.Contains("temp", StringComparison.CurrentCultureIgnoreCase)) isTemp = true;

                        foundItem = item;
                        bag = isTemp ? null : await _bagRepository.FirstOrDefaultAsync(x => x.Id.Equals(item.BagID));
                        dispatch = temp_dispatch;
                    }
                }

                var postal = await _postalRepository.FirstOrDefaultAsync(x =>
                                                                        x.PostalCode.Equals(dispatch.PostalCode) &&
                                                                        x.ServiceCode.Equals(dispatch.ServiceCode) &&
                                                                        x.ProductCode.Equals(dispatch.ProductCode)
                                                                    );
                string ProductDesc = "";
                string ServiceDesc = "";
                if (postal is null)
                {
                    var postals = await _postalRepository.GetAllListAsync();
                    var postalDistinctedByProductCode = postals.DistinctBy(x => x.ProductCode).ToList();
                    var postalDistinctedByServiceCode = postals.DistinctBy(x => x.ServiceCode).ToList();
                    ProductDesc = postalDistinctedByProductCode.FirstOrDefault(x => x.ProductCode.Equals(dispatch.ProductCode)).ProductDesc;
                    ServiceDesc = postalDistinctedByServiceCode.FirstOrDefault(x => x.ServiceCode.Equals(dispatch.ServiceCode)).ServiceDesc;
                }

                string address = foundItem.Address is null ? "" : foundItem.Address + ", ";
                string city = foundItem.City is null ? "" : foundItem.City + ", ";
                string state = foundItem.State is null ? "" : foundItem.State + ", ";
                string postcode = foundItem.Postcode is null ? "" : foundItem.Postcode + ", ";
                string country = foundItem.CountryCode is null ? "" : foundItem.CountryCode;
                string addressString = string.Format("{0} {1} {2} {3} {4}", address, city, state, postcode, country);

                itemInfo.itemDetails = new()
                {
                    TrackingNo = trackingNo,
                    DispatchNo = dispatch.DispatchNo,
                    BagNo = bag is null ? "" : bag.BagNo,
                    DispatchDate = $"{dispatch.DispatchDate:dd/MM/yyyy}",
                    Postal = postal is null ? dispatch.PostalCode : postal.PostalDesc,
                    Service = postal is null ? ServiceDesc : postal.ServiceDesc,
                    Product = postal is null ? ProductDesc : postal.ProductDesc,
                    Country = foundItem.CountryCode,
                    Weight = foundItem.Weight is null ? 0.000m : (decimal)foundItem.Weight,
                    Status = foundItem.Status is null ? 0 : (int)foundItem.Status,
                    Value = foundItem.ItemValue is null ? 0.00m : (decimal)foundItem.ItemValue,
                    Description = foundItem.ItemDesc is null ? "" : foundItem.ItemDesc,
                    ReferenceNo = foundItem.RefNo is null ? "" : foundItem.RefNo,
                    Recipient = foundItem.RecpName is null ? "" : foundItem.RecpName,
                    ContactNo = foundItem.TelNo is null ? "" : foundItem.TelNo,
                    Email = foundItem.Email is null ? "" : foundItem.Email,
                    Address = addressString,
                };
            }

            if (tracking)
            {
                if (IsExternal)
                {
                    List<string> trackingNos = [];
                    trackingNos.Add(trackingNo);
                    //Call External
                }
                else
                {
                    var item = await _itemRepository.FirstOrDefaultAsync(x => x.Id.Equals(trackingNo));

                    if (item is not null)
                    {
                        List<StageResult> stageResult = GetTouchedStages(item);
                        for (int i = 0; i < stageResult.Count; i++)
                        {
                            itemInfo.trackingDetails.Add(new TrackingDetails()
                            {
                                trackingNo = trackingNo,
                                location = item.CountryCode,
                                description = stageResult[i].Description,
                                dateTime = stageResult[i].DateStage.ToString("dd/MM/yyyy HH:mm:ss")
                            });
                        }
                    }
                }
            }
            return itemInfo;
        }
        private static List<StageResult> GetTouchedStages(Item item)
        {
            // Create a list to hold the result for each touched stage
            List<StageResult> touchedStages = [];

            // Evaluate each stage and add to the list if touched
            AddIfTouched(touchedStages, item.DateStage1, item.Stage1StatusDesc);
            AddIfTouched(touchedStages, item.DateStage2, item.Stage2StatusDesc);
            AddIfTouched(touchedStages, item.DateStage3, item.Stage3StatusDesc);
            AddIfTouched(touchedStages, item.DateStage4, item.Stage4StatusDesc);
            AddIfTouched(touchedStages, item.DateStage5, item.Stage5StatusDesc);
            AddIfTouched(touchedStages, item.DateStage6, item.Stage6StatusDesc);
            AddIfTouched(touchedStages, item.DateStage7, item.Stage7StatusDesc);
            AddIfTouched(touchedStages, item.DateStage8, item.Stage8StatusDesc);
            AddIfTouched(touchedStages, item.DateStage9, item.Stage9StatusDesc);

            return touchedStages;
        }
        private static void AddIfTouched(List<StageResult> touchedStages, DateTime? dateStage, string stageDesc)
        {
            // Check if DateStage is valid and StageDesc is not empty
            if (dateStage.HasValue && dateStage.Value != DateTime.MinValue && !string.IsNullOrWhiteSpace(stageDesc))
            {
                touchedStages.Add(new StageResult((DateTime)dateStage, stageDesc));
            }
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
        private static DataTable ConcatenateDataTables(params DataTable[] tables)
        {
            if (tables == null || tables.Length == 0)
                throw new ArgumentException("At least one DataTable must be provided", nameof(tables));

            // Clone the schema of the first table (assuming all tables have the same schema)
            DataTable mergedTable = tables[0].Clone();

            // Import rows from each table into the merged table
            foreach (DataTable table in tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    mergedTable.ImportRow(row);
                }
            }

            return mergedTable;
        }
        private async Task<bool> IsTrackingNoOwner(string customerCode, string trackingNo, string productCode)
        {
            //---- Checks if Item Tracking Id is already used, retrieves from used table. ----//
            var itemtrackingid = await _itemTrackingRepository.FirstOrDefaultAsync(x =>
                                                                                        x.TrackingNo.Equals(trackingNo) &&
                                                                                        x.ProductCode.Equals(productCode) &&
                                                                                        x.CustomerCode.Equals(customerCode)
                                                                                  );


            //---- If exists then means already used, and returns false. ----//
            if (itemtrackingid is not null) return false;

            //---- Checks if Item Tracking Id exists in the System ----//
            var trackingIds = await GetItemTrackingFile(customerCode, trackingNo);

            //---- If returns null, means cannot find tracking no in the system, so returns false. ----//
            if (trackingIds is null) return false;

            //---- Found Item Tracking Id generated by system and unused. ----//
            return true;

        }
        private async Task<ItemIds> GetItemTrackingFile(string customerCode, string trackingNo = "", string postalCode = null, string productCode = null)
        {
            List<ItemTrackingReview> reviews = [];

            //---- Gets the reviews using Tracking No. Prefix, Suffix and CustomerCode ----//
            if (!string.IsNullOrWhiteSpace(trackingNo))
            {
                string prefix = trackingNo[..2];
                string suffix = trackingNo[^2..];

                reviews = await _itemTrackingReviewRepository.GetAllListAsync(x =>
                                                                                    x.Prefix.Equals(prefix) &&
                                                                                    x.Suffix.Equals(suffix) &&
                                                                                    x.CustomerCode.Equals(customerCode)
                                                                                 );
            }
            //---- Gets all reviews tied to Customer Code using optional parameters of Postal or Product Code ----//
            else
            {
                reviews = await _itemTrackingReviewRepository.GetAllListAsync(x => x.CustomerCode.Equals(customerCode));
                reviews = [.. reviews.WhereIf(!string.IsNullOrWhiteSpace(postalCode), x => x.PostalCode.Equals(postalCode))];
                reviews = [.. reviews.WhereIf(!string.IsNullOrWhiteSpace(productCode), x => x.ProductCode.Equals(productCode))];
            }

            //---- If reviews does not exist, means either Tracking No. is not in system ----//
            //---- or there is no records for reviews with the provided Postal or Product Code ----//
            if (reviews.Count == 0) return null;

            List<string> paths = [];
            List<ItemTrackingWithPath> ItemWithPath = [];

            string itemIdFilePath = "";

            //---- First gets all applications ----//
            var applications = await _itemTrackingApplicationRepository.GetAllListAsync();

            //---- Gets all excel paths with the reviews ----//
            foreach (var review in reviews)
            {
                var application = applications.FirstOrDefault(x => x.Id.Equals(review.ApplicationId));

                if (application is not null && !application.Path.IsNullOrWhiteSpace()) paths.Add(application.Path);
            }

            if (!paths.Count.Equals(0))
            {
                //---- Gets all Excel files and retrieves its info to create the object ItemIds ----//
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

            return new ItemIds()
            {
                ItemWithPath = ItemWithPath,
                Path = itemIdFilePath
            };
        }
        private async Task<UnusedItemIds> GetUnusedTrackingId(string trackingNo = "", string postalCode = null, string productCode = null, string customerCode = null)
        {
            var ItemIds = await GetItemTrackingFile(customerCode ?? "Any Account", trackingNo, postalCode, productCode);

            if (ItemIds is null) return null;

            var usedTrackingIdList = await _itemTrackingRepository.GetAllListAsync();

            usedTrackingIdList = usedTrackingIdList.Where(x =>
                                        ItemIds.ItemWithPath.Any(y =>
                                                y.ItemIds.Any(z =>
                                                        z.TrackingNo.Equals(x.TrackingNo, StringComparison.Ordinal)
                                                )
                                        )
                                ).ToList();

            List<ItemTrackingWithPath> itemWithPath = [];
            List<string> unusedList = [];

            foreach (var itemIdWithPath in ItemIds.ItemWithPath)
            {
                List<ItemTrackingIdDto> tempItemIds = [];
                foreach (var item in itemIdWithPath.ItemIds)
                {
                    bool contains = usedTrackingIdList.Any(x => x.TrackingNo.Equals(item.TrackingNo));
                    if (!contains)
                    {
                        tempItemIds.Add(item);
                        unusedList.Add(item.TrackingNo);
                    }
                }
                itemIdWithPath.ItemIds = tempItemIds;
                itemWithPath.Add(itemIdWithPath);
            }

            return new UnusedItemIds()
            {
                ItemWithPath = itemWithPath,
                UnusedList = unusedList,
            };
        }
        private async Task<string> GetNextAvailableTrackingNumber(string postalCode, bool? willUpdate = false, string productCode = null, string customerCode = null)
        {
            string result = null;

            var allAccountLists = await GetUnusedTrackingId("", postalCode, productCode, customerCode ?? null);

            if (allAccountLists is null) return result;

            var randomIndex = 0;
            var count = allAccountLists.UnusedList.Count;

            if (count > 1)
            {
                var maxRan = count - 1;
                randomIndex = new Random().Next(0, maxRan);
            }

            var randomTrackingNo = allAccountLists.UnusedList
                .OrderBy(u => u)
                .Skip(randomIndex)
                .Take(1)
                .FirstOrDefault();

            var itemPath = allAccountLists.ItemWithPath.FirstOrDefault(x => x.ItemIds.Any(y => y.TrackingNo.Equals(randomTrackingNo)));

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
                        DateUsed = DateTime.Now,
                        ProductCode = itemPath.ProductCode,
                    }).ConfigureAwait(false);
                }
            }

            return result;
        }
        private async Task<decimal> GetItemTopupValueFromPostalMaintenance(string postalCode, string serviceCode, string productCode)
        {
            var postal = await _postalRepository.FirstOrDefaultAsync(x =>
                                                                        x.PostalCode.Equals(postalCode) &&
                                                                        x.ServiceCode.Equals(serviceCode) &&
                                                                        x.ProductCode.Equals(productCode)
                                                                    );

            return postal is not null ? postal.ItemTopUpValue : 0m;
        }
        private async Task InsertUpdateTrackingNumber(string trackingNo, string customerCode, long customerId, string productCode, Dispatch dispatch, bool isAnyAccount = true, bool isSelfGenerated = true)
        {
            var item = await _itemTrackingRepository.FirstOrDefaultAsync(x => x.TrackingNo.Equals(trackingNo));

            if (item is not null)
            {
                item.CustomerCode = customerCode;
                item.CustomerId = customerId;
                item.ProductCode = productCode;
                item.DispatchId = dispatch.Id;
                item.DispatchNo = dispatch.DispatchNo;

                await _itemTrackingRepository.UpdateAsync(item).ConfigureAwait(false);
            }
            else
            {
                if (isSelfGenerated)
                {
                    var itemIdDetails = await GetItemTrackingFile(isAnyAccount ? "Any Account" : customerCode, trackingNo);

                    if (itemIdDetails is not null)
                    {
                        var matched = itemIdDetails.ItemWithPath.FirstOrDefault(x => x.ExcelPath.Equals(itemIdDetails.Path));

                        await _itemTrackingRepository.InsertAsync(new ItemTracking()
                        {
                            TrackingNo = trackingNo,
                            ApplicationId = matched.ApplicationId,
                            ReviewId = matched.ReviewId,
                            CustomerId = customerId,
                            CustomerCode = customerCode,
                            DateCreated = matched.DateCreated,
                            DateUsed = DateTime.Now,
                            ProductCode = matched.ProductCode,
                            DispatchNo = dispatch.DispatchNo,
                            DispatchId = dispatch.Id
                        }).ConfigureAwait(false);
                    }
                }
                else
                {
                    await _itemTrackingRepository.InsertAsync(new ItemTracking()
                    {
                        TrackingNo = trackingNo,
                        ApplicationId = 0,
                        ReviewId = 0,
                        CustomerId = customerId,
                        CustomerCode = customerCode,
                        DateCreated = DateTime.Now,
                        DateUsed = DateTime.Now,
                        ProductCode = productCode,
                        DispatchNo = dispatch.DispatchNo,
                        DispatchId = dispatch.Id,
                        IsExternal = true
                    }).ConfigureAwait(false);
                }
            }
        }
        private static async Task AlertIfLowThreshold(string postalCode, int threshold, string productCode = null)
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
        private async Task<int> GetLastRunningNo(string prefix, string prefixNo, string suffix)
        {
            var itemIdRunningNos = await _itemIdRunningNoRepository.FirstOrDefaultAsync(x => x.Prefix.Equals(prefix) &&
                                                                                             x.PrefixNo.Equals(prefixNo) &&
                                                                                             x.Suffix.Equals(suffix));

            if (itemIdRunningNos is null)
            {
                await _itemIdRunningNoRepository.InsertAsync(new ItemIdRunningNo()
                {
                    Customer = "Generate",
                    Prefix = prefix,
                    PrefixNo = prefixNo,
                    Suffix = suffix,
                    RunningNo = 0
                }).ConfigureAwait(false);

                return 0;
            }
            return itemIdRunningNos.RunningNo;
        }
        private static int GenerateCheckDigit(string serialNo)
        {
            int[] multiplier = [8, 6, 4, 2, 3, 5, 9, 7];

            char[] charArr = serialNo.ToCharArray();
            int checkDigit;
            int sum = 0;

            for (int i = 0; i < charArr.Length; i++)
            {
                int x = int.Parse(charArr[i].ToString());
                int m = multiplier[i];

                sum += x * m;
            }

            int remainder = sum % 11;

            if (remainder == 0) checkDigit = 5;
            else if (remainder == 1) checkDigit = 0;
            else checkDigit = 11 - remainder;

            return checkDigit;
        }
        private async Task<bool> IsTrackingNumberExist(string trackingNo, string Prefix, string PrefixNo, string Suffix)
        {
            var itemTrackingReviews = await _itemTrackingReviewRepository.GetAllListAsync();

            var reviews = itemTrackingReviews
                .Where(x => x.Prefix == Prefix)
                .Where(x => x.PrefixNo == PrefixNo)
                .Where(x => x.Suffix == Suffix)
                .ToList();

            if (reviews.Count == 0) return false;

            foreach (var review in reviews)
            {
                var applications = await _itemTrackingApplicationRepository.GetAllListAsync(x => x.Id.Equals(review.ApplicationId));

                foreach (var application in applications)
                {
                    Stream excelFile = await GetFileStream(application.Path);

                    DataTable dataTable = ConvertToDatatable(excelFile);

                    if (dataTable.Rows.Count == 0) return false;

                    foreach (DataRow dr in dataTable.Rows)
                    {
                        if (dr.ItemArray[0].ToString() == trackingNo) return true;
                    }
                }

            }

            return false;
        }
        private async Task<List<string>> GenerateTrackingNumbers(int startNo, int totalRequested, string suffix, string prefixNo, string prefix)
        {
            List<string> result = [];

            int startingNo = startNo;
            int endingNo = startingNo + totalRequested;
            var maxSerialNo = 999999;

            if (endingNo > maxSerialNo)
            {
                int over = endingNo - maxSerialNo;
                var totalAvailable = maxSerialNo - startingNo + 1;
                throw new UserFriendlyException($"The amount requested is over by {over}. Only able to generate {totalAvailable} Tracking Ids.");
            }

            for (var i = 0; i < totalRequested; i++)
            {
                int padLeft = 6;
                if (prefixNo.ToString().Length == 3) padLeft = 5;
                if (prefixNo.ToString().Length == 4) padLeft = 4;

                string serialNo = prefixNo + Convert.ToString(startingNo + i).PadLeft(padLeft, '0');

                var canAdd = true;

                int checkDigit = GenerateCheckDigit(serialNo);

                string trackingNo = string.Format("{0}{1}{2}{3}", prefix, serialNo, checkDigit.ToString(), suffix);

                await _itemTrackingRepository.InsertAsync(new ItemTracking()
                {
                    TrackingNo = trackingNo,
                    ApplicationId = 0,
                    ReviewId = 0,
                    CustomerId = 0,
                    CustomerCode = "",
                    DateCreated = DateTime.Now,
                    DateUsed = DateTime.Now,
                    ProductCode = "",
                }).ConfigureAwait(false);

                #region Check Existence (For Single)
                var checkExistenceEnabled = false;
                if (checkExistenceEnabled)
                {
                    if (totalRequested == 1)
                    {
                        var existed = await IsTrackingNumberExist(trackingNo, prefix, prefixNo, suffix);
                        if (existed)
                        {
                            #region Another Attempt
                            serialNo = prefixNo + Convert.ToString(startNo + i + 1).PadLeft(padLeft, '0');
                            checkDigit = GenerateCheckDigit(serialNo);
                            trackingNo = string.Format("{0}{1}{2}{3}", prefix, serialNo, checkDigit.ToString(), suffix);

                            existed = await IsTrackingNumberExist(trackingNo, prefix, prefixNo, suffix);

                            if (existed) return result;
                            else canAdd = true;
                            #endregion
                        }
                        else canAdd = true;
                    }
                }
                #endregion
                if (canAdd) result.Add(trackingNo);
            }
            return result;
        }
        private async Task<string> GetSAToken()
        {
            var TokenGenerationUrl = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("SA_TokenGenerationUrl"));
            var DevEnvironment = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("SA_DevEnvironment"));
            var isDevEnvironment = (DevEnvironment.Value.Trim() == "false") ? false : (DevEnvironment.Value.Trim() == "true");
            var username = isDevEnvironment ? await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("SA_UserName_Dev")) : await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("SA_UserName_Prod"));
            var password = isDevEnvironment ? await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("SA_Password_Dev")) : await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("SA_Password_Prod"));

            var saTokenClient = new HttpClient();
            saTokenClient.DefaultRequestHeaders.Clear();

            var tokenRequest = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("UserName", username.Value.Trim()),
                new KeyValuePair<string, string>("Password", password.Value.Trim()),
                new KeyValuePair<string, string>("grant_type", "password")
            ]);

            APIRequestResponse apiRequestResponse = new()
            {
                URL = TokenGenerationUrl.Value.Trim(),
                RequestBody = JsonConvert.SerializeObject(tokenRequest),
                RequestDateTime = DateTime.Now
            };

            var saTokenRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(TokenGenerationUrl.Value.Trim()),
                Content = tokenRequest,
            };
            using var saTokenResponse = await saTokenClient.SendAsync(saTokenRequestMessage);
            saTokenResponse.EnsureSuccessStatusCode();
            var saTokenBody = await saTokenResponse.Content.ReadAsStringAsync();

            apiRequestResponse.ResponseBody = saTokenBody;
            apiRequestResponse.ResponseDateTime = DateTime.Now;
            apiRequestResponse.Duration = (apiRequestResponse.ResponseDateTime - apiRequestResponse.RequestDateTime).Seconds;

            await _apiRequestResponseRepository.InsertAsync(apiRequestResponse).ConfigureAwait(false);

            var saTokenResult = JsonConvert.DeserializeObject<SATokenResponse>(saTokenBody);

            if (saTokenResult != null)
            {
                var token_expiration = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("SA_TokenExpiration"));
                token_expiration.Value = saTokenResult.expires;

                var token = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("SA_Token"));
                token.Value = saTokenResult.token;

                await _applicationSettingRepository.UpdateAsync(token_expiration);
                await _applicationSettingRepository.UpdateAsync(token);
                await _applicationSettingRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                return saTokenResult.token;
            }

            return "";
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

        [HttpPost]
        [Route("api/PreRegisterItem/DO")]
        public async Task<OutPreRegisterItem> PreRegisterItemDO(InPreRegisterItem input)
        {
            return await PreRegisterItem(input, "DO");
        }

        [HttpPost]
        [Route("api/PreRegisterItem/MY")]
        public async Task<OutPreRegisterItem> PreRegisterItemMY(InPreRegisterItem input)
        {
            return await PreRegisterItem(input, "MY");
        }

        [HttpPost]
        [Route("api/PreRegisterItem/CO")]
        public async Task<OutPreRegisterItem> PreRegisterItemCO(InPreRegisterItem input)
        {
            return await PreRegisterItem(input, "CO");
        }

        [HttpGet]
        public async Task<OutPreRegisterItem> PreRegisterItem(InPreRegisterItem input, string postal)
        {
            const string SUCCESS = "success";
            const string FAILED = "failed";
            string auto = "auto";

            string customerCode = "";
            string clientSecret = "";

            int threshold = 50000;

            var result = new OutPreRegisterItem();

            var newResponseIDRaw = Guid.NewGuid().ToString();

            result.ResponseID = GenerateMD5Hash(newResponseIDRaw);
            result.RefNo = input.RefNo;
            result.Status = FAILED;
            result.Errors = [];
            result.APIItemID = "";
            //result.ItemID = input.ItemID;

            string postalSupported = postal[..2];

            string postalCode = postalSupported;

            if (input.PostalCode.Contains(postalSupported))
            {
                var cust = await _customerRepository.FirstOrDefaultAsync(u => u.ClientKey == input.ClientKey);

                if (cust is not null)
                {
                    customerCode = cust.Code;
                    clientSecret = cust.ClientSecret;

                    string signHashRequest = input.SignatureHash.Trim().ToUpper();
                    string signHashRaw = string.Format("{0}-{1}-{2}-{3}", input.ItemID, input.RefNo, input.ClientKey, clientSecret);
                    string signHashServer = GenerateMD5Hash(signHashRaw).Trim().ToUpper();

                    var signMatched = signHashRequest.Equals(signHashServer);
                    var isItemIDAutoMandatory = true;

                    if (signMatched)
                    {
                        #region Validation

                        #region Check Recipient Country For DE
                        // var willValidateCountry = false;
                        // if (willValidateCountry)
                        // {
                        //     if (input.RecipientCountry.ToUpper().Trim() == postalSupported)
                        //     {
                        //         result.Errors.Add($"Invalid country {input.RecipientCountry} for this service");
                        //     }
                        // }

                        if (string.Equals(input.ServiceCode.Trim(), "DE", StringComparison.OrdinalIgnoreCase))
                        {
                            var rateZone = await _rateZoneRepository.FirstOrDefaultAsync(x => x.Country.ToUpper().Trim().Equals(input.RecipientCountry.ToUpper().Trim()) &&
                                                                                              x.State.ToUpper().Trim().Equals(input.RecipientState.ToUpper().Trim()) &&
                                                                                              x.City.ToUpper().Trim().Equals(input.RecipientCity.ToUpper().Trim()));

                            if (rateZone is null)
                            {
                                result.Errors.Add($"Rate Zone Could not be found with the Country, State and City of {input.RecipientCountry}, {input.RecipientState}, {input.RecipientCity}.");
                            }
                        }
                        #endregion

                        #region Check Service Code
                        // if (postalSupported != "DO")
                        // {
                        //     if (!string.Equals(input.ServiceCode.Trim(), "TS", StringComparison.OrdinalIgnoreCase))
                        //     {
                        //         result.Errors.Add("Invalid service code");
                        //     }
                        // }

                        //Check for TS postals
                        if (string.Equals(input.ServiceCode.Trim(), "TS", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] TS_Postals = ["DO", "KG", "GQ", "SL", "GE"];
                            if (!TS_Postals.Contains(postalSupported))
                            {
                                result.Errors.Add("Invalid service code");
                            }
                        }

                        //Check for DE postals
                        if (string.Equals(input.ServiceCode.Trim(), "DE", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] DE_Postals = ["MY", "CO"];
                            if (!DE_Postals.Contains(postalSupported))
                            {
                                result.Errors.Add("Invalid service code");
                            }
                        }

                        #endregion

                        #region Check Product Code
                        // if (postalSupported == "KG")
                        // {

                        //     if (!string.Equals(input.ProductCode.Trim(), "OMT", StringComparison.OrdinalIgnoreCase) &&
                        //         !string.Equals(input.ProductCode.Trim(), "PRT", StringComparison.OrdinalIgnoreCase))
                        //     {
                        //         result.Errors.Add("Invalid product code");
                        //     }
                        // }
                        // else
                        // {
                        //     if (postalSupported != "DO")
                        //     {
                        //         if (!string.Equals(input.ProductCode.Trim(), "OMT", StringComparison.OrdinalIgnoreCase))
                        //         {
                        //             result.Errors.Add("Invalid product code");
                        //         }
                        //     }
                        // }

                        //PRT Product Code
                        if (string.Equals(input.ProductCode.Trim(), "PRT", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] Postals = ["KG", "MY", "CO"];
                            if (!Postals.Contains(postalSupported))
                            {
                                result.Errors.Add("Invalid product code");
                            }
                        }

                        //OMT Product Code
                        if (string.Equals(input.ProductCode.Trim(), "OMT", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] Postals = ["DO", "KG", "GQ", "SL", "GE"];
                            if (!Postals.Contains(postalSupported))
                            {
                                result.Errors.Add("Invalid product code");
                            }
                        }

                        //EMS Product Code
                        if (string.Equals(input.ProductCode.Trim(), "EMS", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] Postals = [];
                            if (!Postals.Contains(postalSupported))
                            {
                                result.Errors.Add("Invalid product code");
                            }
                        }

                        //R Product Code
                        if (string.Equals(input.ProductCode.Trim(), "R", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] Postals = [];
                            if (!Postals.Contains(postalSupported))
                            {
                                result.Errors.Add("Invalid product code");
                            }
                        }

                        #endregion

                        #region Check IOSS Tax
                        string[] IOSS_Postals = ["KG", "GQ"];
                        if (IOSS_Postals.Contains(postalSupported))
                        {
                            string[] IOSS_countryList = ["IE", "HR", "MT", "CZ"];
                            string recipientCountry = input.RecipientCountry.ToUpper().Trim();
                            if (IOSS_countryList.Contains(recipientCountry) && string.IsNullOrWhiteSpace(input.IOSSTax))
                            {
                                result.Errors.Add($"IOSSTax is mandatory for {input.RecipientCountry}");
                            }
                        }
                        #endregion

                        #region Check ItemID Empty
                        if ((string.IsNullOrWhiteSpace(input.ItemID) ? "" : input.ItemID.ToLower().Trim()) != auto.ToLower().Trim())
                        {
                            result.Errors.Add("Invalid ItemID value. ItemID must be set to 'auto'");
                        }
                        #endregion

                        #region Check Sufficient ItemID
                        // if (!postalSupported.Equals("GQ") && !postalSupported.Equals("DO"))
                        // {
                        //     var nextItemIdFromRange = await GetNextAvailableTrackingNumber(postalCode);
                        //     if (string.IsNullOrWhiteSpace(nextItemIdFromRange)) result.Errors.Add("Insufficient API Item ID. Please contact us for assistance");
                        // }
                        string[] required_Postals = ["KG", "SL", "GE"];
                        if (required_Postals.Contains(postalSupported))
                        {
                            var nextItemIdFromRange = await GetNextAvailableTrackingNumber(postalCode);
                            if (string.IsNullOrWhiteSpace(nextItemIdFromRange))
                            {
                                result.Errors.Add("Insufficient API Item ID. Please contact us for assistance");
                            }
                        }

                        #endregion

                        #region Check Valid PoolItemID
                        if (!string.IsNullOrWhiteSpace(input.PoolItemId))
                        {
                            var isOwned = await IsTrackingNoOwner(customerCode, input.PoolItemId, input.ProductCode);
                            if (!isOwned)
                            {
                                result.Errors.Add("Invalid Pool Item ID");
                            }
                        }
                        #endregion

                        #endregion

                        if (result.Errors.Count == 0) //All validations has passed
                        {
                            //---- Create a Temporary Dispatch to insert Items ----//
                            string dispNo = string.Format("TempDisp-{0}-{1}-{2}-{3}", customerCode, input.PostalCode, input.ServiceCode, input.ProductCode);

                            var dispatchTemp = await _dispatchRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispNo) &&
                                                                                                  x.CustomerCode.Equals(customerCode));

                            if (dispatchTemp == null)
                            {
                                dispatchTemp = await _dispatchRepository.InsertAsync(
                                    new Dispatch
                                    {
                                        DispatchNo = dispNo,
                                        DispatchDate = DateOnly.FromDateTime(DateTime.Now),
                                        CustomerCode = customerCode,
                                        PostalCode = input.PostalCode,
                                        ServiceCode = input.ServiceCode,
                                        ProductCode = input.ProductCode,
                                        BatchId = "",
                                        TransactionDateTime = DateTime.Now
                                    });

                                await _dispatchRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);
                            }

                            string newItemIdFromSMI = null;

                            if (!string.IsNullOrWhiteSpace(input.PoolItemId))
                            {
                                //---- Nothing to generate because the pool item ID already assigned to this Customer Code. ----//
                                newItemIdFromSMI = input.PoolItemId;
                                input.ItemID = newItemIdFromSMI;

                                try
                                {
                                    await InsertUpdateTrackingNumber(newItemIdFromSMI, customerCode, cust.Id, input.ProductCode, dispatchTemp, isAnyAccount: false);

                                    await AlertIfLowThreshold(postalCode, threshold);

                                    result.ItemID = newItemIdFromSMI;
                                    result.Status = SUCCESS;
                                }
                                catch (Exception ex)
                                {
                                    result.Status = FAILED;
                                    result.Errors.Add(ex.Message);
                                }
                            }
                            else
                            {
                                //DE ItemID Handling
                                if (string.Equals(input.ServiceCode.Trim(), "DE", StringComparison.OrdinalIgnoreCase))
                                {
                                    newItemIdFromSMI = input.RefNo;

                                    try
                                    {
                                        await InsertUpdateTrackingNumber(newItemIdFromSMI, customerCode, cust.Id, input.ProductCode, dispatchTemp, isSelfGenerated: false);

                                        //await AlertIfLowThreshold(postalCode, threshold); //DE no need to check threshold

                                        result.ItemID = newItemIdFromSMI;
                                        result.Status = SUCCESS;
                                    }
                                    catch (Exception ex)
                                    {
                                        result.Status = FAILED;
                                        result.Errors.Add(ex.Message);
                                    }
                                }
                                //TS ItemID Handling
                                else
                                {
                                    if (postalSupported == "DO") // Generate ItemID on Call
                                    {
                                        #region Picking Item ID
                                        List<string> useCustomerPoolAccNos = ["GTS"];
                                        bool useCustomerPool = useCustomerPoolAccNos.Contains(customerCode);

                                        if (input.PostalCode == "DO01")
                                        {
                                            #region New Item ID from SPS

                                            List<string> trackingNumbers = [];

                                            if ((string.IsNullOrWhiteSpace(input.ItemID) ? "" : input.ItemID).ToLower().Trim() == auto.ToLower().Trim())
                                            {
                                                newItemIdFromSMI = await GetNextAvailableTrackingNumber(postalCode, false, input.ProductCode, useCustomerPool ? customerCode : null);

                                                if (!string.IsNullOrWhiteSpace(newItemIdFromSMI))
                                                {
                                                    try
                                                    {
                                                        await InsertUpdateTrackingNumber(newItemIdFromSMI, customerCode, cust.Id, input.ProductCode, dispatchTemp);

                                                        await AlertIfLowThreshold(postalCode, threshold);

                                                        result.ItemID = newItemIdFromSMI;
                                                        result.Status = SUCCESS;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        result.Status = FAILED;
                                                        result.Errors.Add(ex.Message);
                                                    }
                                                }
                                                else
                                                {
                                                    result.Status = FAILED;
                                                    result.Errors.Add("Insufficient Pool Item ID");
                                                }
                                                input.ItemID = newItemIdFromSMI;
                                            }
                                            else
                                            {
                                                if (isItemIDAutoMandatory)
                                                {
                                                    newItemIdFromSMI = await GetNextAvailableTrackingNumber(postalCode, false, input.ProductCode, useCustomerPool ? customerCode : null);

                                                    if (!string.IsNullOrWhiteSpace(newItemIdFromSMI))
                                                    {
                                                        try
                                                        {
                                                            await InsertUpdateTrackingNumber(newItemIdFromSMI, customerCode, cust.Id, input.ProductCode, dispatchTemp);

                                                            await AlertIfLowThreshold(postalCode, threshold);

                                                            result.ItemID = newItemIdFromSMI;
                                                            result.Status = SUCCESS;
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            result.Status = FAILED;
                                                            result.Errors.Add(ex.Message);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        result.Status = FAILED;
                                                        result.Errors.Add("Insufficient Pool Item ID");
                                                    }
                                                    input.ItemID = newItemIdFromSMI;
                                                }
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region SMIQ
                                            // SMIQueue spsq = new($"SMI-PreReg-{postalSupported}-{customerCode}", true);
                                            // spsq.SendMessage(input.RefNo, input.RefNo, input.PostalCode, input.ServiceCode, input.ProductCode, customerCode);

                                            // var kkk = 0;
                                            // var maxQAttempt = 20;
                                            // var qAttempt = 0;
                                            // var canProceedNext = false;
                                            // do
                                            // {
                                            //     kkk++;
                                            //     var spsqMsg = spsq.PeekFirstMessage();
                                            //     if (spsqMsg != null)
                                            //     {
                                            //         if (spsqMsg.Key == input.RefNo &&
                                            //             spsqMsg.AccNo == customerCode &&
                                            //             spsqMsg.PostalCode == input.PostalCode &&
                                            //             spsqMsg.ServiceCode == input.ServiceCode &&
                                            //             spsqMsg.ProductCode == input.ProductCode
                                            //            ) canProceedNext = true;
                                            //         else
                                            //         {
                                            //             if (qAttempt < maxQAttempt) System.Threading.Thread.Sleep(250);
                                            //             else canProceedNext = true;
                                            //         }
                                            //     }
                                            //     else
                                            //     {
                                            //         if (qAttempt < maxQAttempt) System.Threading.Thread.Sleep(250);
                                            //         else canProceedNext = true;
                                            //     }

                                            //     qAttempt++;
                                            // }
                                            // while (canProceedNext == false);
                                            #endregion

                                            #region New Item ID from SPS
                                            //string newItemIdFromSMI = null;

                                            List<string> trackingNumbers = [];

                                            string prefixNo = "00";
                                            string prefix = "UE";
                                            string suffix = "DO";

                                            if (input.ProductCode == "R") prefix = "RC";
                                            else if (input.ProductCode == "PRT") prefix = "LB";

                                            int lastRunningNo = await GetLastRunningNo(prefix, prefixNo, suffix);

                                            #region Item ID Last No
                                            // int itemIDlastNo = await GetItemIDLastNo(postalSupported);

                                            // if (lastRunningNo < itemIDlastNo) lastRunningNo = itemIDlastNo;
                                            #endregion

                                            int startNo = lastRunningNo == 0 ? 1 : (lastRunningNo + 1);

                                            if ((string.IsNullOrWhiteSpace(input.ItemID) ? "" : input.ItemID).ToLower().Trim() == auto.ToLower().Trim())
                                            {
                                                trackingNumbers = await GenerateTrackingNumbers(startNo, 1, suffix, prefixNo, prefix);

                                                newItemIdFromSMI = trackingNumbers.FirstOrDefault();

                                                if (!string.IsNullOrWhiteSpace(newItemIdFromSMI))
                                                {
                                                    try
                                                    {
                                                        var itemIdDetails = await _itemTrackingRepository.FirstOrDefaultAsync(x => x.TrackingNo.Equals(newItemIdFromSMI));

                                                        if (itemIdDetails is not null)
                                                        {
                                                            await _itemTrackingRepository.InsertAsync(new ItemTracking()
                                                            {
                                                                TrackingNo = newItemIdFromSMI,
                                                                ApplicationId = itemIdDetails.ApplicationId,
                                                                ReviewId = itemIdDetails.ReviewId,
                                                                CustomerId = cust.Id,
                                                                CustomerCode = customerCode,
                                                                DateCreated = itemIdDetails.DateCreated,
                                                                DateUsed = DateTime.Now,
                                                                ProductCode = itemIdDetails.ProductCode,
                                                                DispatchNo = ""
                                                            }).ConfigureAwait(false);
                                                        }

                                                        var itemIdRunningNos = await _itemIdRunningNoRepository.FirstOrDefaultAsync(x =>
                                                                                                x.Prefix.Equals(prefix) &&
                                                                                                x.PrefixNo.Equals(prefixNo) &&
                                                                                                x.Suffix.Equals(suffix)
                                                                                            );
                                                        if (itemIdRunningNos is not null)
                                                        {
                                                            itemIdRunningNos.RunningNo = startNo;
                                                            await _itemIdRunningNoRepository.UpdateAsync(itemIdRunningNos).ConfigureAwait(false);
                                                        }

                                                        result.ItemID = newItemIdFromSMI;
                                                        result.Status = SUCCESS;

                                                        // spsq.ReceiveMessage();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        result.Status = FAILED;
                                                        result.Errors.Add(ex.Message);
                                                    }
                                                }

                                                input.ItemID = newItemIdFromSMI;
                                            }
                                            #endregion
                                        }
                                        #endregion
                                    }
                                    else // Retrieve Generated Item ID from System
                                    {
                                        #region New Item ID from SPS
                                        if ((string.IsNullOrWhiteSpace(input.ItemID) ? "" : input.ItemID).ToLower().Trim() == auto.ToLower().Trim())
                                        {
                                            newItemIdFromSMI = await GetNextAvailableTrackingNumber(postalCode, false, postalSupported == "KG" ? input.ProductCode : null, null);

                                            if (string.IsNullOrWhiteSpace(newItemIdFromSMI)) result.Errors.Add("Insufficient Pool Item ID");
                                            else
                                            {
                                                try
                                                {
                                                    await InsertUpdateTrackingNumber(newItemIdFromSMI, customerCode, cust.Id, input.ProductCode, dispatchTemp);

                                                    await AlertIfLowThreshold(postalCode, threshold);

                                                    result.ItemID = newItemIdFromSMI;
                                                    result.Status = SUCCESS;
                                                }
                                                catch (Exception ex)
                                                {
                                                    result.Status = FAILED;
                                                    result.Errors.Add(ex.Message);
                                                }
                                                input.ItemID = newItemIdFromSMI;
                                            }
                                        }
                                        else
                                        {
                                            if (isItemIDAutoMandatory)
                                            {
                                                newItemIdFromSMI = await GetNextAvailableTrackingNumber(postalCode, false, input.ProductCode, null);

                                                if (string.IsNullOrWhiteSpace(newItemIdFromSMI)) result.Errors.Add("Insufficient Pool Item ID");
                                                else
                                                {
                                                    try
                                                    {
                                                        await InsertUpdateTrackingNumber(newItemIdFromSMI, customerCode, cust.Id, input.ProductCode, dispatchTemp);

                                                        await AlertIfLowThreshold(postalCode, threshold);

                                                        result.ItemID = newItemIdFromSMI;
                                                        result.Status = SUCCESS;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        result.Status = FAILED;
                                                        result.Errors.Add(ex.Message);
                                                    }
                                                    input.ItemID = newItemIdFromSMI;
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }

                            if (newItemIdFromSMI != null)
                            {
                                var newItem = await _itemRepository.FirstOrDefaultAsync(x => x.DispatchID.Equals(dispatchTemp.Id) &&
                                                                                             x.Id.Equals(newItemIdFromSMI));

                                if (newItem is null)
                                {
                                    newItem = await _itemRepository.InsertAsync(new Item
                                    {
                                        Id = newItemIdFromSMI,
                                        DispatchID = dispatchTemp.Id,
                                        BagID = null,
                                        DispatchDate = dispatchTemp.DispatchDate,
                                        Month = 0,
                                        PostalCode = input.PostalCode,
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
                                    newItem.DispatchID = dispatchTemp.Id;
                                    newItem.DispatchDate = dispatchTemp.DispatchDate;
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

                                    newItem = await _itemRepository.UpdateAsync(newItem);
                                }

                                string apiItemId = newItem.Id;

                                if (!string.IsNullOrWhiteSpace(apiItemId))
                                {
                                    result.APIItemID = apiItemId;
                                    result.Status = SUCCESS;
                                    result.Errors.Clear();
                                }
                                else
                                {
                                    apiItemId = newItem.Id;
                                    result.APIItemID = apiItemId;
                                    result.Status = SUCCESS;
                                    result.Errors.Clear();
                                }
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
            else result.Errors.Add("Postal not supported");

            string signHashRespRaw = string.Format("{0}-{1}-{2}-{3}-{4}", result.ResponseID, result.RefNo, result.APIItemID, input.ClientKey, clientSecret);
            string signHashRespServer = GenerateMD5Hash(signHashRespRaw);

            result.SignatureHash = signHashRespServer;

            #region Pool Item ID - Don't return API Item ID output

            if (!string.IsNullOrWhiteSpace(input.PoolItemId)) result.APIItemID = null;

            #endregion

            return result;
        }


        [HttpPost]
        [Route("api/PreRegisterItem/SA")]
        public async Task<OutPreRegisterItem> PreRegisterItemSA(InPreRegisterItem input)
        {
            const string SUCCESS = "success";
            const string FAILED = "failed";
            string auto = "auto";

            string customerCode = "";
            string clientSecret = "";
            string postalSupported = input.PostalCode[..2];

            var result = new OutPreRegisterItem();
            var newResponseIDRaw = Guid.NewGuid().ToString();

            result.ResponseID = GenerateMD5Hash(newResponseIDRaw);
            result.RefNo = input.RefNo;
            result.ItemID = input.ItemID;
            result.Status = FAILED;
            result.Errors = [];
            result.APIItemID = "";

            var cust = await _customerRepository.FirstOrDefaultAsync(u => u.ClientKey == input.ClientKey);

            if (cust is not null)
            {
                customerCode = cust.Code;
                clientSecret = cust.ClientSecret;

                string signHashRequest = input.SignatureHash.Trim().ToUpper();
                string signHashRaw = string.Format("{0}-{1}-{2}-{3}", input.ItemID, input.RefNo, input.ClientKey, clientSecret);
                string signHashServer = GenerateMD5Hash(signHashRaw).Trim().ToUpper();

                var signMatched = signHashRequest.Equals(signHashServer);

                if (signMatched)
                {
                    var DevEnvironment = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("SA_DevEnvironment"));
                    var isDevEnvironment = (DevEnvironment.Value.Trim() == "false") ? false : (DevEnvironment.Value.Trim() == "true");
                    var ParcelGenerationUrl = isDevEnvironment ? await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("SA_ParcelGenerationUrl_Dev")) : await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("SA_ParcelGenerationUrl_Prod"));
                    var countryListIOSS = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("SA_EU_CountryList"));
                    var token_expiration = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("SA_TokenExpiration"));
                    var token = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("SA_Token"));

                    #region SA02
                    if (input.PostalCode == "SA02")
                    {
                        if (input.ServiceCode != "DE" && input.ProductCode != "PRT")
                        {
                            result.Errors.Add("SA02 is only applicable for Service Code DE and Product Code PRT");
                        }

                        if (input.ItemValue == 0)
                        {
                            result.Errors.Add("Please specify the Item Amount");
                        }

                        result.Remarks = "Please take note, this is a Cash On Delivery(COD) service.";
                    }
                    #endregion

                    #region Service Code
                    if (input.ServiceCode.ToUpper().Trim() != "DE")
                    {
                        result.Errors.Add($"Invalid Service Code {input.ServiceCode.ToUpper().Trim()}. Must be Service Code DE.");
                    }
                    #endregion

                    #region Recipient Country
                    if (!string.Equals(input.RecipientCountry.Trim(), postalSupported, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Errors.Add($"Invalid Recipient Country {input.RecipientCountry.ToUpper().Trim()}");
                    }
                    #endregion

                    #region Validate Final Office
                    SAParam po = await _saParamRepository.FirstOrDefaultAsync(x => x.Type.Equals("SAFinalOffices") &&
                                                                               x.FinalOfficeId.Equals(input.PostOfficeName.ToUpper().Trim()));


                    string cityId = po != null ? po.CityId : "3";
                    string postOfficeId = po != null ? po.FinalOfficeId : "20300";

                    #endregion

                    #region 4 Digit Address No
                    var is4DigitAddressNoEnabled = false;
                    if (is4DigitAddressNoEnabled)
                    {
                        bool isValid = Regex.IsMatch(input.RecipientAddress, @"\d{4}");

                        if (!isValid)
                        {
                            result.Errors.Add("The recipient address does not contain a minimum 4-digit address number");
                        }
                    }
                    #endregion

                    #region Validate Item Value (Effective 1 Nov 2021)
                    var willValidateItemValue = false;
                    if (willValidateItemValue)
                    {
                        if (DateTime.Now >= new DateTime(2021, 11, 1))
                        {
                            var maxItemValue = 180m;
                            if (input.ItemValue > maxItemValue)
                            {
                                result.Errors.Add("Item value exceeded SAR180");
                            }
                        }
                    }
                    #endregion

                    #region Content Filtering
                    var illegalItemDescKeywords = new List<string> { "ADULT TOY", "drone", "flashlight", "booster", "boosting", "signal" };

                    foreach (var keyword in illegalItemDescKeywords)
                    {
                        if (input.ItemDesc.ToUpper().Trim().Contains(keyword))
                        {
                            result.Errors.Add("Contains prohibited item");
                        }
                    }

                    if (input.ItemDesc.ToUpper().Trim().Contains("SEX") && !input.ItemDesc.ToUpper().Trim().Contains("SEXY"))
                    {
                        result.Errors.Add("Contains prohibited item");
                    }
                    #endregion

                    #region Phone No
                    var willValidatePhoneNo = true;
                    if (willValidatePhoneNo)
                    {
                        var parseStatus = int.TryParse(input.RecipientContactNo, out int tel);
                        if (parseStatus && tel == 0)
                        {
                            result.Errors.Add("Invalid recipient contact number");
                        }
                    }
                    #endregion

                    var isItemIDAutoMandatory = false;
                    if (isItemIDAutoMandatory)
                    {
                        if ((string.IsNullOrWhiteSpace(input.ItemID) ? "" : input.ItemID.ToLower().Trim()) != auto.ToLower().Trim())
                        {
                            result.Errors.Add("Invalid ItemID value. ItemID must be set to 'auto'");
                        }
                    }

                    //Check IOSS EG : XX1234567890 - 2 Aplhabet + 10 digits of number, Length must equal 12  
                    #region IOSS Tax
                    var willValidateIOSS = false;
                    if (willValidateIOSS)
                    {
                        string[] countryCodes = countryListIOSS.Value.Split(',');

                        if (countryCodes.Contains(input.RecipientCountry.ToUpper().Trim()))
                        {
                            if (string.IsNullOrWhiteSpace(input.IOSSTax))
                            {
                                result.Errors.Add($"IOSSTax is mandatory for {input.RecipientCountry}");
                            }
                            else
                            {
                                // Regular expression to match the pattern: two letters followed by 1-10 digits
                                string iossTax = input.IOSSTax;


                                bool isValid = Regex.IsMatch(iossTax, @"^[A-Za-z]{2}\d{10}$");

                                if (!isValid)
                                {
                                    result.Errors.Add($"The IOSS format is incorrect. The IOSS identifier should follow the format IM1234567890.");
                                }
                            }

                        }
                    }
                    #endregion

                    string saToken = token.Value.Trim() == "" ? await GetSAToken() : token.Value.Trim();

                    if (token.Value.Trim() != "")
                    {
                        var dateString = token_expiration.Value.Replace(" UTC", "");
                        var token_expiration_date = DateTime.Parse(dateString);
                        if (token_expiration_date < DateTime.Now) saToken = await GetSAToken();
                    }

                    var httpstatus = HttpStatusCode.Unauthorized;

                    if (ParcelGenerationUrl != null)
                    {
                        if (result.Errors.Count == 0)
                        {
                            //---- Create a Temporary Dispatch to insert Items ----//
                            string dispNo = string.Format("TempDisp-{0}-{1}-{2}-{3}", customerCode, input.PostalCode, input.ServiceCode, input.ProductCode);

                            var dispatchTemp = await _dispatchRepository.FirstOrDefaultAsync(x =>
                                                                                                x.DispatchNo.Equals(dispNo) &&
                                                                                                x.CustomerCode.Equals(customerCode)
                                                                                            );
                            if (dispatchTemp == null)
                            {
                                dispatchTemp = await _dispatchRepository.InsertAsync(new Dispatch
                                {
                                    DispatchNo = dispNo,
                                    CustomerCode = customerCode,
                                    PostalCode = input.PostalCode,
                                    ServiceCode = input.ServiceCode,
                                    ProductCode = input.ProductCode,
                                    DispatchDate = DateOnly.FromDateTime(DateTime.Now),
                                    BatchId = "",
                                    TransactionDateTime = DateTime.Now
                                });

                                await _dispatchRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);
                            }

                            string newItemIdFromSPS = null;

                            do
                            {
                                var saClient = new HttpClient();
                                saClient.DefaultRequestHeaders.Clear();
                                saClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", saToken);

                                var addrMaxLen = 100;
                                string addressLine1 = input.RecipientAddress.Length > addrMaxLen ? input.RecipientAddress.Substring(0, addrMaxLen) : input.RecipientAddress;
                                string addressLine2 = input.RecipientPostcode;

                                addressLine2 = input.RecipientAddress.Length > addrMaxLen ? input.RecipientAddress.Substring(addrMaxLen, input.RecipientAddress.Length - addrMaxLen) : addressLine2;

                                string tel = input.RecipientContactNo;
                                tel = tel.Replace("+", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "");
                                tel = tel.PadLeft(10, '0');

                                int telMaxLength = 15;
                                if (tel.Length > telMaxLength)
                                {
                                    tel = tel.Substring(tel.Length - telMaxLength);
                                }


                                InAddressDetail senderAddressDetail = new()
                                {
                                    AddressTypeID = 6,
                                    LocationId = "1",
                                    FinalOfficeID = "20300",
                                    AddressLine1 = "King Khalid International Airport",
                                    AddressLine2 = "Riyadh"
                                };

                                InAddressDetail receiverAddressDetail = new()
                                {
                                    AddressTypeID = 6,
                                    FinalOfficeID = postOfficeId,
                                    LocationId = cityId,
                                    AddressLine1 = addressLine1,
                                    AddressLine2 = addressLine2
                                };

                                List<InPostItem> postItems = [];
                                postItems.Add(new InPostItem()
                                {
                                    ReferenceId = input.RefNo,
                                    ContentPrice = input.ItemValue,
                                    ContentDescription = input.ItemDesc,
                                    Weight = input.Weight,
                                    SenderAddressDetail = senderAddressDetail,
                                    ReceiverAddressDetail = receiverAddressDetail,
                                    TotalAmount = input.ItemValue,
                                    PaymentType = input.ItemValue > 0 ? 2 : 1
                                });

                                SARequest request = new()
                                {
                                    CRMAccountId = isDevEnvironment ? "521017871" : "31320397463",
                                    CustomerName = input.RecipientName.Length > 50 ? input.RecipientName.Substring(0, 50) : input.RecipientName,
                                    CustomerMobileNumber = tel,
                                    Items = postItems
                                };

                                APIRequestResponse apiRequestResponse = new()
                                {
                                    URL = ParcelGenerationUrl.Value.Trim(),
                                    RequestBody = JsonConvert.SerializeObject(request),
                                    RequestDateTime = DateTime.Now
                                };

                                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                                var saRequestMessage = new HttpRequestMessage
                                {
                                    Method = HttpMethod.Post,
                                    RequestUri = new Uri(ParcelGenerationUrl.Value.Trim()),
                                    Content = content,
                                };
                                using var apgResponse = await saClient.SendAsync(saRequestMessage);
                                httpstatus = apgResponse.StatusCode;

                                var saBody = await apgResponse.Content.ReadAsStringAsync();

                                apiRequestResponse.ResponseBody = saBody;
                                apiRequestResponse.ResponseDateTime = DateTime.Now;
                                apiRequestResponse.Duration = (apiRequestResponse.ResponseDateTime - apiRequestResponse.RequestDateTime).Seconds;

                                await _apiRequestResponseRepository.InsertAsync(apiRequestResponse);
                                await _apiRequestResponseRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                                if (httpstatus == HttpStatusCode.OK)
                                {
                                    var saResult = JsonConvert.DeserializeObject<SAResponse>(saBody);

                                    if (saResult != null)
                                    {
                                        if (saResult.Message == "Success")
                                        {
                                            newItemIdFromSPS = saResult.Items[0].Barcode;

                                            if (string.IsNullOrWhiteSpace(newItemIdFromSPS)) result.Errors.Add("Insufficient Pool Item ID");
                                            else
                                            {
                                                try
                                                {
                                                    await InsertUpdateTrackingNumber(newItemIdFromSPS, customerCode, cust.Id, input.ProductCode, dispatchTemp, isSelfGenerated: false);

                                                    result.ItemID = newItemIdFromSPS;
                                                    result.Status = SUCCESS;
                                                }
                                                catch (Exception ex)
                                                {
                                                    result.Status = FAILED;
                                                    result.Errors.Add(ex.Message);
                                                }
                                                input.ItemID = newItemIdFromSPS;
                                            }
                                        }
                                        else
                                        {
                                            result.APIItemID = saResult.Status;
                                            result.Status = FAILED;
                                            if (saResult.Items.Count != 0)
                                            {
                                                foreach (var item in saResult.Items)
                                                {
                                                    result.Errors.Add(item.Message);
                                                }
                                            }
                                            else
                                            {
                                                result.Errors.Add(saResult.Message);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        result.APIItemID = "";
                                        result.Status = FAILED;
                                        result.Errors.Add("Response was empty.");
                                    }

                                }
                                else
                                {
                                    if (httpstatus == HttpStatusCode.Unauthorized) saToken = await GetSAToken();
                                    else
                                    {
                                        result.APIItemID = "";
                                        result.Status = FAILED;
                                        result.Errors.Add(httpstatus.ToString());
                                    }
                                }
                            }
                            while (httpstatus == HttpStatusCode.Unauthorized);



                            if (result.Errors.Count == 0)
                            {
                                var newItem = await _itemRepository.FirstOrDefaultAsync(x =>
                                                                                        x.DispatchID.Equals(dispatchTemp.Id) &&
                                                                                        x.Id.Equals(input.ItemID)
                                                                                    );


                                if (newItem is null && newItemIdFromSPS is not null)
                                {
                                    newItem = await _itemRepository.InsertAsync(new Item
                                    {
                                        Id = newItemIdFromSPS,
                                        DispatchID = dispatchTemp.Id,
                                        BagID = null,
                                        DispatchDate = dispatchTemp.DispatchDate,
                                        Month = 0,
                                        PostalCode = input.PostalCode,
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
                                    newItem.DispatchID = dispatchTemp.Id;
                                    newItem.DispatchDate = dispatchTemp.DispatchDate;
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

                                    newItem = await _itemRepository.UpdateAsync(newItem);
                                }

                                string apiItemId = newItem.Id;

                                if (!string.IsNullOrWhiteSpace(apiItemId))
                                {
                                    result.APIItemID = apiItemId;

                                    result.Status = SUCCESS;
                                    result.Errors.Clear();
                                }
                                else
                                {
                                    var newId = newItem.Id;

                                    apiItemId = newItem.Id;

                                    result.APIItemID = apiItemId;

                                    result.Status = SUCCESS;
                                    result.Errors.Clear();
                                }
                            }
                        }
                    }
                    else
                    {
                        result.APIItemID = "";
                        result.Status = FAILED;
                        result.Errors.Add("Endpoint Not Found.");
                    }
                }
                else
                {
                    result.APIItemID = "";
                    result.Status = FAILED;
                    result.Errors.Add("Invalid SignatureHash.");
                }
            }

            string signHashRespRaw = string.Format("{0}-{1}-{2}-{3}-{4}", result.ResponseID, result.RefNo, result.APIItemID, input.ClientKey, clientSecret);
            string signHashRespServer = GenerateMD5Hash(signHashRespRaw);

            result.SignatureHash = signHashRespServer;

            return result;
        }

        [HttpGet]
        [Route("api/GetHashCode")]
        public async Task<string> GetHashCode(string itemId, string refNo, string clientKey)
        {
            string hash = "";
            var cust = await _customerRepository.FirstOrDefaultAsync(u => u.ClientKey == clientKey);

            if (cust is not null)
            {
                string signHashRaw = string.Format("{0}-{1}-{2}-{3}", itemId, refNo, clientKey, cust.ClientSecret);
                hash = GenerateMD5Hash(signHashRaw).Trim().ToUpper();
            }

            return hash;
        }
    }
}


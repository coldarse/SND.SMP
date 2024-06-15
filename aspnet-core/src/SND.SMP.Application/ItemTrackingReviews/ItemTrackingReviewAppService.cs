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

        // [HttpGet]
        // [Route("api/test")]
        // public async OutPreRegisterItem PreReg(InPreRegisterItem input, string postal)
        // {
        //     const string SUCCESS = "success";
        //     const string FAILED = "failed";

        //     string auto = "auto";

        //     string accNo = "";
        //     string clientSecret = "";

        //     int threshold = 50000;

        //     var result = new OutPreRegisterItem();

        //     var newResponseIDRaw = Guid.NewGuid().ToString();

        //     result.ResponseID = Assetrio.Core.Crypto.Encode(newResponseIDRaw);
        //     result.RefNo = input.RefNo;
        //     //result.ItemID = input.ItemID;
        //     result.Status = FAILED;
        //     result.Errors = [];
        //     result.APIItemID = "";

        //     string postalSupported = postal[..2];

        //     string postalId = postalSupported;

        //     if (input.PostalCode.Contains(postalSupported))
        //     {
        //         var cust = await _customerRepository.FirstOrDefaultAsync(u => u.ClientKey == input.ClientKey);

        //         if (cust != null)
        //         {
        //             accNo = cust.Code;
        //             clientSecret = cust.ClientSecret;

        //             string signHashRequest = input.SignatureHash.Trim().ToUpper();
        //             string signHashRaw = string.Format("{0}-{1}-{2}-{3}", input.ItemID, input.RefNo, input.ClientKey, clientSecret);
        //             string signHashServer = Assetrio.Core.Crypto.Encode(signHashRaw).Trim().ToUpper();

        //             var signMatched = signHashRequest == signHashServer;

        //             if (signMatched)
        //             {
        //                 #region Validation

        //                 #region Recipient Country
        //                 var willValidateCountry = false;
        //                 if (willValidateCountry)
        //                 {
        //                     if (input.RecipientCountry.ToUpper().Trim() == postalSupported)
        //                     {
        //                         result.Errors.Add($"Invalid country {input.RecipientCountry} for this service");
        //                     }
        //                 }
        //                 #endregion

        //                 #region Service Code
        //                 if (!string.Equals(input.ServiceCode.Trim(), "TS", StringComparison.OrdinalIgnoreCase))
        //                 {
        //                     result.Errors.Add("Invalid service code");
        //                 }
        //                 #endregion

        //                 if (postalSupported == "KG")
        //                 {
        //                     #region Product Code
        //                     if (!string.Equals(input.ProductCode.Trim(), "OMT", StringComparison.OrdinalIgnoreCase) && !string.Equals(input.ProductCode.Trim(), "PRT", StringComparison.OrdinalIgnoreCase))
        //                     {
        //                         result.Errors.Add("Invalid product code");
        //                     }
        //                     #endregion

        //                     #region IOSS Tax
        //                     var willValidateIOSS = true;
        //                     if (willValidateIOSS)
        //                     {
        //                         var countryListIOSS = new List<string> { "IE", "HU", "LU", "CZ" };

        //                         if (countryListIOSS.Contains(input.RecipientCountry.ToUpper().Trim()))
        //                         {
        //                             if (string.IsNullOrWhiteSpace(input.IOSSTax))
        //                             {
        //                                 result.Errors.Add($"IOSSTax is mandatory for {input.RecipientCountry}");
        //                             }
        //                         }
        //                     }
        //                     #endregion
        //                 }
        //                 else
        //                 {
        //                     #region Product Code
        //                     if (!string.Equals(input.ProductCode.Trim(), "OMT", StringComparison.OrdinalIgnoreCase))
        //                     {
        //                         result.Errors.Add("Invalid product code");
        //                     }
        //                     #endregion
        //                 }

        //                 var isItemIDAutoMandatory = true;
        //                 if (isItemIDAutoMandatory)
        //                 {
        //                     if ((string.IsNullOrWhiteSpace(input.ItemID) ? "" : input.ItemID.ToLower().Trim()) != auto.ToLower().Trim())
        //                     {
        //                         result.Errors.Add("Invalid ItemID value. ItemID must be set to 'auto'");
        //                     }
        //                 }

        //                 if (!postalSupported.Equals("GQ"))
        //                 {
        //                     var nextItemIdFromRange = await GetNextAvailableAnyAccountTrackingNumber(postalId);
        //                     if (string.IsNullOrWhiteSpace(nextItemIdFromRange))
        //                     {
        //                         result.Errors.Add("Insufficient API Item ID. Please contact us for assistance");
        //                     }
        //                 }

        //                 if (!string.IsNullOrWhiteSpace(input.PoolItemId))
        //                 {
        //                     var isOwned = IsTrackingNoOwner(accNo, input.PoolItemId, input.ProductCode);
        //                     if (!isOwned)
        //                     {
        //                         result.Errors.Add("Invalid Pool Item ID");
        //                     }
        //                 }

        //                 #endregion


        //                 string dispNo = string.Format("TempDisp-{0}-{1}-{2}-{3}", accNo, input.PostalCode, input.ServiceCode, input.ProductCode);

        //                 var dispatchTemp = await _dispatchRepository.FirstOrDefaultAsync(x =>
        //                                                                                     x.DispatchNo.Equals(dispNo) &&
        //                                                                                     x.CustomerCode.Equals(accNo)
        //                                                                                 );

        //                 if (dispatchTemp == null)
        //                 {
        //                     dispatchTemp = await _dispatchRepository.InsertAsync(new Dispatch
        //                     {
        //                         DispatchNo = dispNo,
        //                         CustomerCode = accNo,
        //                         PostalCode = input.PostalCode,
        //                         ServiceCode = input.ServiceCode,
        //                         ProductCode = input.ProductCode,
        //                         DispatchDate = null,
        //                         BatchId = "",
        //                         TransactionDateTime = DateTime.Now
        //                     });

        //                     await _dispatchRepository.GetDbContext().SaveChangesAsync();
        //                 }

        //                 string newItemIdFromSPS = null;

        //                 if (!string.IsNullOrWhiteSpace(input.PoolItemId))
        //                 {
        //                     newItemIdFromSPS = input.PoolItemId;
        //                     input.ItemID = newItemIdFromSPS;
        //                     //Nothing to generate because the pool item ID already assigned to this account no.
        //                 }
        //                 else
        //                 {
        //                     #region New Item ID from SPS
        //                     if ((string.IsNullOrWhiteSpace(input.ItemID) ? "" : input.ItemID).ToLower().Trim() == auto.ToLower().Trim())
        //                     {
        //                         newItemIdFromSPS = GetNextAvailableAnyAccountTrackingNumber(postalId, true, accNo, cust.Id, input.PostalCode, ProductCode = postalSupported == "KG" ? input.ProductCode : null);

        //                         if (!string.IsNullOrWhiteSpace(newItemIdFromSPS))
        //                         {
        //                             try
        //                             {
        //                                 UpdateTrackingNumber(newItemIdFromSPS, accNo, cust.Id, input.PostalCode);

        //                                 AlertIfLowThreshold(postalId, threshold);

        //                                 result.ItemID = newItemIdFromSPS;
        //                                 result.Status = SUCCESS;
        //                             }
        //                             catch (Exception ex)
        //                             {
        //                                 result.Status = FAILED;
        //                                 result.Errors.Add(ex.Message);
        //                             }
        //                         }

        //                         input.ItemID = newItemIdFromSPS;
        //                     }
        //                     else
        //                     {
        //                         if (isItemIDAutoMandatory)
        //                         {
        //                             newItemIdFromSPS = GetNextAvailableAnyAccountTrackingNumber(postalId, true, accNo, cust.Id, input.PostalCode);

        //                             if (!string.IsNullOrWhiteSpace(newItemIdFromSPS))
        //                             {
        //                                 try
        //                                 {
        //                                     UpdateTrackingNumber(newItemIdFromSPS, accNo, cust.Id, input.PostalCode);

        //                                     AlertIfLowThreshold(postalId, threshold);

        //                                     result.ItemID = newItemIdFromSPS;
        //                                     result.Status = SUCCESS;
        //                                 }
        //                                 catch (Exception ex)
        //                                 {
        //                                     result.Status = FAILED;
        //                                     result.Errors.Add(ex.Message);
        //                                 }
        //                             }

        //                             input.ItemID = newItemIdFromSPS;
        //                         }
        //                     }
        //                     #endregion
        //                 }

        //                 var newItem = await _itemRepository.FirstOrDefaultAsync(x =>
        //                                                                             x.DispatchID.Equals(dispatchTemp.Id) &&
        //                                                                             x.Id.Equals(input.ItemID)
        //                                                                        );

        //                 if (newItem == null)
        //                 {
        //                     newItem = await _itemRepository.InsertAsync(new Item
        //                     {
        //                         Id = newItemIdFromSPS,
        //                         DispatchID = dispatchTemp.Id,
        //                         BagID = 0,
        //                         DispatchDate = dispatchTemp.DispatchDate,
        //                         Month = 1,
        //                         PostalCode = input.ServiceCode,
        //                         ServiceCode = input.ServiceCode,
        //                         ProductCode = input.ProductCode,
        //                         CountryCode = input.RecipientCountry,
        //                         Weight = input.Weight,
        //                         BagNo = "",
        //                         SealNo = "",
        //                         Price


        //                         ExtRemark = dispatchTemp.DispatchNo,
        //                         BagNo = null,
        //                         SealNo = null,
        //                         BatchId = null,
        //                         DispatchDate = DateTime.Now.ToString("dd/MM/yyyy H:mm:ss"),

        //                         TrackingNo = input.ItemID,
        //                         BRItemID = string.IsNullOrWhiteSpace(newItemIdFromSPS) ? null : newItemIdFromSPS,

        //                         Postal = input.PostalCode,
        //                         Service = input.ServiceCode,
        //                         ProductCode = input.ProductCode,

        //                         Weight = input.Weight.ToString(),
        //                         WeightDec = input.Weight,
        //                         ItemValueDec = input.ItemValue,
        //                         ItemValue = input.ItemValue.ToString(),
        //                         ItemDesc = input.ItemDesc,

        //                         RecpName = input.RecipientName,
        //                         TelNo = input.RecipientContactNo,
        //                         Email = input.RecipientEmail,
        //                         Address = input.RecipientAddress,
        //                         City = input.RecipientCity,
        //                         State = input.RecipientState,
        //                         Postcode = input.RecipientPostcode,
        //                         Country = input.RecipientCountry,
        //                         RefNo = input.RefNo,
        //                         HSCode = input.HSCode,
        //                         SenderName = input.SenderName,
        //                         IOSSTax = input.IOSSTax,
        //                         TaxPayMethod = postalSupported == "GQ" ? input.IOSSTax : "",

        //                         AddressNo = input.AddressNo,
        //                         PassportNo = input.IdentityNo,
        //                         IdentityType = input.IdentityType
        //                     });

        //                     #region Item Topup Value
        //                     try
        //                     {
        //                         var itemTopupValue = GetItemTopupValueFromPostalMaintenance(input.PostalCode, input.ServiceCode, input.ProductCode);
        //                         var parseStatus = decimal.TryParse(newItem.ItemValue, out decimal currentItemValue);
        //                         if (parseStatus)
        //                         {
        //                             newItem.ItemValue = (currentItemValue + itemTopupValue).ToString();
        //                         }
        //                     }
        //                     catch (Exception exx)
        //                     {

        //                     }
        //                     #endregion

        //                     db.SaveChanges();
        //                 }
        //                 else
        //                 {
        //                     newItem.Weight = input.Weight.ToString(); newItem.WeightDec = input.Weight;
        //                     newItem.ItemValue = input.ItemValue.ToString();
        //                     newItem.ItemDesc = input.ItemDesc;

        //                     newItem.RecpName = input.RecipientName;
        //                     newItem.TelNo = input.RecipientContactNo;
        //                     newItem.Email = input.RecipientEmail;
        //                     newItem.Address = input.RecipientAddress;
        //                     newItem.City = input.RecipientCity;
        //                     newItem.Postcode = input.RecipientPostcode;
        //                     newItem.Country = input.RecipientCountry;
        //                     newItem.RefNo = input.RefNo;
        //                     newItem.HSCode = input.HSCode;
        //                     newItem.SenderName = input.SenderName;
        //                     newItem.IOSSTax = input.IOSSTax;

        //                     newItem.AddressNo = input.AddressNo;
        //                     newItem.PassportNo = input.IdentityNo;
        //                     newItem.IdentityType = input.IdentityType;

        //                     #region Item Topup Value
        //                     try
        //                     {
        //                         var itemTopupValue = GetItemTopupValueFromPostalMaintenance(input.PostalCode, input.ServiceCode, input.ProductCode);
        //                         var parseStatus = decimal.TryParse(newItem.ItemValue, out decimal currentItemValue);
        //                         if (parseStatus)
        //                         {
        //                             newItem.ItemValue = (currentItemValue + itemTopupValue).ToString();
        //                         }
        //                     }
        //                     catch (Exception exx)
        //                     {

        //                     }
        //                     #endregion


        //                 }

        //                 var item = await _itemRepository.FirstOrDefaultAsync(u => u.Id == newItem.Id);

        //                 if (item != null)
        //                 {
        //                     string apiItemId = item.BRItemID;

        //                     if (!string.IsNullOrWhiteSpace(apiItemId))
        //                     {
        //                         result.APIItemID = apiItemId;

        //                         result.Status = SUCCESS;
        //                         result.Errors.Clear();

        //                         //Log.Add($"PreReg-KG GetExisting for { input.RefNo } - { input.ItemID }", $"input [{ input.ItemID } - { input.ItemDesc } - { input.RecipientName } - { input.Weight }] get { result.APIItemID }");
        //                     }
        //                     else
        //                     {
        //                         var newId = newItem.Id;
        //                         apiItemId = "";

        //                         apiItemId = item.BRItemID;

        //                         result.APIItemID = apiItemId;

        //                         result.Status = SUCCESS;
        //                         result.Errors.Clear();

        //                         //Log.Add($"PreReg-KG Success for { input.RefNo } - { input.ItemID }", $"input [{ input.ItemID } - { input.ItemDesc } - { input.RecipientName } - { input.Weight }] get { result.APIItemID }");
        //                     }

        //                 }
        //                 else
        //                 {
        //                     result.Status = FAILED;
        //                     result.Errors.Add("Item is not found in the system");
        //                 }
        //             }
        //             else
        //             {
        //                 result.Status = FAILED;
        //                 result.Errors.Add("Signature hash is invalid");
        //             }
        //         }
        //         else
        //         {
        //             result.Status = FAILED;
        //             result.Errors.Add("Client key is invalid");
        //         }
        //     }
        //     else
        //     {
        //         result.Status = FAILED;
        //         result.Errors.Add("Postal not supported");
        //     }


        //     result.Status = FAILED;

        //     foreach (var modelState in ModelState.Values)
        //     {
        //         foreach (var error in modelState.Errors)
        //         {
        //             if (!string.IsNullOrWhiteSpace(error.ErrorMessage))
        //             {
        //                 result.Errors.Add(error.ErrorMessage);
        //             }
        //             if (error.Exception != null)
        //             {
        //                 if (!string.IsNullOrWhiteSpace(error.Exception.Message))
        //                 {
        //                     result.Errors.Add(error.Exception.Message);
        //                 }
        //             }
        //         }
        //     }

        //     string signHashRespRaw = string.Format("{0}-{1}-{2}-{3}-{4}", result.ResponseID, result.RefNo, result.APIItemID, input.ClientKey, clientSecret);
        //     string signHashRespServer = Assetrio.Core.Crypto.Encode(signHashRespRaw);

        //     result.SignatureHash = signHashRespServer;

        //     if (postalSupported != "SL")
        //     {
        //         #region Issued Item
        //         if (result.Status == SUCCESS)
        //         {
        //             try
        //             {
        //                 string respRaw = JsonConvert.SerializeObject(result);
        //                 string resp = respRaw.Length > 400 ? respRaw.Substring(0, 400) : respRaw;
        //                 APIItemIDHelper.AddToIssuedItem(input.RefNo, result.APIItemID, accNo, input.PostalCode, resp);
        //             }
        //             catch { }
        //         }
        //         #endregion
        //     }


        //     #region Pool Item ID - Don't return API Item ID output
        //     if (!string.IsNullOrWhiteSpace(input.PoolItemId))
        //     {
        //         result.APIItemID = null;
        //     }
        //     #endregion

        //     return result;
        // }

        // public static async Task<Stream> GetFileStream(string url)
        // {
        //     using var httpClient = new HttpClient();
        //     using var response = await httpClient.GetAsync(url);
        //     if (response.IsSuccessStatusCode)
        //     {
        //         var contentByteArray = await response.Content.ReadAsByteArrayAsync();
        //         return new MemoryStream(contentByteArray);
        //     }
        //     return null;
        // }
        // private static DataTable ConvertToDatatable(Stream ms)
        // {
        //     DataTable dataTable = new();

        //     ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        //     using (var package = new ExcelPackage(ms))
        //     {
        //         ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

        //         // Assuming the first row is the header
        //         for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
        //         {
        //             string columnName = worksheet.Cells[1, i].Value?.ToString();
        //             if (!string.IsNullOrEmpty(columnName))
        //             {
        //                 dataTable.Columns.Add(columnName);
        //             }
        //         }

        //         // Populate DataTable with data from Excel
        //         for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
        //         {
        //             DataRow dataRow = dataTable.NewRow();
        //             for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
        //             {
        //                 dataRow[col - 1] = worksheet.Cells[row, col].Value;
        //             }
        //             dataTable.Rows.Add(dataRow);
        //         }
        //     }

        //     return dataTable;
        // }

        // public async Task<ItemIds> GetItemTrackingFile(string trackingNo, string customerCode)
        // {
        //     string prefix = trackingNo[..2];
        //     string prefixNo = trackingNo.Substring(2, 2);
        //     string suffix = trackingNo[^2..];

        //     var reviews = await _itemTrackingReviewRepository.GetAllListAsync(x =>
        //                                                                         x.Prefix.Equals(prefix) &&
        //                                                                         x.PrefixNo.Equals(prefixNo) &&
        //                                                                         x.Suffix.Equals(suffix) &&
        //                                                                         x.CustomerCode.Equals(customerCode)
        //                                                                      );

        //     List<string> paths = [];

        //     foreach (var review in reviews)
        //     {
        //         var application = await _itemTrackingApplicationRepository.FirstOrDefaultAsync(x => x.Id.Equals(review.ApplicationId));

        //         if (application is not null) paths.Add(application.Path);
        //     }


        //     string itemIdFilePath = "";
        //     List<ItemTrackingWithPath> ItemWithPath = [];
        //     if (!paths.Count.Equals(0))
        //     {
        //         foreach (var path in paths)
        //         {
        //             ItemTrackingWithPath itemWithPath = new();      
        //             Stream excel_stream = await GetFileStream(path);
        //             DataTable dataTable = ConvertToDatatable(excel_stream);

        //             List<ItemTrackingIdDto> items = [];
        //             foreach (DataRow dr in dataTable.Rows)
        //             {
        //                 if (dr.ItemArray[0].ToString() != "")
        //                 {
        //                     items.Add(new ItemTrackingIdDto()
        //                     {
        //                         TrackingNo = dr.ItemArray[0].ToString(),
        //                         DateCreated = dr.ItemArray[1].ToString(),
        //                         DateUsed = dr.ItemArray[2].ToString(),
        //                         DispatchNo = dr.ItemArray[3].ToString(),
        //                     });

        //                     if (dr.ItemArray[0].ToString().Equals(trackingNo)) itemIdFilePath = path;
        //                 }
        //             }
        //             itemWithPath.ItemIds = items;
        //             itemWithPath.ExcelPath = path;
        //             ItemWithPath.Add(itemWithPath);
        //         }
        //     }

        //     return new ItemIds()
        //     {
        //         ItemWithPath = ItemWithPath,
        //         Path = itemIdFilePath
        //     };
        // }

        // public async Task<string> GetNextAvailableAnyAccountTrackingNumber(string trackingNo)
        // {
        //     var itemIds = await GetItemTrackingFile(trackingNo, "Any Account");

        //     var itemLists = itemIds.ItemWithPath.FirstOrDefault(x => x.ExcelPath.Equals(itemIds.Path));

        //     int index = itemLists.ItemIds.FindIndex(x => x.TrackingNo.Equals(trackingNo));

        //     bool isLast = index == itemLists.ItemIds.Count - 1;

        //     if(!isLast) return itemLists.ItemIds[index + 1].TrackingNo;

        //     return "";
        // }

        // public static async string GetNextAvailableAnyAccountTrackingNumber(
        //         string postalId, 
        //         bool? willUpdate = false, 
        //         string accNo = null, 
        //         int? customerId = null, 
        //         string postalCode = null, 
        //         string productCode = null, 
        //         bool mustFilterByPostalCode = false)
        // {
        //     string result = null;

        //     using (Core.ModelP.spsp db = new Core.ModelP.spsp())
        //     {
        //         using (var dbContextTransaction = db.Database.BeginTransaction())
        //         {
        //             var q = db.RegisterTrackingNumbers
        //                 .Where(u => u.PostalID == postalId && u.AccountNo == null && u.DateUsed == null);
        //             //.Where(u => u.PostalID == postalId)
        //             //.Where(u => u.AccountNo == null)
        //             //.Where(u => u.DateUsed == null);

        //             if (!string.IsNullOrWhiteSpace(productCode))
        //             {
        //                 q = q.Where(u => u.ProductCode == productCode);
        //             }

        //             if (mustFilterByPostalCode)
        //             {
        //                 if (!string.IsNullOrWhiteSpace(postalCode))
        //                 {
        //                     q = q.Where(u => u.PostalCode == postalCode);
        //                 }
        //             }

        //             var x = 0;
        //             var count = q.Count();

        //             if (count > 1)
        //             {
        //                 var maxRan = count - 1;

        //                 x = RNGUtil.Next(0, maxRan);
        //             }

        //             var item = q
        //                 .OrderBy(u => u.TrackingNo)
        //                 .Skip(x)
        //                 .Take(1)
        //                 .FirstOrDefault();

        //             if (item != null)
        //             {
        //                 result = item.TrackingNo;

        //                 if (willUpdate.GetValueOrDefault())
        //                 {

        //                     item.AccountNo = accNo;
        //                     item.CustomerID = customerId;
        //                     item.PostalCode = postalCode;

        //                     db.SaveChanges();
        //                 }
        //             }

        //             dbContextTransaction.Commit();
        //         }
        //     }

        //     return result;
        // }
        // public async Task<decimal> GetItemTopupValueFromPostalMaintenance(string postalCode, string serviceCode, string productCode)
        // {
        //     var postal = await _postalRepository.FirstOrDefaultAsync(x =>
        //                                                                 x.PostalCode.Equals(postalCode) &&
        //                                                                 x.ServiceCode.Equals(serviceCode) &&
        //                                                                 x.ProductCode.Equals(productCode)
        //                                                             );

        //     return postal is not null ? postal.ItemTopUpValue : 0m;
        // }

        // public async Task InsertUpdateTrackingNumber(string trackingNo, string accNo, int customerId, string postalCode)
        // {
        //     var item = await _itemTrackingRepository.FirstOrDefaultAsync(x => x.TrackingNo.Equals(trackingNo));

        //     if (item != null)
        //     {
        //         item.CustomerCode = accNo;
        //         item.CustomerId = customerId;
        //         item.ProductCode = postalCode;
        //     }
        //     else
        //     {
        //         await _itemTrackingRepository.InsertAsync(new ItemTracking()
        //         {
        //             TrackingNo = trackingNo,
        //             ApplicationId = 1,
        //             ReviewId = 1,
        //             CustomerId = customerId,
        //             CustomerCode = accNo,
        //             DateCreated = DateTime.Now,
        //             DateUsed = DateTime.Now,
        //             DispatchId = 1,
        //             DispatchNo = "",
        //             ProductCode = postalCode
        //         });
        //     }
        // }

        // public static async Task AlertIfLowThreshold(string postalId, int threshold, string productCode = null)
        // {
        //     threshold = 50000; //default

        //     if (postalId == "GE" && (productCode ?? "") == "R")
        //     {
        //         threshold = 2500;
        //     }
        //     if (postalId == "GE" && (productCode ?? "") == "PRT")
        //     {
        //         threshold = 2500;
        //     }
        //     if (postalId == "GE" && (productCode ?? "") == "OMT")
        //     {
        //         threshold = 10000;
        //     }
        //     if (postalId == "KG")
        //     {
        //         threshold = 50000;
        //     }
        //     if (postalId == "CO")
        //     {
        //         threshold = 50000;
        //     }
        //     if (postalId == "SL")
        //     {
        //         threshold = 50000;
        //     }
        //     if (postalId == "GQ")
        //     {
        //         threshold = 50000;
        //     }

        //     var q = await _itemTrackingRepository.RegisterTrackingNumbers
        //         .Where(u => u.PostalID == postalId)
        //         .Where(u => u.AccountNo == null)
        //         .Where(u => u.DateUsed == null);

        //     if (!string.IsNullOrWhiteSpace(productCode))
        //     {
        //         q = q.Where(u => u.ProductCode == productCode);
        //     }

        //     var count = q.Count();

        //     if (count == threshold)
        //     {
        //         //alert
        //         #region Email to Admins
        //         var enableEmailNotificationToAdmin = true;
        //         if (enableEmailNotificationToAdmin)
        //         {
        //             try
        //             {
        //                 string strHost = System.Web.Configuration.WebConfigurationManager.AppSettings["EmailHost"];
        //                 string strFromEmail = System.Web.Configuration.WebConfigurationManager.AppSettings["EmailFrom"];
        //                 string strPassEmailFrom = System.Web.Configuration.WebConfigurationManager.AppSettings["EmailFromPss"];
        //                 string strEmailPort = System.Web.Configuration.WebConfigurationManager.AppSettings["EmailFromPort"];
        //                 string subject = $"{postalId} API Item ID Low Threshold Hit";
        //                 string emailContent = $"Dear admins, API Item ID range for {postalId} {(string.IsNullOrWhiteSpace(productCode) ? "" : productCode)} has just hit the low threshold of {threshold.ToString()}. Kindly take action on this matter.";
        //                 string strAdminEmail = "";

        //                 strAdminEmail = Services.Common.SystemHelper.APIEmails;

        //                 if (string.IsNullOrEmpty(strAdminEmail))
        //                 {
        //                     strAdminEmail = string.Join(",", db.Users.Where(u => u.IsSuperAdmin == true).Where(u => u.Email != null && u.Email.Trim() != "").Select(u => u.Email).ToArray());
        //                 }

        //                 Services.Common.Email.SendMail(subject, emailContent, strFromEmail, strAdminEmail, strPassEmailFrom, strHost, strEmailPort, null, "", "");
        //             }
        //             catch (Exception exEmail)
        //             {

        //             }
        //         }
        //         #endregion
        //     }
        // }

    }
}


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
using Newtonsoft.Json;
using System.Net;


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
        IRepository<ApplicationSetting, int> applicationSettingRepository
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

        private async Task<string> GetAPGToken()
        {
            var TokenGenerationUrl = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("APG_TokenGenerationUrl"));
            var username = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("APG_UserName"));
            var password = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("APG_Password"));

            var apgTokenClient = new HttpClient();
            apgTokenClient.DefaultRequestHeaders.Clear();

            APGTokenRequest tokenRequest = new()
            {
                userName = username.Value,
                passWord = password.Value
            };

            var content = new StringContent(JsonConvert.SerializeObject(tokenRequest), Encoding.UTF8, "application/json");
            var apgTokenRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(TokenGenerationUrl.Value),
                Content = content,
            };
            using var apgTokenResponse = await apgTokenClient.SendAsync(apgTokenRequestMessage);
            apgTokenResponse.EnsureSuccessStatusCode();
            var apgTokenBody = await apgTokenResponse.Content.ReadAsStringAsync();
            var apgTokenResult = System.Text.Json.JsonSerializer.Deserialize<APGTokenResponse>(apgTokenBody);

            if (apgTokenResult != null)
            {
                var token_expiration = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("APG_TokenExpiration"));
                token_expiration.Value = apgTokenResult.token.expiration;

                var token = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("APG_Token"));
                token.Value = apgTokenResult.token.token;

                await _applicationSettingRepository.UpdateAsync(token_expiration).ConfigureAwait(false);

                return apgTokenResult.token.token;
            }

            return "";
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
        [Route("api/PreRegisterItem/APG")]
        public async Task<OutPreRegisterItem> PreRegisterItemAPG(InPreRegisterItem input)
        {
            const string SUCCESS = "success";
            const string FAILED = "failed";

            string customerCode = "";
            string clientSecret = "";

            var result = new OutPreRegisterItem();
            var newResponseIDRaw = Guid.NewGuid().ToString();

            result.ResponseID = GenerateMD5Hash(newResponseIDRaw);
            result.RefNo = input.RefNo;
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
                    var ParcelGenerationUrl = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("APG_ParcelGenerationUrl"));
                    var sender_identification = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("APG_SenderIdentification"));
                    var token_expiration = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("APG_TokenExpiration"));
                    var token = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("APG_Token"));

                    string apgToken = token.Value;

                    var token_expiration_date = DateTime.Parse(token_expiration.Value);
                    if (token_expiration_date > DateTime.Now) apgToken = await GetAPGToken();

                    var httpstatus = HttpStatusCode.Unauthorized;

                    if (ParcelGenerationUrl != null)
                    {
                        do
                        {
                            var apgClient = new HttpClient();
                            apgClient.DefaultRequestHeaders.Clear();
                            apgClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apgToken);

                            List<Commodity> commodities = [];
                            commodities.Add(new Commodity()
                            {
                                description = input.ItemDesc,
                                value = input.ItemValue,
                                weight = input.Weight,
                                hsTariffNumber = input.HSCode,
                                quantity = 1,
                                countryOfGoods = ""
                            });

                            List<Package> packages = [];
                            packages.Add(new Package()
                            {
                                preAlertCode = "",
                                stamp = input.RefNo,
                                senderCode = "",
                                senderName = input.SenderName,
                                senderIdentification = sender_identification.Value,
                                senderCountry = "HK",
                                receiverName = input.RecipientName,
                                receiverIdentification = "",
                                receiverAddress1 = input.RecipientAddress,
                                receiverAddress2 = "",
                                receiverAddress3 = "",
                                receiverNeighborhood = "",
                                receiverZipCode = input.RecipientPostcode,
                                receiverCity = input.RecipientCity,
                                receiverState = input.RecipientCity,
                                receiverCountry = input.RecipientCountry,
                                receiverPhoneNumber = input.RecipientContactNo,
                                receiverEmail = input.RecipientEmail,
                                receiverTaxId = "",
                                division = 3,
                                serviceValue = 1,
                                serviceOptValue = 7,
                                dimensionTypeValue = 1,
                                weightTypeValue = 1,
                                commodities = commodities
                            });


                            APGRequest apgRequest = new()
                            {
                                packages = packages
                            };

                            var content = new StringContent(JsonConvert.SerializeObject(apgRequest), Encoding.UTF8, "application/json");
                            var apgRequestMessage = new HttpRequestMessage
                            {
                                Method = HttpMethod.Post,
                                RequestUri = new Uri(ParcelGenerationUrl.Value),
                                Content = content,
                            };
                            using var apgResponse = await apgClient.SendAsync(apgRequestMessage);
                            httpstatus = apgResponse.StatusCode;

                            if (httpstatus == HttpStatusCode.OK)
                            {
                                var apgBody = await apgResponse.Content.ReadAsStringAsync();
                                var apgResult = System.Text.Json.JsonSerializer.Deserialize<APGResponse>(apgBody);

                                if (apgResult != null)
                                {
                                    if (apgResult.status == "Successfully Saved")
                                    {
                                        result.APIItemID = apgResult.tracking;
                                        result.Status = SUCCESS;
                                        result.Errors.Clear();
                                    }
                                    else
                                    {
                                        result.APIItemID = apgResult.tracking;
                                        result.Status = FAILED;
                                        result.Errors.Add(apgResult.status);
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
                                if (httpstatus == HttpStatusCode.Unauthorized) apgToken = await GetAPGToken();
                                else
                                {
                                    result.APIItemID = "";
                                    result.Status = FAILED;
                                    result.Errors.Add(httpstatus.ToString());
                                }
                            }
                        }
                        while (httpstatus == HttpStatusCode.Unauthorized);
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
                        if (postalSupported != "DO")
                        {
                            if (!string.Equals(input.ServiceCode.Trim(), "TS", StringComparison.OrdinalIgnoreCase))
                            {
                                result.Errors.Add("Invalid service code");
                            }
                        }
                        #endregion

                        if (postalSupported == "KG")
                        {
                            #region Product Code
                            if (!string.Equals(input.ProductCode.Trim(), "OMT", StringComparison.OrdinalIgnoreCase) &&
                                !string.Equals(input.ProductCode.Trim(), "PRT", StringComparison.OrdinalIgnoreCase))
                            {
                                result.Errors.Add("Invalid product code");
                            }
                            #endregion
                        }
                        else
                        {
                            #region Product Code
                            if (postalSupported != "DO")
                            {
                                if (!string.Equals(input.ProductCode.Trim(), "OMT", StringComparison.OrdinalIgnoreCase))
                                {
                                    result.Errors.Add("Invalid product code");
                                }
                            }
                            #endregion
                        }

                        if (postalSupported == "KG" || postalSupported == "GQ")
                        {
                            #region IOSS Tax
                            var willValidateIOSS = true;
                            if (willValidateIOSS)
                            {
                                var countryListIOSS = new List<string> { "IE", "HR", "MT", "CZ" };

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

                        var isItemIDAutoMandatory = true;
                        if (isItemIDAutoMandatory)
                        {
                            if ((string.IsNullOrWhiteSpace(input.ItemID) ? "" : input.ItemID.ToLower().Trim()) != auto.ToLower().Trim())
                            {
                                result.Errors.Add("Invalid ItemID value. ItemID must be set to 'auto'");
                            }
                        }

                        if (!postalSupported.Equals("GQ") && !postalSupported.Equals("DO"))
                        {
                            var nextItemIdFromRange = await GetNextAvailableTrackingNumber(postalCode);
                            if (string.IsNullOrWhiteSpace(nextItemIdFromRange)) result.Errors.Add("Insufficient API Item ID. Please contact us for assistance");
                        }

                        #endregion

                        if (!string.IsNullOrWhiteSpace(input.PoolItemId))
                        {
                            var isOwned = await IsTrackingNoOwner(customerCode, input.PoolItemId, input.ProductCode);
                            if (!isOwned) result.Errors.Add("Invalid Pool Item ID");
                        }

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
                                    DispatchDate = null,
                                    BatchId = "",
                                    TransactionDateTime = DateTime.Now
                                });

                                await _dispatchRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);
                            }

                            string newItemIdFromSPS = null;

                            if (!string.IsNullOrWhiteSpace(input.PoolItemId))
                            {
                                //---- Nothing to generate because the pool item ID already assigned to this Customer Code. ----//
                                newItemIdFromSPS = input.PoolItemId;
                                input.ItemID = newItemIdFromSPS;

                                try
                                {
                                    await InsertUpdateTrackingNumber(newItemIdFromSPS, customerCode, cust.Id, input.PostalCode, false);

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
                            else
                            {
                                if (postalSupported == "DO")
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
                                            newItemIdFromSPS = await GetNextAvailableTrackingNumber(postalCode, true, input.ProductCode, useCustomerPool ? customerCode : null);

                                            if (!string.IsNullOrWhiteSpace(newItemIdFromSPS))
                                            {
                                                try
                                                {
                                                    await InsertUpdateTrackingNumber(newItemIdFromSPS, customerCode, cust.Id, input.PostalCode);

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
                                                newItemIdFromSPS = await GetNextAvailableTrackingNumber(postalCode, true, input.ProductCode, useCustomerPool ? customerCode : null);

                                                if (!string.IsNullOrWhiteSpace(newItemIdFromSPS))
                                                {
                                                    try
                                                    {
                                                        await InsertUpdateTrackingNumber(newItemIdFromSPS, customerCode, cust.Id, input.PostalCode);

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
                                        //string newItemIdFromSPS = null;

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

                                            newItemIdFromSPS = trackingNumbers.FirstOrDefault();

                                            if (!string.IsNullOrWhiteSpace(newItemIdFromSPS))
                                            {
                                                try
                                                {
                                                    var itemIdDetails = await _itemTrackingRepository.FirstOrDefaultAsync(x => x.TrackingNo.Equals(newItemIdFromSPS));

                                                    if (itemIdDetails is not null)
                                                    {
                                                        await _itemTrackingRepository.InsertAsync(new ItemTracking()
                                                        {
                                                            TrackingNo = newItemIdFromSPS,
                                                            ApplicationId = itemIdDetails.ApplicationId,
                                                            ReviewId = itemIdDetails.ReviewId,
                                                            CustomerId = cust.Id,
                                                            CustomerCode = customerCode,
                                                            DateCreated = itemIdDetails.DateCreated,
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

                                                    result.ItemID = newItemIdFromSPS;
                                                    result.Status = SUCCESS;

                                                    // spsq.ReceiveMessage();
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
                                            var enableOld = false;
                                            if (enableOld)
                                            {
                                                #region NOT IN USE
                                                if (isItemIDAutoMandatory)
                                                {
                                                    trackingNumbers = await GenerateTrackingNumbers(startNo, 1, prefix, prefixNo, prefix);

                                                    newItemIdFromSPS = trackingNumbers.FirstOrDefault();

                                                    if (!string.IsNullOrWhiteSpace(newItemIdFromSPS))
                                                    {
                                                        try
                                                        {
                                                            var itemIdDetails = await GetItemTrackingFile(customerCode, newItemIdFromSPS);

                                                            if (itemIdDetails is not null)
                                                            {
                                                                var matched = itemIdDetails.ItemWithPath.FirstOrDefault(x => x.ExcelPath.Equals(itemIdDetails.Path));

                                                                await _itemTrackingRepository.InsertAsync(new ItemTracking()
                                                                {
                                                                    TrackingNo = newItemIdFromSPS,
                                                                    ApplicationId = matched.ApplicationId,
                                                                    ReviewId = matched.ReviewId,
                                                                    CustomerId = cust.Id,
                                                                    CustomerCode = customerCode,
                                                                    DateCreated = matched.DateCreated,
                                                                    ProductCode = matched.ProductCode,
                                                                    DispatchNo = ""
                                                                }).ConfigureAwait(false);
                                                            }

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
                                                #endregion
                                            }
                                        }
                                        #endregion
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region New Item ID from SPS
                                    if ((string.IsNullOrWhiteSpace(input.ItemID) ? "" : input.ItemID).ToLower().Trim() == auto.ToLower().Trim())
                                    {
                                        newItemIdFromSPS = await GetNextAvailableTrackingNumber(postalCode, false, postalSupported == "KG" ? input.ProductCode : null, null);

                                        if (string.IsNullOrWhiteSpace(newItemIdFromSPS)) result.Errors.Add("Insufficient Pool Item ID");
                                        else
                                        {
                                            try
                                            {
                                                await InsertUpdateTrackingNumber(newItemIdFromSPS, customerCode, cust.Id, input.PostalCode);

                                                await AlertIfLowThreshold(postalCode, threshold);

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
                                        if (isItemIDAutoMandatory)
                                        {
                                            newItemIdFromSPS = await GetNextAvailableTrackingNumber(postalCode, false, input.ProductCode, null);

                                            if (string.IsNullOrWhiteSpace(newItemIdFromSPS)) result.Errors.Add("Insufficient Pool Item ID");
                                            else
                                            {
                                                try
                                                {
                                                    await InsertUpdateTrackingNumber(newItemIdFromSPS, customerCode, cust.Id, input.PostalCode);

                                                    await AlertIfLowThreshold(postalCode, threshold);

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
                                    }
                                    #endregion
                                }
                            }

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

                if (application is not null) paths.Add(application.Path);
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
        private async Task InsertUpdateTrackingNumber(string trackingNo, string customerCode, long customerId, string postalCode, bool isAnyAccount = true)
        {
            var item = await _itemTrackingRepository.FirstOrDefaultAsync(x => x.TrackingNo.Equals(trackingNo));

            if (item is not null)
            {
                item.CustomerCode = customerCode;
                item.CustomerId = customerId;
                item.ProductCode = postalCode;
            }
            else
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
                        ProductCode = matched.ProductCode,
                        DispatchNo = ""
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
            int lastRunningNo = 0;

            var itemIdRunningNos = await _itemIdRunningNoRepository.FirstOrDefaultAsync(x =>
                                                                                            x.Prefix.Equals(prefix) &&
                                                                                            x.PrefixNo.Equals(prefixNo) &&
                                                                                            x.Suffix.Equals(suffix)
                                                                                        );

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
    }
}


using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.Chibis.Dto;
using SND.SMP.FileUploadAPI.Dto;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Abp.EntityFrameworkCore.Repositories;
using Microsoft.Extensions.Configuration;
using SND.SMP.Queues;
using System.Data;
using OfficeOpenXml;
using SND.SMP.ApplicationSettings;
using System.Collections.Generic;
using Abp.UI;
using SND.SMP.DispatchValidations;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel;
using SND.SMP.Shared;
using System.IO;
using SND.SMP.ItemTrackingReviews;
using SND.SMP.Dispatches;
using SND.SMP.ItemTrackings;
using SND.SMP.Items;
using SND.SMP.ItemMins;
using SND.SMP.Bags;
using SND.SMP.DispatchUsedAmounts;
using SND.SMP.Wallets;
using SND.SMP.CustomerTransactions;
using SND.SMP.EWalletTypes;
using SND.SMP.Currencies;
using SND.SMP.ItemTrackingApplications;
using SND.SMP.TrackingNoForUpdates;
using static SND.SMP.Shared.EnumConst;

namespace SND.SMP.Chibis
{
    public class ChibiAppService(
        IRepository<Chibi, long> repository,
        IRepository<Queue, long> queueRepository,
        IRepository<ApplicationSetting, int> applicationSettingRepository,
        IRepository<DispatchValidation, string> dispatchValidationRepository,
        IRepository<ItemTrackingReview, int> itemTrackingReviewsRepository,
        IRepository<ItemTracking, int> itemTrackingsRepository,
        IRepository<Item, string> itemsRepository,
        IRepository<ItemMin, string> itemMinsRepository,
        IRepository<Dispatch, int> dispatchRepository,
        IRepository<Bag, int> bagsRepository,
        IMemoryCache memoryCache,
        IRepository<Wallet, string> walletRepository,
        IRepository<DispatchUsedAmount, int> dispatchUsedAmountRepository,
        IRepository<CustomerTransaction, long> customerTransactionRepository,
        IRepository<EWalletType, long> ewalletTypeRepository,
        IRepository<Currency, long> currencyRepository,
        IRepository<ItemTrackingApplication, int> itemTrackingApplicationRepository,
        IRepository<ItemTrackingReview, int> itemTrackingReviewRepository,
        IRepository<TrackingNoForUpdate, long> trackingNoForUpdateRepository
    ) : AsyncCrudAppService<Chibi, ChibiDto, long, PagedChibiResultRequestDto>(repository)
    {
        private readonly IRepository<Queue, long> _queueRepository = queueRepository;
        private readonly IRepository<ApplicationSetting, int> _applicationSettingRepository = applicationSettingRepository;
        private readonly IRepository<DispatchValidation, string> _dispatchValidationRepository = dispatchValidationRepository;
        private readonly IRepository<ItemTrackingReview, int> _itemTrackingReviewsRepository = itemTrackingReviewsRepository;
        private readonly IRepository<ItemTracking, int> _itemTrackingsRepository = itemTrackingsRepository;
        private readonly IRepository<Item, string> _itemsRepository = itemsRepository;
        private readonly IRepository<ItemMin, string> _itemMinsRepository = itemMinsRepository;
        private readonly IRepository<Bag, int> _bagsRepository = bagsRepository;
        private readonly IRepository<Dispatch, int> _dispatchRepository = dispatchRepository;
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly IRepository<DispatchUsedAmount, int> _dispatchUsedAmountRepository = dispatchUsedAmountRepository;
        private readonly IRepository<Wallet, string> _walletRepository = walletRepository;
        private readonly IRepository<CustomerTransaction, long> _customerTransactionRepository = customerTransactionRepository;
        private readonly IRepository<EWalletType, long> _ewalletTypeRepository = ewalletTypeRepository;
        private readonly IRepository<Currency, long> _currencyRepository = currencyRepository;
        private readonly IRepository<ItemTrackingApplication, int> _itemTrackingApplicationRepository = itemTrackingApplicationRepository;
        private readonly IRepository<ItemTrackingReview, int> _itemTrackingReviewRepository = itemTrackingReviewRepository;
        private readonly IRepository<TrackingNoForUpdate, long> _trackingNoForUpdateRepository = trackingNoForUpdateRepository;

        private static async Task<string> GetFileStreamAsString(string url)
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string contentString = await response.Content.ReadAsStringAsync();
                return contentString;
            }
            return null;
        }

        private async Task<Dictionary<string, Album>> PrepareAlbums()
        {
            Dictionary<string, Album> albumsDict = [];

            var albums = await GetAlbumsAsync();

            foreach (var album in albums)
            {
                var temp_album = await GetAlbumAsync(album.uuid);
                album.files = temp_album.files.Count == 0 ? [] : temp_album.files;
                albumsDict.TryAdd(album.name, album);
            }

            _memoryCache.Remove(EnumConst.GlobalConst.Albums);
            _memoryCache.Set(EnumConst.GlobalConst.Albums, albumsDict);

            return albumsDict;
        }

        private static DataTable ConvertToDatatable(Stream ms)
        {
            DataTable dataTable = new();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

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

        private async Task<List<Album>> GetDictAlbums()
        {
            _memoryCache.TryGetValue(EnumConst.GlobalConst.Albums, out Dictionary<string, Album> albumsDict);

            if (albumsDict is null || albumsDict.Count == 0) albumsDict = await PrepareAlbums();

            return [.. albumsDict.Values];
        }

        private async Task<bool> CreateInsertPostalAlbum(string postalCode, string file_uuid)
        {
            var album = await CreateAlbumAsync("Postal_" + postalCode);
            var addFileToAlbum = await AddFileToAlbum(album.album.uuid, file_uuid);
            return true;
        }

        private async Task<bool> CreateInsertServiceAlbum(string serviceCode, string file_uuid)
        {
            var album = await CreateAlbumAsync("Service_" + serviceCode);
            var addFileToAlbum = await AddFileToAlbum(album.album.uuid, file_uuid);
            return true;
        }

        private async Task<bool> CreateInsertProductAlbum(string productCode, string file_uuid)
        {
            var album = await CreateAlbumAsync("Product_" + productCode);
            var addFileToAlbum = await AddFileToAlbum(album.album.uuid, file_uuid);
            return true;
        }

        private async Task<bool> InsertFileToAlbum(string file_uuid, bool isError, bool isDispatchTrackingUpdate, bool isInvoiceGeneration, string postalCode = null, string serviceCode = null, string productCode = null)
        {
            List<Album> albums = await GetDictAlbums();
            if (isError)
            {
                if (albums.Count == 0)
                {
                    var album = await CreateAlbumAsync("ErrorDetails");
                    await AddFileToAlbum(album.album.uuid, file_uuid).ConfigureAwait(false);
                }
                else
                {
                    var error_album = albums.FirstOrDefault(a => a.name == "ErrorDetails");
                    if (error_album == null)
                    {
                        var album = await CreateAlbumAsync("ErrorDetails");
                        await AddFileToAlbum(album.album.uuid, file_uuid).ConfigureAwait(false);
                    }
                    else await AddFileToAlbum(error_album.uuid, file_uuid).ConfigureAwait(false);
                }
            }
            else if (isDispatchTrackingUpdate)
            {
                if (albums.Count == 0)
                {
                    var album = await CreateAlbumAsync("DispatchTrackingUpdates");
                    await AddFileToAlbum(album.album.uuid, file_uuid).ConfigureAwait(false);
                }
                else
                {
                    var error_album = albums.FirstOrDefault(a => a.name == "DispatchTrackingUpdates");
                    if (error_album == null)
                    {
                        var album = await CreateAlbumAsync("DispatchTrackingUpdates");
                        await AddFileToAlbum(album.album.uuid, file_uuid).ConfigureAwait(false);
                    }
                    else await AddFileToAlbum(error_album.uuid, file_uuid).ConfigureAwait(false);
                }
            }
            else if (isInvoiceGeneration)
            {
                if (albums.Count == 0)
                {
                    var album = await CreateAlbumAsync("InvoiceInfosForGeneration");
                    await AddFileToAlbum(album.album.uuid, file_uuid).ConfigureAwait(false);
                }
                else
                {
                    var error_album = albums.FirstOrDefault(a => a.name == "InvoiceInfosForGeneration");
                    if (error_album == null)
                    {
                        var album = await CreateAlbumAsync("InvoiceInfosForGeneration");
                        await AddFileToAlbum(album.album.uuid, file_uuid).ConfigureAwait(false);
                    }
                    else await AddFileToAlbum(error_album.uuid, file_uuid).ConfigureAwait(false);
                }
            }
            else
            {
                if (albums.Count == 0)
                {
                    if (postalCode != null) await CreateInsertPostalAlbum(postalCode[..2], file_uuid).ConfigureAwait(false);
                    if (serviceCode != null) await CreateInsertServiceAlbum(serviceCode, file_uuid).ConfigureAwait(false);
                    if (productCode != null) await CreateInsertProductAlbum(productCode, file_uuid).ConfigureAwait(false);
                }
                else
                {
                    if (postalCode != null)
                    {
                        var postal_album = albums.FirstOrDefault(a => a.name == "Postal_" + postalCode[..2]);
                        if (postal_album == null) await CreateInsertPostalAlbum(postalCode[..2], file_uuid).ConfigureAwait(false);
                        else await AddFileToAlbum(postal_album.uuid, file_uuid).ConfigureAwait(false);
                    }
                    if (serviceCode != null)
                    {
                        var service_album = albums.FirstOrDefault(a => a.name == "Service_" + serviceCode);
                        if (service_album == null) await CreateInsertServiceAlbum(serviceCode, file_uuid).ConfigureAwait(false);
                        else await AddFileToAlbum(service_album.uuid, file_uuid).ConfigureAwait(false);
                    }
                    if (productCode != null)
                    {
                        var product_album = albums.FirstOrDefault(a => a.name == "Product_" + productCode);
                        if (product_album == null) await CreateInsertProductAlbum(productCode, file_uuid).ConfigureAwait(false);
                        else await AddFileToAlbum(product_album.uuid, file_uuid).ConfigureAwait(false);
                    }
                }
            }
            return true;
        }

        private async Task<GetAlbumDto> GetAlbumAsync(string uuid)
        {
            var chibiKey = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var chibiURL = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", chibiKey.Value);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(chibiURL.Value + "album/" + uuid),
            };

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GetAlbumDto>(body);

            return result;
        }

        private async Task<List<Album>> GetAlbumsAsync()
        {
            var chibiKey = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var chibiURL = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", chibiKey.Value);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(chibiURL.Value + "albums"),
            };

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AlbumDto>(body);

            return result.albums;
        }

        private async Task<CreateAlbumDto> CreateAlbumAsync(string name)
        {
            var chibiKey = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var chibiURL = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", chibiKey.Value);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(chibiURL.Value + "album/create"),
                Content = new StringContent("{\n  \"name\": \"" + name + "\"\n}")
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CreateAlbumDto>(body);

            return result;
        }

        private async Task DeleteAlbumAsync(string uuid)
        {
            var chibiKey = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var chibiURL = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", chibiKey.Value);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(chibiURL.Value + "album/" + uuid),
            };

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }

        private async Task<bool> AddFileToAlbum(string album_uuid, string file_uuid)
        {
            var chibiKey = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var chibiURL = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", chibiKey.Value);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(chibiURL.Value + $"file/{file_uuid}/album/{album_uuid}"),
            };

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                await PrepareAlbums();
                return true;
            };

            return false;
        }

        private static bool IsUpdateParticulars(string PostalCode, string ServiceCode, string ProductCode, string RateOptionId)
        {
            if (PostalCode != "" && PostalCode != null) return true;
            if (ServiceCode != "" && ServiceCode != null) return true;
            if (ProductCode != "" && ProductCode != null) return true;
            if (RateOptionId != "") return true;
            return false;
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

        private async Task UpdateItemTrackingFile(string customerCode, List<string> trackingNos)
        {
            var itemIdPathWithDatatables = await GetItemTrackingFiles(customerCode, trackingNos);
            List<ItemIdPath> itemIdPaths = itemIdPathWithDatatables.ItemIdPaths;
            List<DataTable> DataTablesByPath = itemIdPathWithDatatables.DataTablesByPath;
            List<string> editedTablePaths = [];

            var distinctedPaths = itemIdPaths.DistinctBy(x => x.Path).ToList();

            foreach (var itemId in itemIdPaths)
            {
                var splits = itemId.Path.Split(",");
                DataTable foundTable = DataTablesByPath.FirstOrDefault(dt => dt.TableName == splits[0].ToString());

                if (foundTable != null)
                {
                    DataRow[] rowsToUpdate = foundTable.Select($"TrackingNo = '{itemId.ItemId}'");

                    foreach (DataRow rowToUpdate in rowsToUpdate)
                    {
                        rowToUpdate["DateUsed"] = "";
                        rowToUpdate["DispatchNo"] = "";
                    }

                    if (rowsToUpdate.Length > 0)
                    {
                        if (!editedTablePaths.Contains(splits[0].ToString()))
                            editedTablePaths.Add(splits[0].ToString());
                    }

                    foundTable.AcceptChanges();
                }
            }

            var chibis = await Repository.GetAllListAsync();
            var itemTrackingApplications = await _itemTrackingApplicationRepository.GetAllListAsync();
            var itemTrackingReviews = await _itemTrackingReviewRepository.GetAllListAsync();

            foreach (DataTable dataTableByPath in DataTablesByPath)
            {
                if (editedTablePaths.Contains(dataTableByPath.TableName))
                {
                    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                    using ExcelPackage package = new();
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Tracking Numbers");

                    for (int i = 0; i < dataTableByPath.Columns.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = dataTableByPath.Columns[i].ColumnName;
                    }
                    for (int i = 0; i < dataTableByPath.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataTableByPath.Columns.Count; j++)
                        {
                            worksheet.Cells[i + 2, j + 1].Value = dataTableByPath.Rows[i][j];
                        }
                    }

                    Stream excelStream = new MemoryStream();
                    package.SaveAs(excelStream);
                    excelStream.Position = 0;

                    var chibiFile = chibis.FirstOrDefault(x => x.URL.Equals(dataTableByPath.TableName));
                    var generatedName = chibiFile.GeneratedName;
                    var fileName = chibiFile.OriginalName.Replace("_" + generatedName, "") + ".xlsx";
                    var application = itemTrackingApplications.FirstOrDefault(x => x.Path.Equals(dataTableByPath.TableName));

                    var review = itemTrackingReviews.FirstOrDefault(x => x.ApplicationId.Equals(application.Id));

                    ChibiUpload uploadExcel = await InsertExcelFileToChibi(excelStream, fileName, originalName: null, postalCode: review.PostalCode, productCode: review.ProductCode);

                    await Repository.DeleteAsync(chibiFile).ConfigureAwait(false);

                    application.Path = uploadExcel.url;

                    await _itemTrackingApplicationRepository.UpdateAsync(application).ConfigureAwait(false);
                }
            }
        }

        private async Task<ChibiUpload> InsertExcelFileToChibi(Stream excel, string fileName, string originalName = null, string postalCode = null, string productCode = null)
        {
            var chibiKey = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var chibiURL = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));

            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", chibiKey.Value);
            var formData = new MultipartFormDataContent();

            var xlsxContent = new StreamContent(excel);
            xlsxContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            formData.Add(xlsxContent, "file", fileName);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(chibiURL.Value + "upload"),
                Content = formData,
            };

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<ChibiUpload>(body);

            if (result != null)
            {
                result.originalName = fileName.Replace(".xlsx", "") + $"_{result.name}";
                //Insert to DB
                Chibi entity = new()
                {
                    FileName = result.name == null ? "" : DateTime.Now.ToString("yyyyMMdd") + "_" + result.name,
                    UUID = result.uuid ?? "",
                    URL = result.url ?? "",
                    OriginalName = originalName is null ? result.originalName : originalName,
                    GeneratedName = result.name ?? ""
                };

                await Repository.InsertAsync(entity).ConfigureAwait(false);

                await InsertFileToAlbum(result.uuid, false, false, false, postalCode, null, productCode);
            }

            return result;
        }

        private async Task<ItemIdPathWithDatatables> GetItemTrackingFiles(string customerCode, List<string> trackingNos)
        {
            List<DataTable> DataTablesByPath = [];

            List<ItemTrackingReview> reviews = [];

            List<PrefixSuffixCustomerCode> prefixSuffixCustomerCodes = [];

            foreach (var trackingNo in trackingNos)
            {
                string prefix = trackingNo[..2];
                string suffix = trackingNo[^2..];

                prefixSuffixCustomerCodes.Add(new PrefixSuffixCustomerCode
                {
                    Prefix = prefix,
                    Suffix = suffix,
                    CustomerCode = customerCode
                });
            }

            prefixSuffixCustomerCodes = prefixSuffixCustomerCodes
                                        .GroupBy(x => new { x.CustomerCode, x.Prefix, x.Suffix })
                                        .Select(g => g.First())
                                        .ToList();

            foreach (var prefixSuffixCustomerCode in prefixSuffixCustomerCodes)
            {
                prefixSuffixCustomerCodes.Add(new PrefixSuffixCustomerCode
                {
                    Prefix = prefixSuffixCustomerCode.Prefix,
                    Suffix = prefixSuffixCustomerCode.Suffix,
                    CustomerCode = "Any Account"
                });
            }

            var reviewList = await _itemTrackingReviewRepository.GetAllListAsync();

            foreach (var prefixSuffixCustomerCode in prefixSuffixCustomerCodes)
            {
                var tempReviews = reviewList.Where(x => x.Prefix.Equals(prefixSuffixCustomerCode.Prefix) &&
                                                            x.Suffix.Equals(prefixSuffixCustomerCode.Suffix) &&
                                                            x.CustomerCode.Equals(prefixSuffixCustomerCode.CustomerCode)).ToList();

                if (tempReviews.Count > 0) foreach (var review in tempReviews) reviews.Add(review);
            }


            List<string> paths = [];
            List<ItemIdPath> itemIdFilePath = [];

            var applications = await _itemTrackingApplicationRepository.GetAllListAsync();

            foreach (var review in reviews)
            {
                var application = applications.FirstOrDefault(x => x.Id.Equals(review.ApplicationId));

                if (application is not null) paths.Add(application.Path + "," + review.PostalCode + "," + review.ProductCode);
            }

            if (!paths.Count.Equals(0))
            {
                //---- Gets all Excel files and retrieves its info to create the object ItemIds ----//
                foreach (var path in paths)
                {
                    string[] splits = path.Split(",");
                    ItemTrackingWithPath itemWithPath = new();
                    Stream excel_stream = await GetFileStream(splits[0].ToString());
                    DataTable dataTable = ConvertToDatatable(excel_stream);
                    dataTable.TableName = splits[0].ToString();
                    DataTablesByPath.Add(dataTable);

                    foreach (DataRow dr in dataTable.Rows)
                    {
                        if (dr.ItemArray[0].ToString() != "")
                        {
                            if (trackingNos.Contains(dr.ItemArray[0].ToString())) itemIdFilePath.Add(
                                new ItemIdPath
                                {
                                    ItemId = dr.ItemArray[0].ToString(),
                                    Path = path,
                                });
                        }
                    }
                }
            }

            return new ItemIdPathWithDatatables { ItemIdPaths = itemIdFilePath, DataTablesByPath = DataTablesByPath };
        }

        protected override IQueryable<Chibi> CreateFilteredQuery(PagedChibiResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.FileName.Contains(input.Keyword) ||
                    x.UUID.Contains(input.Keyword) ||
                    x.URL.Contains(input.Keyword) ||
                    x.OriginalName.Contains(input.Keyword) ||
                    x.GeneratedName.Contains(input.Keyword));
        }




        public async Task<List<DispatchValidateDto>> GetDispatchValidationError(string dispatchNo)
        {
            var file = await Repository.FirstOrDefaultAsync(x => x.OriginalName == dispatchNo) ?? throw new UserFriendlyException("Error Details Not Found");

            using var httpClient = new HttpClient();
            try
            {
                using var response = await httpClient.GetAsync(file.URL);
                if (response.IsSuccessStatusCode)
                {
                    string contentString = await response.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<DispatchValidateDto>>(contentString);
                }
                else
                {
                    throw new UserFriendlyException($"Failed to download file. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"Error downloading file: {ex.Message}");
            }
        }

        public async Task<GetFileDto> GetFile(string uuid)
        {
            var chibiKey = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var chibiURL = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", chibiKey.Value);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(chibiURL.Value + $"file/{uuid}"),
            };

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GetFileDto>(body);
        }

        public async Task<bool> DeleteFile(string uuid)
        {
            var chibiKey = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var chibiURL = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", chibiKey.Value);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(chibiURL.Value + $"file/{uuid}"),
            };

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<bool> DeleteDispatch(string path, string dispatchNo)
        {
            var dispatchValidation = await _dispatchValidationRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatchNo)) ?? throw new UserFriendlyException("No Dispatch Validation Found");
            var dispatchFile = await Repository.FirstOrDefaultAsync(x => x.URL.Equals(path));
            var dispatchFilePair = await Repository.GetAllListAsync(x => x.OriginalName.Equals(dispatchFile.OriginalName));
            var excelDispatchFile = dispatchFilePair.FirstOrDefault(x => x.URL.Contains("xlsx"));

            var dispatch = await _dispatchRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatchNo));

            if (dispatch is not null)
            {
                var items = await _itemsRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatch.Id));
                var itemTrackings = await _itemTrackingsRepository.GetAllListAsync();

                if (items.Count > 0)
                {
                    List<ItemTracking> itemTrackingsList = [];
                    List<TrackingNoForUpdate> trackingNoForUpdates = [];
                    foreach (var item in items)
                    {
                        var itemTracking = itemTrackings.FirstOrDefault(x => x.TrackingNo.Equals(item.Id));
                        if (itemTracking is not null)
                        {
                            itemTracking.DispatchId = 0;
                            itemTracking.DispatchNo = "";
                            itemTracking.DateUsed = DateTime.MinValue;

                            itemTrackingsList.Add(itemTracking);
                        }
                        trackingNoForUpdates.Add(new TrackingNoForUpdate()
                        {
                            TrackingNo = item.Id,
                            DispatchNo = dispatch.DispatchNo,
                            ProcessType = TrackingNoForUpdateConst.STATUS_DELETE,
                        });
                    }
                    _itemTrackingsRepository.GetDbContext().UpdateRange(itemTrackingsList);
                    await _itemTrackingsRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                    _itemsRepository.RemoveRange(items);
                    await _itemsRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                    _trackingNoForUpdateRepository.InsertRange(trackingNoForUpdates);
                    await _trackingNoForUpdateRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                    await _queueRepository.InsertAsync(new Queue()
                    {
                        EventType = "Update Tracking",
                        FilePath = dispatch.DispatchNo,
                        Status = QueueEnumConst.STATUS_NEW,
                        DateCreated = DateTime.Now,
                        DeleteFileOnFailed = false,
                        DeleteFileOnSuccess = false,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.MinValue,
                        TookInSec = 0,
                    }).ConfigureAwait(false);
                }

                var itemMins = await _itemMinsRepository.GetAllListAsync(x => x.DispatchID.Equals(dispatch.Id));
                if (itemMins.Count > 0)
                {
                    _itemMinsRepository.RemoveRange(itemMins);
                    await _itemMinsRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);
                }

                var bags = await _bagsRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id));
                if (bags.Count > 0)
                {
                    _bagsRepository.RemoveRange(bags);
                    await _bagsRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);
                }

                var dispatchUsedAmount = await _dispatchUsedAmountRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatch.DispatchNo));

                if (dispatchUsedAmount is not null)
                {
                    var wallet = await _walletRepository.FirstOrDefaultAsync(x => x.Id.Equals(dispatchUsedAmount.Wallet));

                    if (wallet is not null)
                    {
                        var refundAmount = dispatchUsedAmount.Amount;
                        wallet.Balance += refundAmount;
                        await _walletRepository.UpdateAsync(wallet).ConfigureAwait(false);

                        var eWallet = await _ewalletTypeRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.EWalletType));
                        var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(wallet.Currency));

                        await _customerTransactionRepository.InsertAsync(new CustomerTransaction()
                        {
                            Wallet = wallet.Id,
                            Customer = wallet.Customer,
                            PaymentMode = eWallet.Type,
                            Currency = currency.Abbr,
                            TransactionType = "Refund Amount after Delete Dispatch",
                            Amount = Math.Abs(refundAmount),
                            ReferenceNo = dispatch.DispatchNo,
                            Description = $"Credited {currency.Abbr} {decimal.Round(Math.Abs(refundAmount), 2, MidpointRounding.AwayFromZero)} to {wallet.Customer}'s {wallet.Id} Wallet. Remaining {currency.Abbr} {decimal.Round(wallet.Balance, 2, MidpointRounding.AwayFromZero)}.",
                            TransactionDate = DateTime.Now,
                        }).ConfigureAwait(false);
                    }
                    await _dispatchUsedAmountRepository.DeleteAsync(dispatchUsedAmount).ConfigureAwait(false);
                }

                dispatch.IsActive = false;
                await _dispatchRepository.UpdateAsync(dispatch).ConfigureAwait(false);
            }

            var queues = await _queueRepository.GetAllListAsync(x => x.FilePath.Equals(excelDispatchFile.URL) && x.FilePath.Equals(dispatchValidation.DispatchNo));
            if (queues.Count > 0)
            {
                foreach (var queue in queues) await _queueRepository.DeleteAsync(queue).ConfigureAwait(false);
            }

            foreach (var pair in dispatchFilePair)
            {
                var uuid = await GetFileUUIDByPath(pair.URL);
                await Repository.DeleteAsync(pair).ConfigureAwait(false);
                if (!uuid.Equals("")) await DeleteFile(uuid).ConfigureAwait(false);
            }

            var errorDetailsForDispatch = await Repository.GetAllListAsync(x => x.OriginalName.Equals(dispatchNo));
            foreach (var error in errorDetailsForDispatch)
            {
                var uuid = await GetFileUUIDByPath(error.URL);
                await Repository.DeleteAsync(error).ConfigureAwait(false);
                if (!uuid.Equals("")) await DeleteFile(uuid).ConfigureAwait(false);
            }

            await _dispatchValidationRepository.DeleteAsync(dispatchValidation).ConfigureAwait(false);

            return true;
        }

        public async Task<string> GetFileUUIDByPath(string path)
        {
            GetFilesDto files = await GetChibiFiles("50", "1");
            if (files.count > 50)
            {
                decimal unrounded = (decimal)files.count / 50;
                int pages = (int)Math.Ceiling(unrounded);
                for (int i = 2; i <= pages; i++)
                {
                    var files_paged = await GetChibiFiles("50", i.ToString());
                    if (files_paged.files.Count > 0) files.files.AddRange(files_paged.files);
                }
            }

            var file = files.files.FirstOrDefault(x => x.url.Equals(path));

            return file is null ? "" : file.uuid;
        }

        public async Task<IActionResult> GetItemTrackingIds(int applicationId)
        {
            var review = await _itemTrackingReviewsRepository.FirstOrDefaultAsync(x => x.ApplicationId.Equals(applicationId)) ?? throw new UserFriendlyException("Item Tracking Review Not Found!");

            var originalName = string.Format("{0}_{1}_{2}_{3}_{4}.xlsx", review.Prefix, review.PrefixNo, review.Suffix, review.TotalGiven, review.CustomerCode.Replace(" ", "_"));

            return await GetExcelByOriginalName(originalName);
        }

        public async Task<IActionResult> GetRateWeightBreakTemplate()
        {
            return await GetExcelByOriginalName("RateWeightBreakTemplate.xlsx");
        }

        public async Task<GetFilesDto> GetChibiFiles(string limit, string page)
        {
            var chibiKey = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var chibiURL = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", chibiKey.Value);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(chibiURL.Value + $"files?limit={limit}&page={page}"),
            };
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            GetFilesDto files = Newtonsoft.Json.JsonConvert.DeserializeObject<GetFilesDto>(body);

            return files;
        }

        public async Task<IActionResult> GetExcelByOriginalName(string originalName)
        {
            GetFilesDto files = await GetChibiFiles("50", "1");
            if (files.count > 50)
            {
                decimal unrounded = (decimal)files.count / 50;
                int pages = (int)Math.Ceiling(unrounded);
                for (int i = 2; i <= pages; i++)
                {
                    var files_paged = await GetChibiFiles("50", i.ToString());
                    if (files_paged.files.Count > 0) files.files.AddRange(files_paged.files);
                }
            }

            var found = files.files.FirstOrDefault(x => x.original.Equals(originalName));

            Stream fileStream = await GetFileStream(found.url);

            using MemoryStream ms = new();
            fileStream.CopyTo(ms);
            return new FileContentResult(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = originalName
            };
        }

        public async Task<bool> CreateTrackingFileForDispatches(List<DispatchInfo> dispatchTracking)
        {
            var dispatches = dispatchTracking.Where(di => di.DispatchCountries.Any(dc => dc.Select)).ToList();
            string json_dispatchTracking = Newtonsoft.Json.JsonConvert.SerializeObject(dispatches);

            string dispatchNo_concatinated = "";
            foreach (var dispatch in dispatches) dispatchNo_concatinated += dispatch.Dispatch + "_";

            string fileName = dispatchNo_concatinated + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".json";

            ChibiUploadDto chibiUpload = new()
            {
                fileName = fileName,
                fileType = "json",
                json = json_dispatchTracking
            };

            var jsonFile = await UploadFile(chibiUpload, fileName);

            await InsertFileToAlbum(jsonFile.uuid, false, true, false).ConfigureAwait(false);

            var existingQueuesByPath = await _queueRepository.GetAllListAsync(x => x.FilePath.Equals(jsonFile.url));

            foreach (var queue in existingQueuesByPath) await _queueRepository.DeleteAsync(queue);

            await _queueRepository.InsertAsync(new Queue()
            {
                EventType = "Update Dispatch Tracking",
                FilePath = jsonFile.url,
                DateCreated = DateTime.Now,
                Status = "New"
            }).ConfigureAwait(false);

            return true;
        }

        public async Task<bool> CreateInvoiceQueue(GenerateInvoice input)
        {
            string invoice_info = Newtonsoft.Json.JsonConvert.SerializeObject(input);

            string fileName = input.InvoiceNo + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".json";

            ChibiUploadDto chibiUpload = new()
            {
                fileName = fileName,
                fileType = "json",
                json = invoice_info
            };

            var jsonFile = await UploadFile(chibiUpload, fileName);

            await InsertFileToAlbum(jsonFile.uuid, false, true, false).ConfigureAwait(false);

            var existingQueuesByPath = await _queueRepository.GetAllListAsync(x => x.FilePath.Equals(jsonFile.url));

            foreach (var queue in existingQueuesByPath) await _queueRepository.DeleteAsync(queue);

            await _queueRepository.InsertAsync(new Queue()
            {
                EventType = "Invoice",
                FilePath = jsonFile.url,
                DateCreated = DateTime.Now,
                Status = "New"
            }).ConfigureAwait(false);

            return true;
        }

        [Consumes("multipart/form-data")]
        public async Task<bool> PreCheckRetryUpload([FromForm] PreCheckRetryDto uploadRetryPreCheck)
        {
            if (uploadRetryPreCheck.UploadFile != null)
            {
                var dispatchFile = await Repository.FirstOrDefaultAsync(x => x.URL.Equals(uploadRetryPreCheck.path));

                var dispatchFilePair = await Repository.GetAllListAsync(x => x.OriginalName.Equals(dispatchFile.OriginalName));
                var jsonDispatchFile = dispatchFilePair.FirstOrDefault(x => x.URL.Contains("json"));
                var fileProfile = jsonDispatchFile.URL;
                var fileString = await GetFileStreamAsString(fileProfile);

                DispatchProfileDto dispatchProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<DispatchProfileDto>(fileString);

                var update = IsUpdateParticulars(uploadRetryPreCheck.Details.PostalCode, uploadRetryPreCheck.Details.ServiceCode, uploadRetryPreCheck.Details.ProductCode, uploadRetryPreCheck.Details.RateOptionId);

                if (update)
                {
                    dispatchProfile.PostalCode = string.IsNullOrWhiteSpace(uploadRetryPreCheck.Details.PostalCode) ? dispatchProfile.PostalCode : uploadRetryPreCheck.Details.PostalCode;
                    dispatchProfile.ServiceCode = string.IsNullOrWhiteSpace(uploadRetryPreCheck.Details.ServiceCode) ? dispatchProfile.ServiceCode : uploadRetryPreCheck.Details.ServiceCode;
                    dispatchProfile.ProductCode = string.IsNullOrWhiteSpace(uploadRetryPreCheck.Details.ProductCode) ? dispatchProfile.ProductCode : uploadRetryPreCheck.Details.ProductCode;
                    dispatchProfile.RateOptionId = string.IsNullOrWhiteSpace(uploadRetryPreCheck.Details.RateOptionId) ? dispatchProfile.RateOptionId : uploadRetryPreCheck.Details.RateOptionId;

                    fileString = Newtonsoft.Json.JsonConvert.SerializeObject(dispatchProfile);
                }

                foreach (var pair in dispatchFilePair)
                {
                    var uuid = await GetFileUUIDByPath(pair.URL);
                    await Repository.DeleteAsync(pair).ConfigureAwait(false);
                    if (!uuid.Equals("")) await DeleteFile(uuid);
                }

                string uuidFileName = Guid.NewGuid().ToString();
                uploadRetryPreCheck.UploadFile.json = fileString;
                uploadRetryPreCheck.UploadFile.fileName = uuidFileName + ".xlsx";
                uploadRetryPreCheck.UploadFile.fileType = "xlsx";
                var xlsxFile = await UploadFile(uploadRetryPreCheck.UploadFile);

                uploadRetryPreCheck.UploadFile.fileName = uuidFileName + ".xlsx.profile.json";
                uploadRetryPreCheck.UploadFile.fileType = "json";
                var jsonFile = await UploadFile(uploadRetryPreCheck.UploadFile, xlsxFile.originalName);

                var deserializedFileString = Newtonsoft.Json.JsonConvert.DeserializeObject<PreCheckDetails>(fileString);

                await InsertFileToAlbum(xlsxFile.uuid, false, false, false, deserializedFileString.PostalCode, deserializedFileString.ServiceCode, deserializedFileString.ProductCode).ConfigureAwait(false);
                await InsertFileToAlbum(jsonFile.uuid, false, false, false, deserializedFileString.PostalCode, deserializedFileString.ServiceCode, deserializedFileString.ProductCode).ConfigureAwait(false);

                var queue = await _queueRepository.FirstOrDefaultAsync(x => (x.FilePath == uploadRetryPreCheck.path) && (x.EventType == "Validate Dispatch"));

                queue.Status = "New";
                queue.TookInSec = 0;
                queue.FilePath = xlsxFile.url;

                await _queueRepository.UpdateAsync(queue);
                await _queueRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);
            }
            else
            {
                var dispatchFile = await Repository.FirstOrDefaultAsync(x => x.URL.Equals(uploadRetryPreCheck.path));

                var dispatchFilePair = await Repository.GetAllListAsync(x => x.OriginalName.Equals(dispatchFile.OriginalName));
                var jsonDispatchFile = dispatchFilePair.FirstOrDefault(x => x.URL.Contains("json"));
                var excelDispatchFile = dispatchFilePair.FirstOrDefault(x => x.URL.Contains("xlsx"));
                var fileProfile = jsonDispatchFile.URL;
                var fileString = await GetFileStreamAsString(fileProfile);

                DispatchProfileDto dispatchProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<DispatchProfileDto>(fileString);

                var update = IsUpdateParticulars(uploadRetryPreCheck.Details.PostalCode, uploadRetryPreCheck.Details.ServiceCode, uploadRetryPreCheck.Details.ProductCode, uploadRetryPreCheck.Details.RateOptionId);

                if (update)
                {
                    dispatchProfile.PostalCode = string.IsNullOrWhiteSpace(uploadRetryPreCheck.Details.PostalCode) ? dispatchProfile.PostalCode : uploadRetryPreCheck.Details.PostalCode;
                    dispatchProfile.ServiceCode = string.IsNullOrWhiteSpace(uploadRetryPreCheck.Details.ServiceCode) ? dispatchProfile.ServiceCode : uploadRetryPreCheck.Details.ServiceCode;
                    dispatchProfile.ProductCode = string.IsNullOrWhiteSpace(uploadRetryPreCheck.Details.ProductCode) ? dispatchProfile.ProductCode : uploadRetryPreCheck.Details.ProductCode;
                    dispatchProfile.RateOptionId = string.IsNullOrWhiteSpace(uploadRetryPreCheck.Details.RateOptionId) ? dispatchProfile.RateOptionId : uploadRetryPreCheck.Details.RateOptionId;

                    fileString = Newtonsoft.Json.JsonConvert.SerializeObject(dispatchProfile);
                }

                await Repository.DeleteAsync(jsonDispatchFile).ConfigureAwait(false);

                uploadRetryPreCheck.UploadFile = new()
                {
                    fileName = excelDispatchFile.UUID + ".xlsx.profile.json",
                    fileType = "json",
                    json = fileString
                };
                var jsonFile = await UploadFile(uploadRetryPreCheck.UploadFile, excelDispatchFile.OriginalName);

                var deserializedFileString = Newtonsoft.Json.JsonConvert.DeserializeObject<PreCheckDetails>(fileString);

                await InsertFileToAlbum(jsonFile.uuid, false, false, false, deserializedFileString.PostalCode, deserializedFileString.ServiceCode, deserializedFileString.ProductCode).ConfigureAwait(false);

                var queue = await _queueRepository.FirstOrDefaultAsync(x => (x.FilePath == uploadRetryPreCheck.path) && (x.EventType == "Validate Dispatch"));

                queue.Status = "New";
                queue.TookInSec = 0;

                await _queueRepository.UpdateAsync(queue);
                await _queueRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);
            }

            if (uploadRetryPreCheck.dispatchNo is not null)
            {
                var errorDetailsForDispatch = await Repository.GetAllListAsync(x => x.OriginalName.Equals(uploadRetryPreCheck.dispatchNo));
                foreach (var error in errorDetailsForDispatch)
                {
                    var uuid = await GetFileUUIDByPath(error.URL);
                    await Repository.DeleteAsync(error).ConfigureAwait(false);
                    if (!uuid.Equals("")) await DeleteFile(uuid).ConfigureAwait(false);
                }
            }

            return true;
        }

        [Consumes("multipart/form-data")]
        public async Task<ChibiUpload> UploadFile([FromForm] ChibiUploadDto uploadFile, string originalName = null)
        {
            var chibiKey = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var chibiURL = await _applicationSettingRepository.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", chibiKey.Value);
            var formData = new MultipartFormDataContent();

            switch (uploadFile.fileType)
            {
                case "xlsx":
                    var xlsxContent = new StreamContent(uploadFile.file.OpenReadStream());
                    xlsxContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    formData.Add(xlsxContent, "file", uploadFile.fileName);
                    break;
                case "json":
                    var jsonContent = new StringContent(uploadFile.json);
                    jsonContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    formData.Add(jsonContent, "file", uploadFile.fileName);
                    break;
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(chibiURL.Value + "upload"),
                Content = formData,
            };

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ChibiUpload>(body);

            if (result != null)
            {
                if (uploadFile.fileType == "xlsx")
                {
                    string dispatchNo = await IsExcelFileUploadedBefore(result.name);
                    if (dispatchNo != "") throw new UserFriendlyException($"This excel has been uploaded before with DispatchNo: {dispatchNo}");
                }

                result.originalName = originalName is null ? uploadFile.file.FileName.Replace(".xlsx", "") + $"_{result.name}" : originalName;

                var existingChibis = await Repository.GetAllListAsync(x => x.URL.Equals(result.url));

                foreach (var chibi in existingChibis) await Repository.DeleteAsync(chibi);

                await Repository.InsertAsync(new Chibi()
                {
                    FileName = result.name == null ? "" : DateTime.Now.ToString("yyyyMMdd") + "_" + result.name,
                    UUID = result.uuid ?? "",
                    URL = result.url ?? "",
                    OriginalName = originalName is null ? result.originalName : originalName,
                    GeneratedName = result.name ?? ""
                });

                await Repository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

            }

            return result;
        }

        [Consumes("multipart/form-data")]
        public async Task<bool> PreCheckUpload([FromForm] PreCheckDto uploadPreCheck)
        {
            var dispatch = await _dispatchRepository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(uploadPreCheck.Details.DispatchNo));

            if (dispatch is null)
            {
                string uuidFileName = Guid.NewGuid().ToString();
                uploadPreCheck.UploadFile.json = Newtonsoft.Json.JsonConvert.SerializeObject(uploadPreCheck.Details);
                uploadPreCheck.UploadFile.fileName = uuidFileName + ".xlsx";
                uploadPreCheck.UploadFile.fileType = "xlsx";
                var xlsxFile = await UploadFile(uploadPreCheck.UploadFile);

                uploadPreCheck.UploadFile.fileName = uuidFileName + ".xlsx.profile.json";
                uploadPreCheck.UploadFile.fileType = "json";
                var jsonFile = await UploadFile(uploadPreCheck.UploadFile, xlsxFile.originalName);

                await InsertFileToAlbum(xlsxFile.uuid, false, false, false, uploadPreCheck.Details.PostalCode, uploadPreCheck.Details.ServiceCode, uploadPreCheck.Details.ProductCode).ConfigureAwait(false);
                await InsertFileToAlbum(jsonFile.uuid, false, false, false, uploadPreCheck.Details.PostalCode, uploadPreCheck.Details.ServiceCode, uploadPreCheck.Details.ProductCode).ConfigureAwait(false);

                var existingQueuesByPath = await _queueRepository.GetAllListAsync(x => x.FilePath.Equals(xlsxFile.url));

                foreach (var queue in existingQueuesByPath) await _queueRepository.DeleteAsync(queue);

                await _queueRepository.InsertAsync(new Queue()
                {
                    EventType = "Validate Dispatch",
                    FilePath = xlsxFile.url,
                    DateCreated = DateTime.Now,
                    Status = "New"
                }).ConfigureAwait(false);

                return true;
            }
            else throw new UserFriendlyException("Dispatch Number already exists.");
        }

        private async Task<string> IsExcelFileUploadedBefore(string name)
        {
            var xlsxFile = await Repository.FirstOrDefaultAsync(x => x.GeneratedName.Equals(name));

            if (xlsxFile is null) return "";

            var dispatchValidation = await _dispatchValidationRepository.FirstOrDefaultAsync(x => x.FilePath.Equals(xlsxFile.URL));

            return dispatchValidation.DispatchNo;
        }
    }
}
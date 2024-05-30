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

namespace SND.SMP.Chibis
{
    public class ChibiAppService(
        IRepository<Chibi, long> repository,
        IRepository<Queue, long> queueRepository,
        IRepository<ApplicationSetting, int> applicationSettingRepository,
        IRepository<DispatchValidation, string> dispatchValidationRepository,
        IMemoryCache memoryCache
    ) : AsyncCrudAppService<Chibi, ChibiDto, long, PagedChibiResultRequestDto>(repository)
    {
        private readonly IRepository<Queue, long> _queueRepository = queueRepository;
        private readonly IRepository<ApplicationSetting, int> _applicationSettingRepository = applicationSettingRepository;
        private readonly IRepository<DispatchValidation, string> _dispatchValidationRepository = dispatchValidationRepository;
        private readonly IMemoryCache _memoryCache = memoryCache;
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

        private async Task<string> GetFileStreamAsString(string url)
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

        private async Task<bool> InsertFileToAlbum(string file_uuid, bool isError, string postalCode = null, string serviceCode = null, string productCode = null)
        {
            List<Album> albums = await GetDictAlbums();
            if (isError)
            {
                if (albums.Count == 0)
                {
                    var album = await CreateAlbumAsync("ErrorDetails");
                    await AddFileToAlbum(album.album.uuid, file_uuid);
                }
                else
                {
                    var error_album = albums.FirstOrDefault(a => a.name == "ErrorDetails");
                    if (error_album == null)
                    {
                        var album = await CreateAlbumAsync("ErrorDetails");
                        await AddFileToAlbum(album.album.uuid, file_uuid);
                    }
                    else await AddFileToAlbum(error_album.uuid, file_uuid);
                }
            }
            else
            {
                if (albums.Count == 0)
                {
                    if (postalCode != null) await CreateInsertPostalAlbum(postalCode[..2], file_uuid);
                    if (serviceCode != null) await CreateInsertServiceAlbum(serviceCode, file_uuid);
                    if (productCode != null) await CreateInsertProductAlbum(productCode, file_uuid);
                }
                else
                {
                    if (postalCode != null)
                    {
                        var postal_album = albums.FirstOrDefault(a => a.name == "Postal_" + postalCode[..2]);
                        if (postal_album == null) await CreateInsertPostalAlbum(postalCode[..2], file_uuid);
                        else await AddFileToAlbum(postal_album.uuid, file_uuid);
                    }
                    if (serviceCode != null)
                    {
                        var service_album = albums.FirstOrDefault(a => a.name == "Service_" + serviceCode);
                        if (service_album == null) await CreateInsertServiceAlbum(serviceCode, file_uuid);
                        else await AddFileToAlbum(service_album.uuid, file_uuid);
                    }
                    if (productCode != null)
                    {
                        var product_album = albums.FirstOrDefault(a => a.name == "Product_" + productCode);
                        if (product_album == null) await CreateInsertPostalAlbum(productCode, file_uuid);
                        else await AddFileToAlbum(product_album.uuid, file_uuid);
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
                Content = new StringContent("{'name': '" + name + "'}"),
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

        [Consumes("multipart/form-data")]
        public async Task<bool> PreCheckUpload([FromForm] PreCheckDto uploadPreCheck)
        {
            string uuidFileName = Guid.NewGuid().ToString();
            uploadPreCheck.UploadFile.json = Newtonsoft.Json.JsonConvert.SerializeObject(uploadPreCheck.Details);
            uploadPreCheck.UploadFile.fileName = uuidFileName + ".xlsx";
            uploadPreCheck.UploadFile.fileType = "xlsx";
            var xlsxFile = await UploadFile(uploadPreCheck.UploadFile);

            uploadPreCheck.UploadFile.fileName = uuidFileName + ".xlsx.profile.json";
            uploadPreCheck.UploadFile.fileType = "json";
            var jsonFile = await UploadFile(uploadPreCheck.UploadFile, xlsxFile.originalName);

            await InsertFileToAlbum(xlsxFile.uuid, false, uploadPreCheck.Details.PostalCode, uploadPreCheck.Details.ServiceCode, uploadPreCheck.Details.ProductCode);
            await InsertFileToAlbum(jsonFile.uuid, false, uploadPreCheck.Details.PostalCode, uploadPreCheck.Details.ServiceCode, uploadPreCheck.Details.ProductCode);

            await _queueRepository.InsertAsync(new Queue()
            {
                EventType = "Validate Dispatch",
                FilePath = xlsxFile.url,
                DateCreated = DateTime.Now,
                Status = "New"
            });

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

                foreach (var pair in dispatchFilePair)
                {
                    await Repository.DeleteAsync(pair);
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

                await InsertFileToAlbum(xlsxFile.uuid, false, deserializedFileString.PostalCode, deserializedFileString.ServiceCode, deserializedFileString.ProductCode);
                await InsertFileToAlbum(jsonFile.uuid, false, deserializedFileString.PostalCode, deserializedFileString.ServiceCode, deserializedFileString.ProductCode);

                var queue = await _queueRepository.FirstOrDefaultAsync(x => (x.FilePath == uploadRetryPreCheck.path) && (x.EventType == "Validate Dispatch"));

                queue.Status = "New";
                queue.TookInSec = 0;
                queue.FilePath = xlsxFile.url;

                await _queueRepository.UpdateAsync(queue);
                await _queueRepository.GetDbContext().SaveChangesAsync();

                if (uploadRetryPreCheck.dispatchNo is not null)
                {
                    var errorDetailsForDispatch = await Repository.GetAllListAsync(x => x.OriginalName.Equals(uploadRetryPreCheck.dispatchNo));
                    foreach (var error in errorDetailsForDispatch) await Repository.DeleteAsync(error);
                }

                return true;
            }
            else
            {
                var queue = await _queueRepository.FirstOrDefaultAsync(x => (x.FilePath == uploadRetryPreCheck.path) && (x.EventType == "Validate Dispatch"));

                queue.Status = "New";
                queue.TookInSec = 0;

                await _queueRepository.UpdateAsync(queue);
                await _queueRepository.GetDbContext().SaveChangesAsync();

                if (uploadRetryPreCheck.dispatchNo is not null)
                {
                    var errorDetailsForDispatch = await Repository.GetAllListAsync(x => x.OriginalName.Equals(uploadRetryPreCheck.dispatchNo));
                    foreach (var error in errorDetailsForDispatch) await Repository.DeleteAsync(error);
                }

                return true;
            }
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
                result.originalName = uploadFile.file.FileName.Replace(".xlsx", "") + $"_{result.name}";
                //Insert to DB
                Chibi entity = new()
                {
                    FileName = result.name == null ? "" : DateTime.Now.ToString("yyyyMMdd") + "_" + result.name,
                    UUID = result.uuid ?? "",
                    URL = result.url ?? "",
                    OriginalName = originalName is null ? result.originalName : originalName,
                    GeneratedName = result.name ?? ""
                };

                await Repository.InsertAsync(entity);
                await Repository.GetDbContext().SaveChangesAsync();


            }

            return result;
        }


    }
}
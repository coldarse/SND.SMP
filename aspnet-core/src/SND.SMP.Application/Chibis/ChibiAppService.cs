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

namespace SND.SMP.Chibis
{
    public class ChibiAppService(
        IRepository<Chibi, long> repository,
        IRepository<Queue, long> queueRepository,
        IRepository<ApplicationSetting, int> applicationSettingRepository,
        IRepository<DispatchValidation, string> dispatchValidationRepository
    ) : AsyncCrudAppService<Chibi, ChibiDto, long, PagedChibiResultRequestDto>(repository)
    {
        private readonly IRepository<Queue, long> _queueRepository = queueRepository;
        private readonly IRepository<ApplicationSetting, int> _applicationSettingRepository = applicationSettingRepository;
        private readonly IRepository<DispatchValidation, string> _dispatchValidationRepository = dispatchValidationRepository;

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

        public class DispatchProfileDto
        {
            public string DispatchNo { get; set; } = "";
            public string AccNo { get; set; } = "";
            public string PostalCode { get; set; } = "";
            public string ServiceCode { get; set; } = "";
            public string ProductCode { get; set; } = "";
            public DateOnly DateDispatch { get; set; }
            public string RateOptionId { get; set; } = "";
            public string PaymentMode { get; set; }
            public bool IsValid { get; set; }
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
            await UploadFile(uploadPreCheck.UploadFile, xlsxFile.originalName);

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

                string uuidFileName = Guid.NewGuid().ToString();
                uploadRetryPreCheck.UploadFile.json = fileString;
                uploadRetryPreCheck.UploadFile.fileName = uuidFileName + ".xlsx";
                uploadRetryPreCheck.UploadFile.fileType = "xlsx";
                var xlsxFile = await UploadFile(uploadRetryPreCheck.UploadFile);

                uploadRetryPreCheck.UploadFile.fileName = uuidFileName + ".xlsx.profile.json";
                uploadRetryPreCheck.UploadFile.fileType = "json";
                await UploadFile(uploadRetryPreCheck.UploadFile, xlsxFile.originalName);

                var queue = await _queueRepository.FirstOrDefaultAsync(x => (x.FilePath == uploadRetryPreCheck.path) && (x.EventType == "Validate Dispatch"));

                queue.Status = "New";
                queue.TookInSec = 0;
                queue.FilePath = xlsxFile.url;

                await _queueRepository.UpdateAsync(queue);
                await _queueRepository.GetDbContext().SaveChangesAsync();

                return true;
            }
            else
            {
                var queue = await _queueRepository.FirstOrDefaultAsync(x => (x.FilePath == uploadRetryPreCheck.path) && (x.EventType == "Validate Dispatch"));

                queue.Status = "New";
                queue.TookInSec = 0;

                await _queueRepository.UpdateAsync(queue);
                await _queueRepository.GetDbContext().SaveChangesAsync();

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
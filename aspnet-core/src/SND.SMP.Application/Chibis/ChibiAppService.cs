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

namespace SND.SMP.Chibis
{
    public class ChibiAppService(IRepository<Chibi, long> repository, IConfiguration configuration, IRepository<Queue, long> queueRepository) : AsyncCrudAppService<Chibi, ChibiDto, long, PagedChibiResultRequestDto>(repository)
    {
        private readonly IRepository<Queue, long> _queueRepository = queueRepository;
        private readonly IConfiguration _configuration = configuration;

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

        //----- For Testing Purposes!!! -----//
        public async Task<bool> Trial()
        {
            var file = await GetFile("d7514df1-50d0-4b65-88fb-ca48607d5012");

            DataSet dataSet = new();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var httpClient = new HttpClient();
            try
            {
                using var response = await httpClient.GetAsync(file.file.url);
                if (response.IsSuccessStatusCode)
                {
                    using var contentStream = await response.Content.ReadAsStreamAsync();
                    using var package = new ExcelPackage(contentStream);
                    var worksheets = package.Workbook.Worksheets;

                    foreach (ExcelWorksheet ws in worksheets)
                    {
                        DataTable dataTable = new();
                        dataTable = ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].ToDataTable(c =>
                        {
                            c.FirstRowIsColumnNames = false;
                        });
                        dataTable.TableName = ws.Name;
                        dataSet.Tables.Add(dataTable);
                    }

                    var tables = dataSet.Tables;

                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to download file. Status code: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file: {ex.Message}");
                return false;
            }
        }
        public async Task<DispatchProfileDto> Trial2()
        {
            var file = await GetFile("d7514df1-50d0-4b65-88fb-ca48607d5012");

            DataSet dataSet = new();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var httpClient = new HttpClient();
            try
            {
                using var response = await httpClient.GetAsync(file.file.url);
                if (response.IsSuccessStatusCode)
                {
                    string contentString = await response.Content.ReadAsStringAsync();
                    var _dispatchProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<DispatchProfileDto>(contentString);

                    return _dispatchProfile;
                }
                else
                {
                    Console.WriteLine($"Failed to download file. Status code: {response.StatusCode}");
                    return new DispatchProfileDto();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file: {ex.Message}");
                return new DispatchProfileDto();
            }
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



        public async Task<GetFileDto> GetFile(string uuid)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", _configuration["Authentication:ChibiAPIKey"]);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_configuration["App:ChibiURL"] + $"file/{uuid}"),
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
            await UploadFile(uploadPreCheck.UploadFile);

            await _queueRepository.InsertAsync(new Queue()
            {
                EventType = "Upload Dispatch",
                FilePath = xlsxFile.url,
                DateCreated = DateTime.Now,
                Status = "Draft"
            });

            return true;
        }

        [Consumes("multipart/form-data")]
        public async Task<ChibiUpload> UploadFile([FromForm] ChibiUploadDto uploadFile)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", _configuration["Authentication:ChibiAPIKey"]);
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
                RequestUri = new Uri(_configuration["App:ChibiURL"] + "upload"),
                Content = formData,
            };

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ChibiUpload>(body);

            if (result != null)
            {
                //Insert to DB
                Chibi entity = new()
                {
                    FileName = result.name == null ? "" : DateTime.Now.ToString("yyyyMMdd") + "_" + result.name,
                    UUID = result.uuid ?? "",
                    URL = result.url ?? "",
                    OriginalName = uploadFile.file.FileName ?? "",
                    GeneratedName = result.name ?? ""
                };

                await Repository.InsertAsync(entity);
                await Repository.GetDbContext().SaveChangesAsync();
            }

            return result;
        }
    }
}
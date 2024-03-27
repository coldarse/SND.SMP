using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.Chibis.Dto;
using SND.SMP.FileUploadAPI.Dto;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using SND.SMP.CustomerPostals;
using Abp.EntityFrameworkCore.Repositories;
using Microsoft.Extensions.Configuration;
using SND.SMP.Queues;

namespace SND.SMP.Chibis
{
    public class ChibiAppService : AsyncCrudAppService<Chibi, ChibiDto, long, PagedChibiResultRequestDto>
    {
        private readonly IRepository<Queue, long> _queueRepository;
        private IConfiguration _configuration;
        public ChibiAppService(IRepository<Chibi, long> repository, IConfiguration configuration, IRepository<Queue, long> queueRepository) : base(repository)
        {
            _configuration = configuration;
            _queueRepository = queueRepository;
        }
        protected override IQueryable<Chibi> CreateFilteredQuery(PagedChibiResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.FileName.Contains(input.Keyword) ||
                    x.UUID.Contains(input.Keyword) ||
                    x.URL.Contains(input.Keyword) ||
                    x.OriginalName.Contains(input.Keyword) ||
                    x.GeneratedName.Contains(input.Keyword)).AsQueryable();
        }

        public async Task<GetFileDto> GetFile(string uuid)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", _configuration["App:ChibiAPIKey"]);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://localhost:24424/api/file/{uuid}"),
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

            var queue = await _queueRepository.InsertAsync(new Queue()
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
            client.DefaultRequestHeaders.Add("x-api-key", _configuration["App:ChibiAPIKey"]);
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
                RequestUri = new Uri("http://localhost:24424/api/upload"),
                Content = formData,
            };

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ChibiUpload>(body);

            if (result != null)
            {
                //Insert to DB
                Chibi entity = new Chibi()
                {
                    FileName = result.name == null ? "" : DateTime.Now.ToString("yyyyMMdd") + "_" + result.name,
                    UUID = result.uuid == null ? "" : result.uuid,
                    URL = result.url == null ? "" : result.url,
                    OriginalName = uploadFile.file.FileName == null ? "" : uploadFile.file.FileName,
                    GeneratedName = result.name == null ? "" : result.name
                };

                await Repository.InsertAsync(entity);
                await Repository.GetDbContext().SaveChangesAsync();
            }

            return JsonSerializer.Deserialize<ChibiUpload>(body);
        }
    }
}
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


namespace SND.SMP.Chibis
{
    public class ChibiAppService : AsyncCrudAppService<Chibi, ChibiDto, long, PagedChibiResultRequestDto>
    {
        public ChibiAppService(IRepository<Chibi, long> repository) : base(repository)
        {
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
            client.DefaultRequestHeaders.Add("x-api-key", "3uaIW5sLU30RCNvoyL1aRvMDLnbIskgCgxIlLDPsoa7yrcISj5V3VtuOLCQG5joA");
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
        public async Task<ChibiUpload> UploadFile([FromForm] ChibiUploadDto uploadFile)
        {

            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", "3uaIW5sLU30RCNvoyL1aRvMDLnbIskgCgxIlLDPsoa7yrcISj5V3VtuOLCQG5joA");

            var formData = new MultipartFormDataContent();
            var fileContent = new StreamContent(uploadFile.file.OpenReadStream());
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            formData.Add(fileContent, "file", uploadFile.file.FileName);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("http://localhost:24424/api/upload"),
                Content = formData,
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(body);

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

                    var created = await Repository.InsertAsync(entity);
                    await Repository.GetDbContext().SaveChangesAsync();
                }

                return JsonSerializer.Deserialize<ChibiUpload>(body);
            }
        }
    }
}
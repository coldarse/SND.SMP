using SND.SMP.FileUploadAPI.Dto;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SND.SMP.FileUploadAPI
{
    public class FileUploadAppService
    {


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

    }
}

using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SND.SMP.Chibis;
using SND.SMP.DispatchTrackingUpdater.EF;

namespace SND.SMP.DispatchTrackingUpdater
{
    public static class FileServer
    {
        public static async Task<Stream> GetFileStream(string url)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = Timeout.InfiniteTimeSpan;
            using var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var contentByteArray = await response.Content.ReadAsByteArrayAsync();
                return new MemoryStream(contentByteArray);
            }
            return null;
        }

        public static async Task<string> GetFileStreamAsString(string url)
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

        private static async Task<List<Album>> GetAlbumsAsync(db dbconn)
        {
            var chibiKey = await dbconn.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var chibiURL = await dbconn.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));
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

        private static async Task<CreateAlbumDto> CreateAlbumAsync(string name, db dbconn)
        {
            var chibiKey = await dbconn.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var chibiURL = await dbconn.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));
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

        private static async Task<bool> AddFileToAlbum(string album_uuid, string file_uuid, db dbconn)
        {
            var chibiKey = await dbconn.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var chibiURL = await dbconn.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", chibiKey.Value);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(chibiURL.Value + $"file/{file_uuid}/album/{album_uuid}"),
            };

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode) return true;

            return false;
        }

        private static async Task<bool> CreateInsertPostalAlbum(string postalCode, string file_uuid, db dbconn)
        {
            var album = await CreateAlbumAsync("Postal_" + postalCode, dbconn);
            var addFileToAlbum = await AddFileToAlbum(album.album.uuid, file_uuid, dbconn);
            return true;
        }

        private static async Task<bool> CreateInsertServiceAlbum(string serviceCode, string file_uuid, db dbconn)
        {
            var album = await CreateAlbumAsync("Service_" + serviceCode, dbconn);
            var addFileToAlbum = await AddFileToAlbum(album.album.uuid, file_uuid, dbconn);
            return true;
        }

        private static async Task<bool> CreateInsertProductAlbum(string productCode, string file_uuid, db dbconn)
        {
            var album = await CreateAlbumAsync("Product_" + productCode, dbconn);
            var addFileToAlbum = await AddFileToAlbum(album.album.uuid, file_uuid, dbconn);
            return true;
        }

        public static async Task<bool> DeleteFile(string uuid)
        {
            using db dbconn = new();
            var obj_ChibiKey = await dbconn.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var obj_ChibiURL = await dbconn.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", obj_ChibiKey.Value);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(obj_ChibiURL.Value + $"file/{uuid}"),
            };

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return true;
        }


        public static async Task<ChibiUpload> InsertExcelFileToChibi(Stream excel, string fileName, string originalName = null, string postalCode = null, string productCode = null)
        {
            using db dbconn = new();
            var obj_ChibiKey = await dbconn.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("ChibiKey"));
            var obj_ChibiURL = await dbconn.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("ChibiURL"));

            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", obj_ChibiKey.Value);
            var formData = new MultipartFormDataContent();

            var xlsxContent = new StreamContent(excel);
            xlsxContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            formData.Add(xlsxContent, "file", fileName);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(obj_ChibiURL.Value + "upload"),
                Content = formData,
            };

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ChibiUpload>(body);

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

                await dbconn.Chibis.AddAsync(entity).ConfigureAwait(false);
                await dbconn.SaveChangesAsync();

                await FileServer.InsertFileToAlbum(result.uuid, false, dbconn, postalCode, null, productCode);
            }

            return result;
        }

        public static async Task<bool> InsertFileToAlbum(string file_uuid, bool isError, db dbconn, string postalCode = null, string serviceCode = null, string productCode = null, bool isInvoice = false)
        {
            List<Album> albums = await GetAlbumsAsync(dbconn);
            if (isError)
            {
                if (albums.Count == 0)
                {
                    var album = await CreateAlbumAsync("ErrorDetails", dbconn);
                    await AddFileToAlbum(album.album.uuid, file_uuid, dbconn).ConfigureAwait(false);
                }
                else
                {
                    var error_album = albums.FirstOrDefault(a => a.name == "ErrorDetails");
                    if (error_album == null)
                    {
                        var album = await CreateAlbumAsync("ErrorDetails", dbconn);
                        await AddFileToAlbum(album.album.uuid, file_uuid, dbconn).ConfigureAwait(false);
                    }
                    else await AddFileToAlbum(error_album.uuid, file_uuid, dbconn);
                }
                return true;
            }
            else if (isInvoice)
            {
                if (albums.Count == 0)
                {
                    var album = await CreateAlbumAsync("Invoices", dbconn);
                    await AddFileToAlbum(album.album.uuid, file_uuid, dbconn).ConfigureAwait(false);
                }
                else
                {
                    var invoice_album = albums.FirstOrDefault(a => a.name == "Invoices");
                    if (invoice_album == null)
                    {
                        var album = await CreateAlbumAsync("Invoices", dbconn);
                        await AddFileToAlbum(album.album.uuid, file_uuid, dbconn).ConfigureAwait(false);
                    }
                    else await AddFileToAlbum(invoice_album.uuid, file_uuid, dbconn).ConfigureAwait(false);
                }
            }
            else
            {
                if (postalCode != null)
                {
                    var postal_album = albums.FirstOrDefault(a => a.name == "Postal_" + postalCode[..2]);
                    if (postal_album == null) await CreateInsertPostalAlbum(postalCode[..2], file_uuid, dbconn).ConfigureAwait(false);
                    else await AddFileToAlbum(postal_album.uuid, file_uuid, dbconn).ConfigureAwait(false);
                }
                if (serviceCode != null)
                {
                    var service_album = albums.FirstOrDefault(a => a.name == "Service_" + serviceCode);
                    if (service_album == null) await CreateInsertServiceAlbum(serviceCode, file_uuid, dbconn).ConfigureAwait(false);
                    else await AddFileToAlbum(service_album.uuid, file_uuid, dbconn).ConfigureAwait(false);
                }
                if (productCode != null)
                {
                    var product_album = albums.FirstOrDefault(a => a.name == "Product_" + productCode);
                    if (product_album == null) await CreateInsertProductAlbum(productCode, file_uuid, dbconn).ConfigureAwait(false);
                    else await AddFileToAlbum(product_album.uuid, file_uuid, dbconn).ConfigureAwait(false);
                }
            }
            return true;
        }


    }
}
using System.Drawing.Text;
using System.Net.Http.Headers;
using JetBrains.Annotations;
using OfficeOpenXml.ExternalReferences;
using SND.SMP.Chibis;
using SND.SMP.DispatchConsole;
using SND.SMP.DispatchConsole.EF;
using static SND.SMP.Shared.EnumConst;

public class InvoiceGenerator
{

    private bool isQueue { get; set; }
    private string _filePath { get; set; }
    private uint QueueId { get; set; }
    private GenerateInvoice invoice_info { get; set; }

    private readonly ILogger<Worker> _logger;

    public class QueueErrorEventArg : EventArgs
    {
        public string FilePath { get; set; }
        public string ErrorMsg { get; set; }
    }
    public InvoiceGenerator(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    public async Task DiscoverAndGenerate()
    {
        using db db = new();
        var hasGenerating = db.Queues
                .Where(x => x.EventType == QueueEnumConst.EVENT_TYPE_INVOICE)
                .Where(x => x.Status == QueueEnumConst.STATUS_GENERATING)
                .Any();

        if (hasGenerating) return;

        var invoice = db.Queues
                .Where(x => x.EventType == QueueEnumConst.EVENT_TYPE_INVOICE)
                .Where(x => x.Status == QueueEnumConst.STATUS_NEW)
                .OrderBy(x => x.DateCreated)
                .FirstOrDefault();

        isQueue = false;
        if (invoice is not null)
        {
            isQueue = true;
            QueueId = invoice.Id;
            _filePath = invoice.FilePath;

            invoice.Status = QueueEnumConst.STATUS_GENERATING;
            await db.SaveChangesAsync().ConfigureAwait(false);

            _logger.LogInformation("Updated Status for Queue");
        }

        if (!string.IsNullOrWhiteSpace(_filePath))
        {
            using db chibiDB = new();
            var updateFile = chibiDB.Chibis.FirstOrDefault(x => x.URL.Equals(_filePath));
            if (updateFile is not null)
            {
                var fileString = await FileServer.GetFileStreamAsString(updateFile.URL);

                if (fileString is not null)
                {
                    invoice_info = Newtonsoft.Json.JsonConvert.DeserializeObject<GenerateInvoice>(fileString);

                    if (invoice_info != null)
                    {
                        _logger.LogInformation("Started Generating");
                        await GenerateInvoicePDF(invoice_info);
                    }
                }
            }
        }
    }

    private async Task GenerateInvoicePDF(GenerateInvoice invoice_info)
    {
        try
        {
            DateTime dateStart = DateTime.Now;
            using db db = new();
            var dispatches = db.Dispatches.Where(x => invoice_info.Dispatches.Contains(x.DispatchNo)).ToList();
            _logger.LogInformation("Retrieved Dispatches");
            var dispatches_id = dispatches.Select(x => x.Id).ToList();
            _logger.LogInformation("Got List of Dispatches");
            var items = db.Items.Where(x => dispatches_id.Contains((uint)x.DispatchId)).ToList();
            _logger.LogInformation("Retrieved Items");
            var item_trackings_by_dispatch = db.ItemTrackings.Where(x => invoice_info.Dispatches.Contains(x.DispatchNo)).ToList();
            _logger.LogInformation("Retrived Item Trackings By Dispatch");
            var distincted_item_trackings = item_trackings_by_dispatch.DistinctBy(x => x.DispatchNo).ToList();
            _logger.LogInformation("Got Distincted Item Trackings");
            //Group by Currency
            var dispatches_grouped_by_currency = dispatches
                .Where(d => !string.IsNullOrEmpty(d.CurrencyId)) // Ensuring CurrencyId is not null or empty
                .GroupBy(d => d.CurrencyId)
                .ToList();

            _logger.LogInformation("Grouped Dispatches By Currency");

            List<ItemsByCurrency> items_by_currency = [];
            foreach (var group in dispatches_grouped_by_currency)
            {
                ItemsByCurrency tempGroup = new()
                {
                    Currency = group.Key,
                    Items = [],
                    TotalAmount = 0.00m
                };
                foreach (var dispatch in group)
                {
                    var item_tracking = distincted_item_trackings.FirstOrDefault(x => x.DispatchId == dispatch.Id);
                    var dispatch_items = items.Where(x => x.DispatchId == dispatch.Id).ToList();

                    decimal ratePerKG = 0.00m;
                    decimal unitPrice = 0.00m;

                    var bags = db.Bags.Where(x => x.DispatchId == dispatch.Id).ToList();

                    if (item_tracking.IsExternal)
                    {
                        tempGroup.Items.AddRange(dispatch_items.Select(x =>
                        {
                            if (dispatch.ServiceCode == "TS")
                            {
                                // TS Rates
                                var rate_items = db.Rateitems.Where(x => x.ProductCode == dispatch.ProductCode).ToList();

                                return new SimplifiedItem()
                                {
                                    DispatchNo = dispatch.DispatchNo,
                                    Weight = (decimal)x.Weight,
                                    Country = x.CountryCode,
                                    Identifier = x.Id,
                                    Rate = (decimal)(rate_items.FirstOrDefault(z => z.CountryCode == x.CountryCode) is null ? 0.00m : rate_items.FirstOrDefault(z => z.CountryCode == x.CountryCode).Total),
                                    Quantity = 1,
                                    UnitPrice = (decimal)(rate_items.FirstOrDefault(z => z.CountryCode == x.CountryCode) is null ? 0.00m : rate_items.FirstOrDefault(z => z.CountryCode == x.CountryCode).Fee),
                                    Amount = (decimal)x.Price,
                                    ProductCode = x.ProductCode
                                };
                            }
                            else
                            {
                                // DE Rates
                                return new SimplifiedItem()
                                {
                                    DispatchNo = dispatch.DispatchNo,
                                    Weight = (decimal)x.Weight,
                                    Country = x.CountryCode,
                                    Identifier = x.Id,
                                    Rate = ratePerKG,
                                    Quantity = 1,
                                    UnitPrice = unitPrice,
                                    Amount = (decimal)x.Price,
                                    ProductCode = x.ProductCode
                                };
                            }
                        }));
                    }
                    else
                    {
                        tempGroup.Items.AddRange(dispatch_items.GroupBy(x => x.BagNo).Select(y =>
                        {
                            var country_code = y.First().CountryCode;
                            var under_amount = bags.FirstOrDefault(x => x.BagNo == y.Key).UnderAmount ?? 0.00m;
                            var weight_variance = bags.FirstOrDefault(x => x.BagNo == y.Key).WeightVariance ?? 0.00m;
                            var bag_country_code = bags.FirstOrDefault(x => x.BagNo == y.Key).CountryCode ?? "";

                            if (!string.IsNullOrWhiteSpace(bag_country_code))
                            {
                                if (dispatch.ServiceCode == "TS")
                                {
                                    // TS Rates
                                    var rate_items = db.Rateitems.FirstOrDefault(x => x.ProductCode == dispatch.ProductCode &&
                                                                             x.CountryCode == bag_country_code);

                                    ratePerKG = (decimal)rate_items.Total;
                                    unitPrice = (decimal)rate_items.Fee;
                                }
                                else
                                {
                                    // DE Rates
                                }
                            }


                            return new SimplifiedItem()
                            {
                                DispatchNo = dispatch.DispatchNo,
                                Weight = (decimal)(y.Sum(i => i.Weight) + weight_variance),
                                Country = country_code,
                                Identifier = under_amount == 0.00m ? y.Key : y.Key + " +" + under_amount,
                                Rate = ratePerKG,
                                Quantity = y.Count(),
                                UnitPrice = unitPrice,
                                Amount = (decimal)y.Sum(i => i.Price),
                                ProductCode = dispatch.ProductCode
                            };
                        }));
                    }

                    tempGroup.TotalAmount += tempGroup.Items.Sum(x => x.Amount);

                    var surcharge = db.WeightAdjustments.Where(u => u.ReferenceNo == dispatch.DispatchNo).Where(u => u.Description.Contains("Under Declare")).FirstOrDefault();

                    if (surcharge != null)
                    {
                        var surcharge_item = new SimplifiedItem()
                        {
                            DispatchNo = string.Format("{0} under declared {1}KG", surcharge.ReferenceNo, surcharge.Weight.ToString("N3")),
                            Weight = 0.00m,
                            Country = "",
                            Identifier = "",
                            Rate = 0.00m,
                            Quantity = 0,
                            UnitPrice = 0.00m,
                            Amount = surcharge.Amount,
                            ProductCode = "",
                        };

                        tempGroup.TotalAmount += surcharge_item.Amount;
                        tempGroup.Items.Add(surcharge_item);
                    }
                }
                items_by_currency.Add(tempGroup);
                _logger.LogInformation("Add Table");
            }

            var invoiceInfo = new InvoiceInfo()
            {
                Customer = invoice_info.Customer,
                InvoiceNo = invoice_info.InvoiceNo,
                InvoiceDate = invoice_info.InvoiceDate,
                BillTo = invoice_info.BillTo,
                Dispatches = invoice_info.Dispatches,
                ExtraCharges = invoice_info.ExtraCharges,
                CurrencyItem = items_by_currency
            };

            _logger.LogInformation("Started Generate PDF");

            PdfGenerator generator = new();
            MemoryStream ms = generator.GenerateInvoicePdf(invoiceInfo);

            _logger.LogInformation("Generated PDF");

            var ChibiKey = db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("ChibiKey"));
            var ChibiURL = db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("ChibiURL"));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", ChibiKey.Value);


            var formData = new MultipartFormDataContent();
            var pdfContent = new StreamContent(ms);
            pdfContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            string fileName = invoice_info.InvoiceNo + ".pdf";
            formData.Add(pdfContent, "file", fileName);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ChibiURL.Value + "upload"),
                Content = formData,
            };

            using var response = await client.SendAsync(request);

            _logger.LogInformation("Uploaded to Chibisafe");

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<ChibiUpload>(body);

            if (result != null)
            {
                result.originalName = fileName.Replace(".pdf", "") + $"_{result.name}";
                //Insert to DB
                Chibi entity = new()
                {
                    FileName = result.name == null ? "" : DateTime.Now.ToString("yyyyMMdd") + "_" + result.name,
                    UUID = result.uuid ?? "",
                    URL = result.url ?? "",
                    OriginalName = result.originalName,
                    GeneratedName = result.name ?? ""
                };

                await db.Chibis.AddAsync(entity).ConfigureAwait(false);

                await db.Invoices.AddAsync(new SND.SMP.Invoices.Invoice()
                {
                    DateTime = DateTime.Now,
                    InvoiceNo = invoice_info.InvoiceNo + "|" + result.url,
                    Customer = invoice_info.Customer
                });

                _logger.LogInformation("Created Invoice");

                await db.SaveChangesAsync();

                await FileServer.InsertFileToAlbum(result.uuid, false, db, isInvoice: true);
            }

            var queueTask = db.Queues.Find(QueueId);
            if (queueTask != null)
            {
                DateTime dateCompleted = DateTime.Now;
                var tookInSec = dateCompleted.Subtract(dateStart).TotalSeconds;

                queueTask.Status = QueueEnumConst.STATUS_FINISH;
                queueTask.ErrorMsg = null;
                queueTask.TookInSec = Math.Round(tookInSec, 0);
                queueTask.StartTime = dateStart;
                queueTask.EndTime = dateCompleted;
            }
            _logger.LogInformation("Completed");

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            await LogQueueError(new QueueErrorEventArg
            {
                FilePath = _filePath,
                ErrorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message,
            });
        }
    }


    private static async Task LogQueueError(QueueErrorEventArg arg)
    {
        using db db = new();
        #region Queue
        var q = db.Queues
            .Where(u => u.FilePath == arg.FilePath)
            .Where(u => u.Status == QueueEnumConst.STATUS_GENERATING)
            .FirstOrDefault();

        if (q != null)
        {
            q.Status = QueueEnumConst.STATUS_ERROR;
            q.ErrorMsg = arg.ErrorMsg;
            q.TookInSec = 0;

            await db.SaveChangesAsync().ConfigureAwait(false);
        }
        #endregion
    }
}
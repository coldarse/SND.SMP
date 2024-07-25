using SND.SMP.DispatchConsole.EF;
using static SND.SMP.Shared.EnumConst;
using OfficeOpenXml;
using System.Net.Http.Headers;
using System.Text.Json;
using SND.SMP.Chibis;
using System.Data;
using Microsoft.EntityFrameworkCore;
using SND.SMP.ItemTrackingReviews;
using static SND.SMP.DispatchConsole.TrackingImporter;

namespace SND.SMP.DispatchConsole
{
    public class TrackingNoUpdater
    {
        private List<DataTable> DataTablesByPath;
        private string DispatchNo;
        private uint QueueId;

        public TrackingNoUpdater() { }

        public async Task DiscoverAndUpdate(int blockSize = 50)
        {
            try
            {
                using db db = new();
                var hasRunning = db.Queues
                    .Where(u => u.EventType == QueueEnumConst.EVENT_TYPE_TRACKING_UPDATE)
                    .Where(u => u.Status == QueueEnumConst.STATUS_RUNNING)
                    .Any();

                if (hasRunning) return;

                var newTask = db.Queues
                    .Where(u => u.EventType == QueueEnumConst.EVENT_TYPE_TRACKING_UPDATE)
                    .Where(u => u.Status == QueueEnumConst.STATUS_NEW)
                    .OrderBy(u => u.DateCreated)
                    .FirstOrDefault();

                if (newTask is not null)
                {
                    DispatchNo = newTask.FilePath;
                    QueueId = newTask.Id;

                    newTask.Status = QueueEnumConst.STATUS_RUNNING;
                    newTask.ErrorMsg = null;
                    newTask.TookInSec = 0;

                    await db.SaveChangesAsync().ConfigureAwait(false);
                }

                var trackingNos = db.TrackingNoForUpdates.ToList();

                var dispatch = await db.Dispatches.FirstOrDefaultAsync(x => x.DispatchNo.Equals(DispatchNo));

                var listItemIds = trackingNos.Where(u => u.DispatchNo.Equals(DispatchNo)).ToList();

                string CustomerCode = dispatch.CustomerCode;
                string ProcessType = listItemIds[0].ProcessType;
                var batchedItemIds = new List<string>();

                DateTime dateUpdateStart = DateTime.Now;
                for (int i = 1; i <= listItemIds.Count; i++)
                {
                    batchedItemIds.Add(listItemIds[i - 1].TrackingNo);
                    var blockMilestone = i % blockSize;
                    if (blockMilestone == 0)
                    {
                        await UpdateItemTrackingFile(CustomerCode, batchedItemIds, dispatch, ProcessType);

                        batchedItemIds.Clear();
                    }
                }

                if (batchedItemIds.Count != 0)
                {
                    await UpdateItemTrackingFile(CustomerCode, batchedItemIds, dispatch, ProcessType);

                    batchedItemIds.Clear();
                }

                var queueTask = db.Queues.Find(QueueId);
                if (queueTask != null)
                {
                    DateTime dateUpdateCompleted = DateTime.Now;
                    var tookInSec = dateUpdateCompleted.Subtract(dateUpdateStart).TotalSeconds;

                    queueTask.Status = QueueEnumConst.STATUS_FINISH;
                    queueTask.ErrorMsg = null;
                    queueTask.TookInSec = Math.Round(tookInSec, 0);
                    queueTask.StartTime = dateUpdateStart;
                    queueTask.EndTime = dateUpdateCompleted;
                }

                if (ProcessType.Equals(TrackingNoForUpdateConst.STATUS_DELETE)) db.Dispatches.Remove(dispatch);
                db.TrackingNoForUpdates.RemoveRange(listItemIds);
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await LogQueueError(new QueueErrorEventArg
                {
                    FilePath = DispatchNo,
                    ErrorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message,
                });
            }

        }

        private async Task LogQueueError(QueueErrorEventArg arg)
        {
            using (EF.db db = new())
            {
                #region Queue
                var q = db.Queues
                    .Where(u => u.FilePath == arg.FilePath)
                    .Where(u => u.Status == QueueEnumConst.STATUS_RUNNING)
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
        private async Task UpdateItemTrackingFile(string customerCode, List<string> trackingNos, Dispatch dispatch, string processType)
        {
            using db db = new();

            string _dateUsed = DateOnly.FromDateTime(DateTime.Now).ToString();
            String _dispatchNo = "";

            switch (processType)
            {
                case TrackingNoForUpdateConst.STATUS_UPDATE:
                    _dateUsed = dispatch.DispatchDate.ToString();
                    _dispatchNo = dispatch.DispatchNo;
                    break;
                case TrackingNoForUpdateConst.STATUS_DELETE:
                    _dateUsed = "";
                    _dispatchNo = "";
                    break;
            }

            List<ItemIdPath> itemIdPaths = await GetItemTrackingFiles(customerCode, trackingNos);
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
                        rowToUpdate["DateUsed"] = _dateUsed;
                        rowToUpdate["DispatchNo"] = _dispatchNo;
                    }

                    if (rowsToUpdate.Length > 0)
                    {
                        if (!editedTablePaths.Contains(splits[0].ToString()))
                            editedTablePaths.Add(splits[0].ToString());
                    }

                    foundTable.AcceptChanges();
                }
            }

            foreach (DataTable dataTableByPath in DataTablesByPath)
            {
                if (editedTablePaths.Contains(dataTableByPath.TableName))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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

                    var chibiFile = db.Chibis.FirstOrDefault(x => x.URL.Equals(dataTableByPath.TableName));
                    var generatedName = chibiFile.GeneratedName;
                    var fileName = chibiFile.OriginalName.Replace("_" + generatedName, "") + ".xlsx";
                    var application = db.ItemTrackingApplications.FirstOrDefault(x => x.Path.Equals(dataTableByPath.TableName));

                    var review = db.ItemTrackingReviews.FirstOrDefault(x => x.ApplicationId.Equals(application.Id));

                    ChibiUpload uploadExcel = await FileServer.InsertExcelFileToChibi(excelStream, fileName, originalName: null, postalCode: review.PostalCode, productCode: review.ProductCode);

                    db.Chibis.Remove(chibiFile);

                    application.Path = uploadExcel.url;

                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }
        private async Task<List<ItemIdPath>> GetItemTrackingFiles(string customerCode, List<string> trackingNos)
        {
            DataTablesByPath = [];
            using db db = new();
            db.ChangeTracker.AutoDetectChangesEnabled = false;

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

            List<PrefixSuffixCustomerCode> prefixSuffixCustomerCodesAnyAccount = [];
            foreach (var prefixSuffixCustomerCode in prefixSuffixCustomerCodes)
            {
                prefixSuffixCustomerCodesAnyAccount.Add(new PrefixSuffixCustomerCode
                {
                    Prefix = prefixSuffixCustomerCode.Prefix,
                    Suffix = prefixSuffixCustomerCode.Suffix,
                    CustomerCode = "Any Account"
                });
            }

            prefixSuffixCustomerCodes.AddRange(prefixSuffixCustomerCodesAnyAccount);

            var reviewList = db.ItemTrackingReviews.ToList();

            foreach (var prefixSuffixCustomerCode in prefixSuffixCustomerCodes)
            {
                var tempReviews = reviewList.Where(x => x.Prefix.Equals(prefixSuffixCustomerCode.Prefix) &&
                                                            x.Suffix.Equals(prefixSuffixCustomerCode.Suffix) &&
                                                            x.CustomerCode.Equals(prefixSuffixCustomerCode.CustomerCode)).ToList();

                if (tempReviews.Count > 0) foreach (var review in tempReviews) reviews.Add(review);
            }


            List<string> paths = [];
            List<ItemIdPath> itemIdFilePath = [];

            var applications = db.ItemTrackingApplications.ToList();

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
                    Stream excel_stream = await FileServer.GetFileStream(splits[0].ToString());
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

            return itemIdFilePath;
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


    }
}

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
using Abp.Extensions;
using SND.SMP.Airports;

namespace SND.SMP.DispatchConsole
{
    public class DispatchTrackingUpdater
    {
        private string _filePath;
        private List<DispatchInfo> _dispatchInfos;
        private uint QueueId;

        private int _blockSize;

        public DispatchTrackingUpdater() { }

        public async Task DiscoverAndUpdate(int blockSize = 50)
        {
            try
            {
                _blockSize = blockSize;

                using db db = new();
                var hasRunning = db.Queues
                    .Where(u => u.EventType == QueueEnumConst.EVENT_TYPE_DISPATCH_TRACKING_UPDATE)
                    .Where(u => u.Status == QueueEnumConst.STATUS_RUNNING)
                    .Any();

                if (hasRunning) return;

                var newTask = db.Queues
                    .Where(u => u.EventType == QueueEnumConst.EVENT_TYPE_DISPATCH_TRACKING_UPDATE)
                    .Where(u => u.Status == QueueEnumConst.STATUS_NEW)
                    .OrderBy(u => u.DateCreated)
                    .FirstOrDefault();

                if (newTask is not null)
                {
                    _filePath = newTask.FilePath;
                    QueueId = newTask.Id;

                    newTask.Status = QueueEnumConst.STATUS_RUNNING;
                    newTask.ErrorMsg = null;
                    newTask.TookInSec = 0;

                    await db.SaveChangesAsync().ConfigureAwait(false);
                }

                if (!string.IsNullOrWhiteSpace(_filePath))
                {
                    using EF.db chibiDB = new();
                    var updateFile = chibiDB.Chibis.FirstOrDefault(x => x.URL.Equals(_filePath));
                    if (updateFile is not null)
                    {
                        var fileString = await FileServer.GetFileStreamAsString(updateFile.URL);

                        if (fileString is not null)
                        {
                            _dispatchInfos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DispatchInfo>>(fileString);

                            if (_dispatchInfos != null)
                            {
                                await UpdateFromJson();
                            }
                        }
                    }
                }
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

        private async Task UpdateFromJson()
        {
            DateTime dateUpdateStart = DateTime.Now;

            using (EF.db db = new())
            {
                try
                {
                    List<DispatchCountry> dispatchCountries = [];
                    foreach (var dispatch in _dispatchInfos)
                    {
                        dispatchCountries.AddRange(dispatch.DispatchCountries);
                    }

                    List<Item> customEditItems = [];
                    List<Item> nonCustomEditItems = [];

                    List<string> airports = [.. db.Airports.Select(x => x.Name)];


                    foreach (var country in dispatchCountries)
                    {
                        if (country.Select)
                        {
                            var customEditBags = country.DispatchBags.Where(x => x.Select == true && x.Custom == true).ToList();
                            var nonCustomEditBags = country.DispatchBags.Where(x => x.Select == true && x.Custom == false).ToList();

                            if (customEditBags.Count > 0)
                            {
                                foreach (var bag in customEditBags)
                                {
                                    var items = db.Items.Where(x => x.BagId == bag.BagId).ToList();
                                    foreach (var item in items)
                                    {
                                        var updatedItem = UpdatedItemTracking(item, bag.Stages, airports);
                                        customEditItems.Add(updatedItem);
                                    }
                                }
                            }

                            if (nonCustomEditBags.Count > 0)
                            {
                                foreach (var bag in nonCustomEditBags)
                                {
                                    var items = db.Items.Where(x => x.BagId == bag.BagId).ToList();
                                    foreach (var item in items)
                                    {
                                        var updatedItem = UpdatedItemTracking(item, country.Stages, airports);
                                        nonCustomEditItems.Add(updatedItem);
                                    }
                                }
                            }
                        }
                    }

                    List<Item> allItemsToUpdate = [];
                    allItemsToUpdate.AddRange(customEditItems);
                    allItemsToUpdate.AddRange(nonCustomEditItems);

                    List<Item> itemsToUpdate = [];

                    for (int i = 1; i <= allItemsToUpdate.Count; i++)
                    {
                        itemsToUpdate.Add(allItemsToUpdate[i - 1]);
                        var blockMilestone = i % _blockSize;
                        if (blockMilestone == 0)
                        {
                            db.Items.UpdateRange(itemsToUpdate);
                            await db.SaveChangesAsync().ConfigureAwait(false);

                            itemsToUpdate.Clear();
                        }
                    }

                    if (itemsToUpdate.Count > 0)
                    {
                        db.Items.UpdateRange(itemsToUpdate);
                        await db.SaveChangesAsync().ConfigureAwait(false);
                        itemsToUpdate.Clear();
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

                    var chibi = await db.Chibis.FirstOrDefaultAsync(x => x.URL.Equals(_filePath));
                    if (chibi is not null)
                    {
                        await FileServer.DeleteFile(chibi.UUID);
                        db.Chibis.Remove(chibi);
                        await db.SaveChangesAsync().ConfigureAwait(false);
                    }
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

        private static string FindAirportName(List<string> airportNames, string inputString)
        {
            // Check each airport name to see if it is contained in the input string
            foreach (var airport in airportNames)
            {
                if (inputString.Contains(airport, StringComparison.OrdinalIgnoreCase))
                {
                    return airport;
                }
            }
            return "";
        }

        private static Item UpdatedItemTracking(Item item, Stage stage, List<string> airports)
        {
            if (!stage.Stage1Desc.IsNullOrWhiteSpace() && item.Stage1StatusDesc.IsNullOrWhiteSpace() || item.Stage1StatusDesc != stage.Stage1Desc) item.Stage1StatusDesc = stage.Stage1Desc;
            if (!stage.Stage2Desc.IsNullOrWhiteSpace() && item.Stage2StatusDesc.IsNullOrWhiteSpace() || item.Stage2StatusDesc != stage.Stage2Desc) item.Stage2StatusDesc = stage.Stage2Desc;
            if (!stage.Stage3Desc.IsNullOrWhiteSpace() && item.Stage3StatusDesc.IsNullOrWhiteSpace() || item.Stage3StatusDesc != stage.Stage3Desc) item.Stage3StatusDesc = stage.Stage3Desc;
            if (!stage.Stage4Desc.IsNullOrWhiteSpace() && item.Stage4StatusDesc.IsNullOrWhiteSpace() || item.Stage4StatusDesc != stage.Stage4Desc) item.Stage4StatusDesc = stage.Stage4Desc;
            if (!stage.Stage5Desc.IsNullOrWhiteSpace() && item.Stage5StatusDesc.IsNullOrWhiteSpace() || item.Stage5StatusDesc != stage.Stage5Desc) item.Stage5StatusDesc = stage.Stage5Desc;
            if (!stage.Stage6Desc.IsNullOrWhiteSpace() && !stage.Airport.IsNullOrWhiteSpace())
            {
                string foundAirport = FindAirportName(airports, stage.Stage6Desc);
                string stage6Desc = stage.Stage6Desc.Replace(foundAirport, "");
                string stage6 = stage6Desc + " " + stage.Airport + ".";
                if (item.Stage6StatusDesc.IsNullOrWhiteSpace() || item.Stage6StatusDesc != stage6) item.Stage6StatusDesc = stage6;
            }
            if (!stage.Stage7Desc.IsNullOrWhiteSpace() && item.Stage7StatusDesc.IsNullOrWhiteSpace() || item.Stage7StatusDesc != stage.Stage7Desc) item.Stage8StatusDesc = stage.Stage7Desc;

            if (stage.Stage1DateTime is not null && item.DateStage1 is null || item.DateStage1 != stage.Stage1DateTime) item.DateStage1 = stage.Stage1DateTime;
            if (stage.Stage2DateTime is not null && item.DateStage2 is null || item.DateStage2 != stage.Stage2DateTime) item.DateStage2 = stage.Stage2DateTime;
            if (stage.Stage3DateTime is not null && item.DateStage3 is null || item.DateStage3 != stage.Stage3DateTime) item.DateStage3 = stage.Stage3DateTime;
            if (stage.Stage4DateTime is not null && item.DateStage4 is null || item.DateStage4 != stage.Stage4DateTime) item.DateStage4 = stage.Stage4DateTime;
            if (stage.Stage5DateTime is not null && item.DateStage5 is null || item.DateStage5 != stage.Stage5DateTime) item.DateStage5 = stage.Stage5DateTime;
            if (stage.Stage6DateTime is not null && item.DateStage6 is null || item.DateStage6 != stage.Stage6DateTime) item.DateStage6 = stage.Stage6DateTime;
            if (stage.Stage7DateTime is not null && item.DateStage7 is null || item.DateStage7 != stage.Stage7DateTime) item.DateStage7 = stage.Stage7DateTime;

            return item;
        }

    }
}

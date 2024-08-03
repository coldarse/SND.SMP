using System.Drawing.Text;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using SND.SMP.DispatchConsole.EF;
using SND.SMP.TrackingStatuses;
using static SND.SMP.Shared.EnumConst;

public class TrackingStatusUpdater
{

    private bool isQueue { get; set; }
    private bool isRunningDailyUpdate { get; set; }
    private string _filePath { get; set; }
    private uint QueueId { get; set; }

    public class QueueErrorEventArg : EventArgs
    {
        public string FilePath { get; set; }
        public string ErrorMsg { get; set; }
    }
    public TrackingStatusUpdater() { }

    public async Task DiscoverAndUpdate()
    {
        if (!isRunningDailyUpdate)
        {
            using db db = new();
            var hasUpdating = db.Queues
                    .Where(x => x.EventType == QueueEnumConst.EVENT_TYPE_TRACKING_STATUS)
                    .Where(x => x.Status == QueueEnumConst.STATUS_UPDATING)
                    .Any();

            if (hasUpdating) return;

            var trackingStatus = db.Queues
                    .Where(x => x.EventType == QueueEnumConst.EVENT_TYPE_TRACKING_STATUS)
                    .Where(x => x.Status == QueueEnumConst.STATUS_NEW)
                    .OrderBy(x => x.DateCreated)
                    .FirstOrDefault();

            isQueue = false;
            if (trackingStatus is not null)
            {
                isQueue = true;
                QueueId = trackingStatus.Id;
                _filePath = trackingStatus.FilePath;

                trackingStatus.Status = QueueEnumConst.STATUS_UPDATING;
                await db.SaveChangesAsync().ConfigureAwait(false);
            }


            if (!isQueue)
            {
                DateTime now = DateTime.Now;
                if (now.Hour == 10 && now.Minute == 0)
                {
                    var incomplete_dispatches = db.Dispatches.Where(x => x.Status != -1).ToList();
                    isRunningDailyUpdate = true;
                    await UpdateTrackingStatusByDispatches(incomplete_dispatches);
                }
            }
            else
            {
                var dispatch = db.Dispatches.Where(x => x.DispatchNo.Equals(_filePath));
                await UpdateTrackingStatusByDispatches([.. dispatch]);
            }
        }
    }

    private async Task UpdateTrackingStatusByDispatches(List<Dispatch> dispatches)
    {
        try
        {
            DateTime dateUpdateStarted = DateTime.Now;
            foreach (var dispatch in dispatches)
            {
                await UpdateTrackingStatusForDispatch(dispatch);
            }

            using db db = new();
            var queueTask = db.Queues.Find(QueueId);
            if (queueTask != null)
            {
                DateTime dateUpdateCompleted = DateTime.Now;
                var tookInSec = dateUpdateCompleted.Subtract(dateUpdateStarted).TotalSeconds;

                queueTask.Status = QueueEnumConst.STATUS_FINISH;
                queueTask.ErrorMsg = "";
                queueTask.TookInSec = Math.Round(tookInSec, 0);
                queueTask.StartTime = dateUpdateStarted;
                queueTask.EndTime = dateUpdateCompleted;
            }

            isRunningDailyUpdate = false;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            isRunningDailyUpdate = false;
            await LogQueueError(new QueueErrorEventArg
            {
                FilePath = _filePath,
                ErrorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message,
            });
        }
    }

    private async Task UpdateTrackingStatusForDispatch(Dispatch dispatch)
    {
        using db db = new();
        var items = db.ItemTrackings.Where(x => x.DispatchId == dispatch.Id && x.IsExternal);

        if (items.Any())
        {
            int batchSize = 10;

            // Split the list into batches
            var batches = items.Select((item, index) => new { item, index })
                               .GroupBy(x => x.index / batchSize)
                               .Select(g => g.Select(x => x.item).ToList())
                               .ToList();

            foreach (var batch in batches)
            {
                var itemtracking = batch.FirstOrDefault();
                var dispatchId = itemtracking.DispatchId;
                var dispatchNo = itemtracking.DispatchNo;

                List<string> trackingNos = batch.Select(x => x.TrackingNo).ToList();

                var apiUrl = await db.ApplicationSettings.FirstOrDefaultAsync(x => x.Name.Equals("APIURL"));

                if (apiUrl != null)
                {
                    string param = "";
                    foreach (var trackingNo in trackingNos)
                    {
                        param += $"trackingNos={trackingNo}&";
                    }

                    param = param[..^1];

                    var apg_tracking = new HttpClient();
                    apg_tracking.DefaultRequestHeaders.Clear();

                    var apg_tracking_request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(apiUrl.Value + "Trackings/APG?" + param),

                    };
                    using var response = await apg_tracking.SendAsync(apg_tracking_request).ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    var body = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<APGTracking>>(body);

                    if (result.Count != 0)
                    {
                        foreach (var tracking in result)
                        {
                            List<TrackingStatus> trackingStatusFromAPI = [];
                            foreach (var status in tracking.status)
                            {
                                trackingStatusFromAPI.Add(new TrackingStatus()
                                {
                                    TrackingNo = tracking.package.tracking,
                                    DispatchId = dispatchId,
                                    DispatchNo = dispatchNo,
                                    Status = status.description,
                                    DateTime = DateTime.Parse(status.statusDate.Replace(" UTC", ""))
                                });
                            }
                            var trackingStatus = db.TrackingStatus.Where(x => x.TrackingNo.Equals(tracking.package.tracking));

                            if (trackingStatus.Any())
                            {
                                var trackingStatus_status = new HashSet<string>(trackingStatus.Select(s => s.Status));
                                var updates = trackingStatusFromAPI.Where(s => !trackingStatus_status.Contains(s.Status)).ToList();
                                if (updates.Count != 0) db.TrackingStatus.AddRange(trackingStatusFromAPI);
                            }
                            else
                            {
                                db.TrackingStatus.AddRange(trackingStatusFromAPI);
                            }
                        }
                    }
                }
                else throw new Exception("No API URL Found.");
            }
        }
        else throw new Exception("No Trackings Found.");
    }

    private async Task LogQueueError(QueueErrorEventArg arg)
    {
        using (db db = new())
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
}
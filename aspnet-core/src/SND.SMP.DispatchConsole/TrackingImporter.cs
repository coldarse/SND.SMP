using System;
using ExcelDataReader;
using System.Globalization;
using SND.SMP.DispatchConsole.Dto;
using static SND.SMP.Shared.EnumConst;

namespace SND.SMP.DispatchConsole
{
	public class TrackingImporter
	{
        private uint _queueId { get; set; }
        private string _filePath { get; set; }

        public class QueueErrorEventArg : EventArgs
        {
            public string FilePath { get; set; }
            public string ErrorMsg { get; set; }
        }

        public TrackingImporter()
		{
		}

        public async Task DiscoverAndImport()
        {
            using (EF.db db = new EF.db())
            {
                var hasRunning = db.Queues
                    .Where(u => u.EventType == QueueEnumConst.EVENT_TYPE_TRACKING_UPLOAD)
                    .Where(u => u.Status == QueueEnumConst.STATUS_RUNNING)
                    .Any();

                if (hasRunning) return;

                var newTask = db.Queues
                    .Where(u => u.EventType == QueueEnumConst.EVENT_TYPE_TRACKING_UPLOAD)
                    .Where(u => u.Status == QueueEnumConst.STATUS_NEW)
                    .OrderBy(u => u.DateCreated)
                    .FirstOrDefault();

                if (newTask != null)
                {
                    _queueId = newTask.Id;
                    _filePath = newTask.FilePath;

                    newTask.Status = QueueEnumConst.STATUS_RUNNING;
                    newTask.ErrorMsg = null;
                    newTask.TookInSec = null;

                    await db.SaveChangesAsync();
                }
            }

            if (!string.IsNullOrWhiteSpace(_filePath))
            {
                var filesExist = File.Exists(_filePath);

                if (filesExist)
                {
                    await ImportFromExcel();
                }
            }
        }

        private async Task ImportFromExcel()
        {
            DateTime dateImportStart = DateTime.Now;

            using (EF.db db = new EF.db())
            {
                try
                {
                    #region Import
                    dateImportStart = DateTime.Now;

                    using (var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var rowCount = reader.RowCount;

                            var listItems = new List<DispatchItemDto>();

                            var itemCount = 0;

                            do
                            {
                                var dispatchNo = reader.Name.Trim();

                                var listTracking = new List<TrackingImportDto>();

                                while (reader.Read())
                                {
                                    if (itemCount > 0)
                                    {
                                        var bagNo = (string)reader[0];
                                        DateTime? dateStage1 = reader[1] == null ? null : (DateTime)reader[1];
                                        DateTime? dateStage2 = reader[2] == null ? null : (DateTime)reader[2];
                                        DateTime? dateStage3 = reader[3] == null ? null : (DateTime)reader[3];
                                        DateTime? dateStage4 = reader[4] == null ? null : (DateTime)reader[4];
                                        DateTime? dateStage5 = reader[5] == null ? null : (DateTime)reader[5];
                                        DateTime? dateStage6 = reader[6] == null ? null : (DateTime)reader[6];
                                        DateTime? dateStage7 = reader[7] == null ? null : (DateTime)reader[7];
                                        var destinationAirport = (string)reader[8];

                                        listTracking.Add(new TrackingImportDto
                                        {
                                            BagNo = bagNo,
                                            DateStage1 = dateStage1,
                                            DateStage2 = dateStage2,
                                            DateStage3 = dateStage3,
                                            DateStage4 = dateStage4,
                                            DateStage5 = dateStage5,
                                            DateStage6 = dateStage6,
                                            DateStage7 = dateStage7,
                                            DestinationAirport = destinationAirport
                                        });
                                    }

                                    itemCount++;
                                }

                                if (listTracking.Any())
                                {
                                    //Single insert to db 
                                }
                            } while (reader.NextResult());

                            var queueTask = db.Queues.Find(_queueId);
                            if (queueTask != null)
                            {
                                DateTime dateImportCompleted = DateTime.Now;
                                var tookInSec = dateImportCompleted.Subtract(dateImportStart).TotalSeconds;

                                queueTask.Status = QueueEnumConst.STATUS_FINISH;
                                queueTask.ErrorMsg = null;
                                queueTask.TookInSec = Math.Round(tookInSec, 0);
                                queueTask.StartTime = dateImportStart;
                                queueTask.EndTime = dateImportCompleted;
                            }

                            await db.SaveChangesAsync();
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        await LogQueueError(new QueueErrorEventArg
                        {
                            FilePath = _filePath,
                            ErrorMsg = ex.InnerException.Message
                        });
                    }
                }
            }
        }

        private async Task LogQueueError(QueueErrorEventArg arg)
        {
            using (EF.db db = new EF.db())
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
                    q.TookInSec = null;

                    await db.SaveChangesAsync();
                }
                #endregion
            }
        }
    }
}


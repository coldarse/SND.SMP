using System;
using ExcelDataReader;
using static SND.SMP.DispatchConsole.WorkerDispatchImport;
using System.Globalization;
using static SND.SMP.Shared.EnumConst;
using SND.SMP.DispatchConsole.Dto;
//using SND.SMP.Shared.Modules.Dispatch;
using SND.SMP.DispatchConsole.EF;

namespace SND.SMP.DispatchConsole
{
	public class RateWeightBreakImporter
	{
        private uint _queueId { get; set; }
        private string _filePath { get; set; }

        public RateWeightBreakImporter()
        {
        }

        public async Task DiscoverAndImport()
        {
            using (EF.db db = new EF.db())
            {
                var hasRunning = db.Queues
                    .Where(u => u.EventType == QueueEnumConst.EVENT_TYPE_RATE_WEIGHT_BREAK)
                    .Where(u => u.Status == QueueEnumConst.STATUS_RUNNING)
                    .Any();

                if (hasRunning) return;

                var newTask = db.Queues
                    .Where(u => u.EventType == QueueEnumConst.EVENT_TYPE_RATE_WEIGHT_BREAK)
                    .Where(u => u.Status == QueueEnumConst.STATUS_NEW)
                    .OrderBy(u => u.DateCreated)
                    .FirstOrDefault();

                if (newTask != null)
                {
                    _queueId = newTask.Id;
                    _filePath = newTask.FilePath;

                    newTask.Status = QueueEnumConst.STATUS_RUNNING;
                    newTask.ErrorMsg = null;
                    newTask.TookInSec = 0;

                    await db.SaveChangesAsync();
                }
            }

            if (!string.IsNullOrWhiteSpace(_filePath))
            {
                var filesExist = File.Exists(_filePath);

                await ImportFromExcel();
            }
        }

        private async Task ImportFromExcel()
        {
            DateTime dateImportStart = DateTime.Now;

            using (EF.db db = new EF.db())
            {
                try
                {
                    RateWeightBreakUtil util = new RateWeightBreakUtil();
                    var list = util.ReadRateWeightBreak(_filePath).GroupBy(u => u.RateCardName);

                    foreach (var rateCard in list)
                    {
                        var name = rateCard.Key;

                        EF.Rate card = db.Rates.Where(u => u.CardName == name).FirstOrDefault();

                        if (card == null)
                        {
                            card = new EF.Rate
                            {
                                CardName = name
                            };

                            db.Rates.Add(card);
                            await db.SaveChangesAsync();
                        }

                        var listRateWeightBreak = list.Select(u => new EF.Rateweightbreak
                        {
                            RateId = card.Id,
                            
                            
                        }).ToList();

                        await db.Rateweightbreaks.AddRangeAsync(listRateWeightBreak);
                    }
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
                    q.TookInSec = 0;

                    await db.SaveChangesAsync();
                }
                #endregion
            }
        }
    }
}


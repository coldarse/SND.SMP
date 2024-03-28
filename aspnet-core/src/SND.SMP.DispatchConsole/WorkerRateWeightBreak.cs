using ExcelDataReader;
using System.Globalization;
using System.Threading;
using Newtonsoft;
using System.IO;

namespace SND.SMP.DispatchConsole;

public class WorkerRateWeightBreak : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public class QueueErrorEventArg : EventArgs
    {
        public string FilePath { get; set; }
        public string ErrorMsg { get; set; }
    }

    public WorkerRateWeightBreak(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    private void LogQueueError(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var pollInMs = _configuration.GetValue<int>("Import:RateWeightBreakPollInSec") * 1000;
            
            DispatchImporter dispatchImporter = new DispatchImporter();

            //await dispatchImporter.DiscoverAndImport(fileType: fileType, batchSize: batchSize);

            #region Logger
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker Dispatch running at: {time}", DateTimeOffset.Now);
            }
            #endregion

            await Task.Delay(pollInMs, stoppingToken);
        }
    }
}


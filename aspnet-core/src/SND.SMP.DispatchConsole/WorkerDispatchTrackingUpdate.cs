using ExcelDataReader;
using System.Globalization;
using System.Threading;
using Newtonsoft;
using System.IO;

namespace SND.SMP.DispatchConsole;

public class WorkerDispatchTrackingUpdate : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    public WorkerDispatchTrackingUpdate(ILogger<Worker> logger, IConfiguration configuration)
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
            var pollInMs = _configuration.GetValue<int>("Update:TrackingNoUpdatePollInSec") * 1000;
            int blockSize = _configuration.GetValue<int>("Import:BlockSize");

            DispatchTrackingUpdater dispatchTrackingUpdater = new DispatchTrackingUpdater();

            await dispatchTrackingUpdater.DiscoverAndUpdate(blockSize);

            #region Logger
            if (_logger.IsEnabled(LogLevel.Information))
            {
                // _logger.LogInformation("Worker Dispatch Tracking Update running at: {time}", DateTimeOffset.Now);
            }
            #endregion

            await Task.Delay(pollInMs, stoppingToken);
        }
    }
}
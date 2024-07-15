using ExcelDataReader;
using System.Globalization;
using System.Threading;
using Newtonsoft;
using System.IO;

namespace SND.SMP.DispatchConsole;

public class WorkerItemTrackingGenerate : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    public WorkerItemTrackingGenerate(ILogger<Worker> logger, IConfiguration configuration)
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
            var pollInMs = _configuration.GetValue<int>("Generate:TrackingNoGeneratePollInSec") * 1000;

            TrackingNoGenerator trackingNoGenerator = new TrackingNoGenerator();

            await trackingNoGenerator.DiscoverAndGenerate();

            #region Logger
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker Item Tracking Generate running at: {time}", DateTimeOffset.Now);
            }
            #endregion

            await Task.Delay(pollInMs, stoppingToken);
        }
    }
}
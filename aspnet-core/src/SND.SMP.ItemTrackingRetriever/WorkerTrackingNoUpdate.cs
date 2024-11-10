using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SND.SMP.ItemTrackingRetriever;

public class WorkerItemTrackingNoUpdate : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    public WorkerItemTrackingNoUpdate(ILogger<Worker> logger, IConfiguration configuration)
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
            var pollInMs = _configuration.GetValue<int>("Retrieve:TrackingNoRetrievePollInSec") * 1000;

            ItemTrackingRetriever trackingNoUpdater = new ItemTrackingRetriever();

            await trackingNoUpdater.RetrieveItemTrackingDetails();

            #region Logger
            if (_logger.IsEnabled(LogLevel.Information))
            {
                // _logger.LogInformation("Worker Tracking No. Update running at: {time}", DateTimeOffset.Now);
            }
            #endregion

            await Task.Delay(pollInMs, stoppingToken);
        }
    }
}
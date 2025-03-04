using ExcelDataReader;
using System.Globalization;
using System.Threading;
using Newtonsoft;
using System.IO;
//using SND.SMP.Shared.Modules.Dispatch;

namespace SND.SMP.DispatchConsole;

public class WorkerInvoiceGenerate : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public class QueueErrorEventArg : EventArgs
    {
        public string FilePath { get; set; }
        public string ErrorMsg { get; set; }
    }

    public WorkerInvoiceGenerate(ILogger<Worker> logger, IConfiguration configuration)
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
            var pollInMs = _configuration.GetValue<int>("Validate:DispatchValidatePollInSec") * 1000;

            InvoiceGenerator invoiceGenerator = new InvoiceGenerator(_logger);

            await invoiceGenerator.DiscoverAndGenerate();

            #region Logger
            if (_logger.IsEnabled(LogLevel.Information))
            {
                // _logger.LogInformation("Worker Invoice Generator running at: {time}", DateTimeOffset.Now);
            }
            #endregion

            await Task.Delay(pollInMs, stoppingToken);
        }
    }
}


using ExcelDataReader;
using System.Globalization;
using System.Threading;
using Newtonsoft;
using System.IO;
//using SND.SMP.Shared.Modules.Dispatch;

namespace SND.SMP.DispatchConsole;

public class WorkerDispatchValidate : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public class QueueErrorEventArg : EventArgs
    {
        public string FilePath { get; set; }
        public string ErrorMsg { get; set; }
    }

    public WorkerDispatchValidate(ILogger<Worker> logger, IConfiguration configuration)
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
            string dirPath = _configuration.GetValue<string>("Validate:DirPath");
            string fileType = _configuration.GetValue<string>("Validate:FileType");
            int batchSize = _configuration.GetValue<int>("Validate:BatchSize");
            int blockSize = _configuration.GetValue<int>("Validate:BlockSize");

            DispatchValidator dispatchValidator = new DispatchValidator();

            await dispatchValidator.DiscoverAndValidate(dirPath: dirPath, fileType: fileType, batchSize: batchSize, blockSize: blockSize);

            #region Logger
            if (_logger.IsEnabled(LogLevel.Information))
            {
                //_logger.LogInformation("Worker Dispatch running at: {time}", DateTimeOffset.Now);
            }
            #endregion

            await Task.Delay(pollInMs, stoppingToken);
        }
    }
}


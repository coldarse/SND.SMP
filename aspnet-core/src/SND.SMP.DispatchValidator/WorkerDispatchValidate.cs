using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SND.SMP.DispatchValidator.Dto;

namespace SND.SMP.DispatchValidator;

public class WorkerDispatchValidate : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public class QueueErrorEventArg : EventArgs
    {
        public string FilePath { get; set; }
        public string ErrorMsg { get; set; }
        public List<DispatchValidateDto> Validations { get; set; }
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
            string fileType = _configuration.GetValue<string>("Validate:FileType");
            int blockSize = _configuration.GetValue<int>("Validate:BlockSize");

            DispatchValidator dispatchValidator = new DispatchValidator();

            await dispatchValidator.DiscoverAndValidate(fileType: fileType, blockSize: blockSize);

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


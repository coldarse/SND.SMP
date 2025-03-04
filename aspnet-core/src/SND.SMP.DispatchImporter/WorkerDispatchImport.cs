﻿using ExcelDataReader;
using System.Globalization;
using System.Threading;
using Newtonsoft;
using System.IO;
using SND.SMP.DispatchImporter.Dto;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace SND.SMP.DispatchImporter;

public class WorkerDispatchImport : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public class QueueErrorEventArg : EventArgs
    {
        public string FilePath { get; set; }
        public string ErrorMsg { get; set; }
        public List<DispatchValidateDto> Validations { get; set; }
    }

    public WorkerDispatchImport(ILogger<Worker> logger, IConfiguration configuration)
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
            var pollInMs = _configuration.GetValue<int>("Import:DispatchImportPollInSec") * 1000;
            string fileType = _configuration.GetValue<string>("Import:FileType");
            int batchSize = _configuration.GetValue<int>("Import:BatchSize");
            int blockSize = _configuration.GetValue<int>("Import:BlockSize");

            DispatchImporter dispatchImporter = new DispatchImporter();

            await dispatchImporter.DiscoverAndImport(fileType: fileType, batchSize: batchSize, blockSize: blockSize);

            #region Logger
            if (_logger.IsEnabled(LogLevel.Information))
            {
                // _logger.LogInformation("Worker Dispatch running at: {time}", DateTimeOffset.Now);
            }
            #endregion

            await Task.Delay(pollInMs, stoppingToken);
        }
    }
}


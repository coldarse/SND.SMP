﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;

namespace SND.SMP.ItemTrackingUpdater;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddMySqlDataSource(builder.Configuration.GetConnectionString("Default")!);

        builder.Services.AddTransient<MySqlConnection>(_ =>
            new MySqlConnection(builder.Configuration.GetConnectionString("Default")));

        builder.Services.AddHostedService<WorkerItemTrackingNoUpdate>();

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        var host = builder.Build();
        host.Run();
    }
}
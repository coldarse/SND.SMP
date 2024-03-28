using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace SND.SMP.DispatchConsole;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddMySqlDataSource(builder.Configuration.GetConnectionString("Default")!);

        builder.Services.AddTransient<MySqlConnection>(_ =>
            new MySqlConnection(builder.Configuration.GetConnectionString("Default")));

        builder.Services.AddHostedService<WorkerDispatchImport>();
        builder.Services.AddHostedService<WorkerDispatchValidate>();
        //builder.Services.AddHostedService<WorkerRateWeightBreak>();
        //builder.Services.AddHostedService<WorkerTracking>();

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        var host = builder.Build();
        host.Run();
    }
}

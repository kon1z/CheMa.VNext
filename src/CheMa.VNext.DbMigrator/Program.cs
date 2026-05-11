using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace CheMa.VNext.DbMigrator;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
#if DEBUG
                .MinimumLevel.Override("CheMa.VNext", LogEventLevel.Debug)
#else
                .MinimumLevel.Override("CheMa.VNext", LogEventLevel.Information)
#endif
                .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        await CreateHostBuilder(args).RunConsoleAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseAgileConfig()
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddEnvironmentVariables();
            })
            .ConfigureLogging((_, logging) =>
            {
                logging.ClearProviders();
                logging.AddSerilog();
            })
            .ConfigureServices((hostContext, services) =>
            {
                if (string.IsNullOrWhiteSpace(hostContext.Configuration.GetConnectionString("Default")))
                {
                    throw new InvalidOperationException(
                        "Missing configuration value 'ConnectionStrings:Default'. Start CheMa.VNext.AppHost or configure it via appsettings.json, AgileConfig, or environment variable 'ConnectionStrings__Default'.");
                }

                services.AddServiceDefaults(hostContext.Configuration, hostContext.HostingEnvironment);
                services.AddHostedService<DbMigratorHostedService>();
            });
    }
}

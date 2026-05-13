using System;
using System.Threading.Tasks;
using AgileConfig.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Volo.Abp;
using Volo.Abp.Autofac;

namespace CheMa.VNext;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        try
        {
            Log.Information("Starting CheMa.VNext.BackgroundWorker.");
            using var host = CreateHostBuilder(args).Build();
            Log.Information("Worker startup phase: after host.Build");

            var application = host.Services.GetRequiredService<IAbpApplicationWithExternalServiceProvider>();

            await application.InitializeAsync(host.Services);
            Log.Information("Worker startup phase: after InitializeAsync");

            try
            {
                await host.RunAsync();
            }
            finally
            {
                await application.ShutdownAsync();
            }

            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var hostBuilder = Host.CreateDefaultBuilder(args)
            .UseAutofac()
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddAgileConfig();
                config.AddEnvironmentVariables();
            })
            .ConfigureLogging((_, logging) =>
            {
                logging.ClearProviders();
                logging.AddSerilog();
            });

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environments.Development}.json", optional: true, reloadOnChange: false)
            .AddAgileConfig()
            .AddEnvironmentVariables()
            .Build();

        if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("Default")))
        {
            throw new InvalidOperationException(
                "Missing configuration value 'ConnectionStrings:Default'. Start CheMa.VNext.AppHost or configure it via appsettings.json, appsettings.Development.json, AgileConfig, or environment variable 'ConnectionStrings__Default'.");
        }

        hostBuilder.ConfigureServices((hostContext, services) =>
        {
            services.AddServiceDefaults(hostContext.Configuration, hostContext.HostingEnvironment);
        });

        Log.Information("Worker startup phase: before AddApplication");
        hostBuilder.ConfigureServices(services =>
        {
            services.AddApplication<VNextBackgroundWorkerModule>(options =>
            {
                options.Services.ReplaceConfiguration(configuration);
            });
        });
        Log.Information("Worker startup phase: after AddApplication");

        return hostBuilder;
    }
}

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
            var builder = Host.CreateApplicationBuilder(args);
            builder.AddServiceDefaults();
            builder.Configuration.AddAgileConfig(new ConfigClient(builder.Configuration), static (ConfigReloadedArgs _) => { });
            builder.Configuration.AddEnvironmentVariables();

            if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Default")))
            {
                throw new InvalidOperationException(
                    "Missing configuration value 'ConnectionStrings:Default'. Start CheMa.VNext.AppHost or configure it via appsettings.json, appsettings.Development.json, AgileConfig, or environment variable 'ConnectionStrings__Default'.");
            }

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();
            Log.Information("Worker startup phase: before AddApplicationAsync");
            try
            {
                await builder.Services.AddApplicationAsync<VNextBackgroundWorkerModule>(options =>
                {
                    options.Services.ReplaceConfiguration(builder.Configuration);
                    options.UseAutofac();
                });
                Log.Information("Worker startup phase: after AddApplicationAsync");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Worker startup failed during AddApplicationAsync");
                throw;
            }

            var host = builder.Build();
            Log.Information("Worker startup phase: after host.Build");
            try
            {
                await host.Services.GetRequiredService<IAbpApplicationWithExternalServiceProvider>()
                    .InitializeAsync(host.Services);
                Log.Information("Worker startup phase: after InitializeAsync");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Worker startup failed during InitializeAsync");
                throw;
            }
            await host.RunAsync();
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
}

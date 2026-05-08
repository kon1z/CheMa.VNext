using System;
using System.Threading.Tasks;
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
            builder.Configuration.AddJsonFile("appsettings.secrets.json", optional: true, reloadOnChange: true);

            if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Default")))
            {
                throw new InvalidOperationException(
                    "Missing configuration value 'ConnectionStrings:Default'. Start CheMa.VNext.AppHost or configure it via user secrets, appsettings.secrets.json, or environment variable 'ConnectionStrings__Default'.");
            }

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();
            await builder.Services.AddApplicationAsync<VNextBackgroundWorkerModule>(options =>
            {
                options.Services.ReplaceConfiguration(builder.Configuration);
                options.UseAutofac();
            });

            var host = builder.Build();
            await host.Services.GetRequiredService<IAbpApplicationWithExternalServiceProvider>()
                .InitializeAsync(host.Services);
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

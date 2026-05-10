using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace CheMa.VNext.DbMigrator;

class Program
{
    static async Task Main(string[] args)
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

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddSerilog();

                if (!string.IsNullOrWhiteSpace(context.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
                {
                    logging.AddOpenTelemetry(options =>
                    {
                        options.SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService(context.HostingEnvironment.ApplicationName)
                            .AddAttributes(
                            [
                                new KeyValuePair<string, object>("service.namespace", "CheMa.VNext"),
                                new KeyValuePair<string, object>("deployment.environment.name", context.HostingEnvironment.EnvironmentName),
                                new KeyValuePair<string, object>("service.instance.id", Environment.MachineName)
                            ]));
                        options.IncludeFormattedMessage = true;
                        options.IncludeScopes = true;
                        options.ParseStateValues = true;
                        options.AddOtlpExporter();
                    });
                }
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddServiceDiscovery();

                if (!string.IsNullOrWhiteSpace(hostContext.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
                {
                    services.AddOpenTelemetry()
                        .ConfigureResource(resource => resource
                            .AddService(hostContext.HostingEnvironment.ApplicationName)
                            .AddAttributes(
                            [
                                new KeyValuePair<string, object>("service.namespace", "CheMa.VNext"),
                                new KeyValuePair<string, object>("deployment.environment.name", hostContext.HostingEnvironment.EnvironmentName),
                                new KeyValuePair<string, object>("service.instance.id", Environment.MachineName)
                            ]))
                        .WithMetrics(metrics => metrics
                            .AddRuntimeInstrumentation()
                            .AddOtlpExporter())
                        .WithTracing(tracing => tracing
                            .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(hostContext.Configuration.GetValue("OpenTelemetry:Tracing:SamplerRatio", 1.0))))
                            .AddOtlpExporter());
                }

                services.AddHostedService<DbMigratorHostedService>();
            });
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CheMa.VNext.Data;
using Serilog;
using Volo.Abp;
using Volo.Abp.Data;

namespace CheMa.VNext.DbMigrator;

public class DbMigratorHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DbMigratorHostedService> _logger;

    public DbMigratorHostedService(
        IHostApplicationLifetime hostApplicationLifetime,
        IConfiguration configuration,
        ILogger<DbMigratorHostedService> logger)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting database migration.");

        try
        {
            using (var application = await AbpApplicationFactory.CreateAsync<VNextDbMigratorModule>(options =>
            {
               options.Services.ReplaceConfiguration(_configuration);
               options.UseAutofac();
               options.Services.AddLogging(c => c.AddSerilog());
               options.AddDataMigrationEnvironment();
            }))
            {
                await application.InitializeAsync();

                await application
                    .ServiceProvider
                    .GetRequiredService<VNextDbMigrationService>()
                    .MigrateAsync();

                await application.ShutdownAsync();

                _logger.LogInformation("Database migration completed successfully.");
                _hostApplicationLifetime.StopApplication();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration failed.");
            _hostApplicationLifetime.StopApplication();
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

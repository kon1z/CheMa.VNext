using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Volo.Abp;

namespace CheMa.VNext;

public class BackgroundWorkerHostedService : IHostedService
{
    private readonly IAbpApplicationWithExternalServiceProvider _application;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundWorkerHostedService> _logger;

    public BackgroundWorkerHostedService(
        IAbpApplicationWithExternalServiceProvider application,
        IServiceProvider serviceProvider,
        ILogger<BackgroundWorkerHostedService> logger)
    {
        _application = application;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing background worker host.");
        await _application.InitializeAsync(_serviceProvider);
        _logger.LogInformation("Background worker host initialized.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _application.ShutdownAsync();
    }
}

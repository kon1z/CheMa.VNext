using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace CheMa.VNext.BackgroundWork;

public class BackgroundExecutionContextRunner : IBackgroundExecutionContextRunner, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ILogger<BackgroundExecutionContextRunner> _logger;

    public BackgroundExecutionContextRunner(
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        ILogger<BackgroundExecutionContextRunner> logger)
    {
        _currentTenant = currentTenant;
        _unitOfWorkManager = unitOfWorkManager;
        _logger = logger;
    }

    public Task RunAsync(BackgroundExecutionContextDto context, Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(action);

        return RunAsync(context, async () =>
        {
            await action();
            return true;
        });
    }

    public async Task<TResult> RunAsync<TResult>(BackgroundExecutionContextDto context, Func<Task<TResult>> action)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(action);

        using (_currentTenant.Change(context.TenantId))
        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["TenantId"] = context.TenantId,
            ["OperatorUserId"] = context.OperatorUserId,
            ["CorrelationId"] = context.CorrelationId,
            ["Source"] = context.Source
        }))
        {
            try
            {
                using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
                var result = await action();
                await uow.CompleteAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Background execution failed. TenantId: {TenantId}, OperatorUserId: {OperatorUserId}, CorrelationId: {CorrelationId}, Source: {Source}.",
                    context.TenantId,
                    context.OperatorUserId,
                    context.CorrelationId,
                    context.Source);
                throw;
            }
        }
    }
}

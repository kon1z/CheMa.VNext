using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace CheMa.VNext.BackgroundWork;

public class BackgroundExecutionContextRunner : IBackgroundExecutionContextRunner, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ILogger<BackgroundExecutionContextRunner> _logger;
    private readonly ICurrentUser _currentUser;

    public BackgroundExecutionContextRunner(
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        ILogger<BackgroundExecutionContextRunner> logger, 
        ICurrentUser currentUser)
    {
        _currentTenant = currentTenant;
        _unitOfWorkManager = unitOfWorkManager;
        _logger = logger;
        _currentUser = currentUser;
    }

    public Task RunAsync(BackgroundExecutionContextDto context, Func<Task> action)
    {
        return RunAsync(context, async () =>
        {
            await action();
            return true;
        });
    }

    public async Task<TResult> RunAsync<TResult>(BackgroundExecutionContextDto context, Func<Task<TResult>> action)
    {
        using (_currentTenant.Change(context.TenantId))
        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["TenantId"] = context.TenantId,
            ["OperatorUserId"] = context.OperatorUserId,
            ["CorrelationId"] = context.CorrelationId,
            ["Source"] = context.Source
        }))
        {
            using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            var result = await action();
            await uow.CompleteAsync();
            return result;
        }
    }
}

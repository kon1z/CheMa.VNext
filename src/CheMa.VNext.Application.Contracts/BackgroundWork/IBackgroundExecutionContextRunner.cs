using System;
using System.Threading.Tasks;

namespace CheMa.VNext.BackgroundWork;

public interface IBackgroundExecutionContextRunner
{
    Task RunAsync(BackgroundExecutionContextDto context, Func<Task> action);

    Task<TResult> RunAsync<TResult>(BackgroundExecutionContextDto context, Func<Task<TResult>> action);
}

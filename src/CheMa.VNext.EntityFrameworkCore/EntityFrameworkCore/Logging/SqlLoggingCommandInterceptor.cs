using System;
using System.Data.Common;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CheMa.VNext.EntityFrameworkCore.Logging;

public class SqlLoggingCommandInterceptor : DbCommandInterceptor
{
    private readonly ILogger<SqlLoggingCommandInterceptor> _logger;
    private readonly int _slowThresholdMs;

    public SqlLoggingCommandInterceptor(ILogger<SqlLoggingCommandInterceptor> logger, IConfiguration configuration)
    {
        _logger = logger;
        _slowThresholdMs = configuration.GetValue("Logging:Sql:SlowThresholdMs", 500);
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        LogIfSlow(command, eventData.Duration, null);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData.Duration, null);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        LogIfSlow(command, eventData.Duration, null);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData.Duration, null);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        LogIfSlow(command, eventData.Duration, null);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData.Duration, null);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        LogException(command, eventData.Duration, eventData.Exception);
        base.CommandFailed(command, eventData);
    }

    public override Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        LogException(command, eventData.Duration, eventData.Exception);
        return base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    private void LogIfSlow(DbCommand command, TimeSpan duration, Exception? exception)
    {
        if (duration.TotalMilliseconds < _slowThresholdMs)
        {
            return;
        }

        LogSql("sql.slow", LogLevel.Warning, command, duration, exception);
    }

    private void LogException(DbCommand command, TimeSpan duration, Exception exception) => LogSql("sql.exception", LogLevel.Error, command, duration, exception);

    private void LogSql(string eventType, LogLevel level, DbCommand command, TimeSpan duration, Exception? exception)
    {
        var activity = Activity.Current;
        var parameters = command.Parameters
            .Cast<DbParameter>()
            .ToDictionary(parameter => parameter.ParameterName, parameter => parameter.Value?.ToString());

        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["event_type"] = eventType,
            ["trace_id"] = activity?.TraceId.ToString(),
            ["span_id"] = activity?.SpanId.ToString(),
            ["db.system"] = "postgresql",
            ["db.name"] = command.Connection?.Database,
            ["db.statement"] = command.CommandText,
            ["db.parameters"] = parameters,
            ["db.duration_ms"] = Math.Round(duration.TotalMilliseconds, 2),
            ["db.slow_threshold_ms"] = _slowThresholdMs,
            ["db.operation"] = GetOperation(command.CommandText),
            ["db.exception.code"] = TryGetExceptionCode(exception),
            ["exception.type"] = exception?.GetType().FullName,
            ["exception.message"] = exception?.Message,
            ["exception.stacktrace"] = exception?.ToString(),
            ["success"] = exception == null,
            ["duration_ms"] = Math.Round(duration.TotalMilliseconds, 2)
        }))
        {
            _logger.Log(level,
                exception,
                "{event_type} {db_operation} {db_duration_ms}ms",
                eventType,
                GetOperation(command.CommandText),
                Math.Round(duration.TotalMilliseconds, 2));
        }
    }

    private static string? GetOperation(string? commandText)
    {
        return commandText?
            .TrimStart()
            .Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()?
            .ToUpperInvariant();
    }

    private static string? TryGetExceptionCode(Exception? exception)
    {
        return exception?.GetType().GetProperty("SqlState")?.GetValue(exception)?.ToString()
            ?? exception?.GetType().GetProperty("Code")?.GetValue(exception)?.ToString();
    }
}

using System;

namespace CheMa.VNext.BackgroundWork;

public sealed class BackgroundExecutionContextDto
{
    public Guid? TenantId { get; set; }

    public Guid? OperatorUserId { get; set; }

    public string? CorrelationId { get; set; }

    public string? Source { get; set; }
}

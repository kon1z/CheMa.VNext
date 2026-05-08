using System;

namespace CheMa.VNext.BackgroundWork;

public class SampleBackgroundWorkRequestedEto
{
    public Guid CorrelationId { get; set; }

    public string Message { get; set; } = string.Empty;

    public BackgroundExecutionContextDto ExecutionContext { get; set; } = new();
}

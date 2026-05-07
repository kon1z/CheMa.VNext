using System;

namespace CheMa.VNext.BackgroundWork;

public class SampleJobArgs
{
    public Guid CorrelationId { get; set; }

    public string Message { get; set; } = string.Empty;
}

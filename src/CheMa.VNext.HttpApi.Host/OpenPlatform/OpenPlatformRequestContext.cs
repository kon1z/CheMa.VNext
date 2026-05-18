using System;

namespace CheMa.VNext.OpenPlatform;

public class OpenPlatformRequestContext
{
    public Guid OpenAppId { get; init; }

    public string ClientId { get; init; } = string.Empty;

    public string AppName { get; init; } = string.Empty;

    public long RequestTimestamp { get; init; }

    public string Nonce { get; init; } = string.Empty;
}

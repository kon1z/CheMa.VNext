namespace CheMa.VNext.OpenPlatform;

public class OpenPlatformValidationResult
{
    public OpenApp OpenApp { get; init; } = default!;

    public string ClientId { get; init; } = string.Empty;

    public long Timestamp { get; init; }

    public string Nonce { get; init; } = string.Empty;

    public string SignVersion { get; init; } = string.Empty;
}

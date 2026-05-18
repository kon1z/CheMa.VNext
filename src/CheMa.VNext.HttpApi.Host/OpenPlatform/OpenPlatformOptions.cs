using CheMa.VNext.OpenPlatform;

namespace CheMa.VNext.OpenPlatform;

public class OpenPlatformOptions
{
    public bool Enabled { get; set; } = true;

    public string OpenApiPrefix { get; set; } = OpenPlatformConsts.DefaultOpenApiPrefix;

    public OpenPlatformSignatureOptions Signature { get; set; } = new();
}

public class OpenPlatformSignatureOptions
{
    public int AllowedTimestampSkewSeconds { get; set; } = 300;

    public string SignVersion { get; set; } = OpenPlatformConsts.DefaultSignVersion;
}

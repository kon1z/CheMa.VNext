using System.ComponentModel.DataAnnotations;

namespace CheMa.VNext.OpenPlatform;

public class OpenPlatformSignatureDebugInput
{
    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string Timestamp { get; set; } = string.Empty;

    [Required]
    public string Nonce { get; set; } = string.Empty;

    public string? SignVersion { get; set; }

    [Required]
    public string Method { get; set; } = string.Empty;

    [Required]
    public string Path { get; set; } = string.Empty;

    public string? Query { get; set; }

    public string? Body { get; set; }

    [Required]
    public string AppSecret { get; set; } = string.Empty;
}

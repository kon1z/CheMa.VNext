namespace CheMa.VNext.OpenPlatform.Dtos;

public class OpenPlatformResponseDto<T>
{
    public string Code { get; set; } = OpenPlatformResponseCodes.Success;

    public string Message { get; set; } = "success";

    public T? Data { get; set; }

    public string? TraceId { get; set; }
}

public static class OpenPlatformResponseCodes
{
    public const string Success = "0";

    public const string Unauthorized = "401";

    public const string Forbidden = "403";

    public const string NotFound = "404";

    public const string Unsupported = "422";

    public const string InvalidRequest = "400";

    public const string InternalError = "500";
}

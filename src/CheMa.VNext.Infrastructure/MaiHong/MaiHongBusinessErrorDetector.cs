using System;
using System.Text.Json;
using CheMa.VNext.ExternalServices;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿 HTTP 2xx 响应业务错误检测器。
/// </summary>
public sealed class MaiHongBusinessErrorDetector : IExternalServiceBusinessErrorDetector
{
    /// <inheritdoc />
    public bool TryDetect(string responseBody, out ExternalServiceBusinessError businessError)
    {
        businessError = null!;

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("errno", out var errno))
            {
                return false;
            }

            var externalErrorCode = GetErrorCode(errno);
            if (IsSuccessCode(externalErrorCode))
            {
                return false;
            }

            businessError = new ExternalServiceBusinessError
            {
                ExternalErrorCode = string.IsNullOrWhiteSpace(externalErrorCode) ? "UNKNOWN" : externalErrorCode,
                ExternalErrorMessage = GetErrorMessage(root),
                BusinessExceptionCode = GetBusinessExceptionCode(externalErrorCode)
            };

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string? GetErrorCode(JsonElement errno)
    {
        return errno.ValueKind switch
        {
            JsonValueKind.Number => errno.GetRawText(),
            JsonValueKind.String => errno.GetString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => null,
            _ => errno.GetRawText()
        };
    }

    private static bool IsSuccessCode(string? externalErrorCode)
    {
        return string.Equals(externalErrorCode, "0", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetBusinessExceptionCode(string? externalErrorCode)
    {
        return externalErrorCode switch
        {
            "5" => VNextDomainErrorCodes.MaiHongDeviceAlreadyBound,
            _ => VNextDomainErrorCodes.MaiHongBusinessError
        };
    }

    private static string GetErrorMessage(JsonElement root)
    {
        if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
        {
            var message = error.GetString();
            if (!string.IsNullOrWhiteSpace(message))
            {
                return message;
            }
        }

        return "迈鸿服务返回业务异常。";
    }
}

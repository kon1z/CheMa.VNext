using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Validation;

namespace CheMa.VNext.OpenPlatform;

[Authorize]
public class OpenPlatformSignatureDebugAppService : VNextAppService, IOpenPlatformSignatureDebugAppService
{
    public Task<OpenPlatformSignatureDebugResultDto> GenerateAsync(OpenPlatformSignatureDebugInput input)
    {
        Check.NotNull(input, nameof(input));

        var method = NormalizeRequired(input.Method, nameof(input.Method)).ToUpperInvariant();
        var path = NormalizePath(input.Path);
        var clientId = NormalizeRequired(input.ClientId, nameof(input.ClientId));
        var timestamp = NormalizeRequired(input.Timestamp, nameof(input.Timestamp));
        var nonce = NormalizeRequired(input.Nonce, nameof(input.Nonce));
        var appSecret = NormalizeRequired(input.AppSecret, nameof(input.AppSecret));
        var body = input.Body ?? string.Empty;
        var query = ParseQuery(input.Query);
        var bodyHash = OpenPlatformSignatureAlgorithm.ComputeBodyHash(Encoding.UTF8.GetBytes(body));
        var canonical = OpenPlatformSignatureAlgorithm.BuildCanonicalString(
            method,
            path,
            query,
            bodyHash,
            clientId,
            timestamp,
            nonce);

        return Task.FromResult(new OpenPlatformSignatureDebugResultDto
        {
            Signature = OpenPlatformSignatureAlgorithm.ComputeSignature(appSecret, canonical)
        });
    }

    private static string NormalizeRequired(string? value, string parameterName)
    {
        if (value.IsNullOrWhiteSpace())
        {
            throw new AbpValidationException($"{parameterName} is required.");
        }

        return value.Trim();
    }

    private static string NormalizePath(string? path)
    {
        var normalizedPath = NormalizeRequired(path, nameof(path));
        if (!normalizedPath.StartsWith('/'))
        {
            normalizedPath = "/" + normalizedPath;
        }

        return normalizedPath;
    }

    private static IEnumerable<KeyValuePair<string, IEnumerable<string?>>> ParseQuery(string? query)
    {
        if (query.IsNullOrWhiteSpace())
        {
            return [];
        }

        var normalized = query!;
        if (normalized.StartsWith('?'))
        {
            normalized = normalized[1..];
        }

        if (normalized.IsNullOrWhiteSpace())
        {
            return [];
        }

        var items = new List<KeyValuePair<string, string?>>();
        foreach (var segment in normalized.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = segment.IndexOf('=');
            if (separatorIndex < 0)
            {
                items.Add(new KeyValuePair<string, string?>(Uri.UnescapeDataString(segment), string.Empty));
                continue;
            }

            var key = Uri.UnescapeDataString(segment[..separatorIndex]);
            var value = Uri.UnescapeDataString(segment[(separatorIndex + 1)..]);
            items.Add(new KeyValuePair<string, string?>(key, value));
        }

        return items
            .GroupBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => new KeyValuePair<string, IEnumerable<string?>>(x.Key, x.Select(v => v.Value).ToArray()))
            .ToArray();
    }
}

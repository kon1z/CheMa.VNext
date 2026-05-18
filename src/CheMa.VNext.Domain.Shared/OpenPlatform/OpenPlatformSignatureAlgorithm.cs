using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CheMa.VNext.OpenPlatform;

public static class OpenPlatformSignatureAlgorithm
{
    public static string ComputeBodyHash(byte[] bodyBytes)
    {
        return Convert.ToHexString(SHA256.HashData(bodyBytes)).ToLowerInvariant();
    }

    public static string BuildCanonicalString(
        string method,
        string path,
        IEnumerable<KeyValuePair<string, IEnumerable<string?>>> query,
        string bodyHash,
        string clientId,
        string timestamp,
        string nonce)
    {
        var sortedQuery = string.Join(
            "&",
            query
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .SelectMany(x => x.Value.Select(value => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(value ?? string.Empty)}")));

        return string.Join(
            '\n',
            method.ToUpperInvariant(),
            path,
            sortedQuery,
            bodyHash,
            clientId,
            timestamp,
            nonce);
    }

    public static string ComputeSignature(string secret, string canonical)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var bytes = Encoding.UTF8.GetBytes(canonical);
        return Convert.ToBase64String(hmac.ComputeHash(bytes));
    }

    public static bool FixedTimeEquals(string actual, string expected)
    {
        var actualBytes = Encoding.UTF8.GetBytes(actual);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        return CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Volo.Abp.Domain.Repositories;

namespace CheMa.VNext.OpenPlatform;

public class OpenPlatformSignatureService : IOpenPlatformSignatureService
{
    private readonly IRepository<OpenApp, Guid> _openAppRepository;
    private readonly OpenAppManager _openAppManager;
    private readonly IOpenPlatformNonceStore _nonceStore;
    private readonly IOptions<OpenPlatformOptions> _options;

    public OpenPlatformSignatureService(
        IRepository<OpenApp, Guid> openAppRepository,
        OpenAppManager openAppManager,
        IOpenPlatformNonceStore nonceStore,
        IOptions<OpenPlatformOptions> options)
    {
        _openAppRepository = openAppRepository;
        _openAppManager = openAppManager;
        _nonceStore = nonceStore;
        _options = options;
    }

    public async Task<OpenPlatformValidationResult> ValidateAsync(HttpContext httpContext)
    {
        var clientId = GetRequiredHeader(httpContext, OpenPlatformRequestHeaders.ClientId, OpenPlatformErrorCodes.MissingClientId);
        var timestampText = GetRequiredHeader(httpContext, OpenPlatformRequestHeaders.Timestamp, OpenPlatformErrorCodes.MissingTimestamp);
        var nonce = GetRequiredHeader(httpContext, OpenPlatformRequestHeaders.Nonce, OpenPlatformErrorCodes.MissingNonce);
        var signature = GetRequiredHeader(httpContext, OpenPlatformRequestHeaders.Signature, OpenPlatformErrorCodes.MissingSignature);
        var signVersion = httpContext.Request.Headers[OpenPlatformRequestHeaders.SignVersion].FirstOrDefault() ?? _options.Value.Signature.SignVersion;

        if (!long.TryParse(timestampText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var timestamp))
        {
            throw CreateException(OpenPlatformErrorCodes.TimestampExpired, "Invalid timestamp.");
        }

        var openApp = await _openAppRepository.FirstOrDefaultAsync(x => x.ClientId == clientId);
        if (openApp is null)
        {
            throw CreateException(OpenPlatformErrorCodes.InvalidClient, "Invalid client id.");
        }

        if (openApp.Status != OpenAppStatus.Enabled)
        {
            throw CreateException(OpenPlatformErrorCodes.AppDisabled, "Open app is disabled.");
        }

        var now = DateTimeOffset.UtcNow;
        var requestTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        if (Math.Abs((now - requestTime).TotalSeconds) > _options.Value.Signature.AllowedTimestampSkewSeconds)
        {
            throw CreateException(OpenPlatformErrorCodes.TimestampExpired, "Timestamp expired.");
        }

        if (!openApp.IsAvailable(now.UtcDateTime))
        {
            throw CreateException(OpenPlatformErrorCodes.AppExpired, "Open app is expired.");
        }

        if (!IsIpAllowed(openApp.AllowedIpRanges, httpContext.Connection.RemoteIpAddress))
        {
            throw CreateException(OpenPlatformErrorCodes.IpNotAllowed, "IP address is not allowed.");
        }

        var bodyHash = await ComputeBodyHashAsync(httpContext.Request);
        var canonical = OpenPlatformSignatureAlgorithm.BuildCanonicalString(
            httpContext.Request.Method,
            httpContext.Request.Path.Value ?? string.Empty,
            httpContext.Request.Query.Select(x => new KeyValuePair<string, IEnumerable<string?>>(x.Key, x.Value.AsEnumerable())),
            bodyHash,
            clientId,
            timestampText,
            nonce);
        var secret = _openAppManager.UnprotectSecret(openApp);
        var expectedSignature = OpenPlatformSignatureAlgorithm.ComputeSignature(secret, canonical);
        if (!OpenPlatformSignatureAlgorithm.FixedTimeEquals(signature, expectedSignature))
        {
            throw CreateException(OpenPlatformErrorCodes.InvalidSignature, "Invalid signature.");
        }

        var nonceAccepted = await _nonceStore.TryUseAsync(
            clientId,
            nonce,
            TimeSpan.FromSeconds(_options.Value.Signature.AllowedTimestampSkewSeconds));

        if (!nonceAccepted)
        {
            throw CreateException(OpenPlatformErrorCodes.NonceReused, "Nonce has been used.");
        }

        return new OpenPlatformValidationResult
        {
            OpenApp = openApp,
            ClientId = clientId,
            Timestamp = timestamp,
            Nonce = nonce,
            SignVersion = signVersion
        };
    }

    private static string GetRequiredHeader(HttpContext httpContext, string headerName, string errorCode)
    {
        var value = httpContext.Request.Headers[headerName].FirstOrDefault();
        if (value.IsNullOrWhiteSpace())
        {
            throw CreateException(errorCode, $"Missing header: {headerName}.");
        }

        return value!;
    }

    private static async Task<string> ComputeBodyHashAsync(HttpRequest request)
    {
        request.EnableBuffering();
        request.Body.Position = 0;

        using var stream = new MemoryStream();
        await request.Body.CopyToAsync(stream);
        request.Body.Position = 0;
        return OpenPlatformSignatureAlgorithm.ComputeBodyHash(stream.ToArray());
    }

    private static bool IsIpAllowed(string? allowedIpRanges, IPAddress? remoteIpAddress)
    {
        if (allowedIpRanges.IsNullOrWhiteSpace())
        {
            return true;
        }

        if (remoteIpAddress is null)
        {
            return false;
        }

        var candidates = allowedIpRanges!
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return candidates.Any(candidate => string.Equals(candidate, remoteIpAddress.ToString(), StringComparison.OrdinalIgnoreCase));
    }

    private static OpenPlatformException CreateException(string errorCode, string message)
    {
        return new OpenPlatformException(errorCode, message, StatusCodes.Status401Unauthorized);
    }
}

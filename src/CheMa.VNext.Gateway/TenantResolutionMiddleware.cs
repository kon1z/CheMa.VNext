using Microsoft.Extensions.Options;

namespace CheMa.VNext;

public class TenantResolutionMiddleware(RequestDelegate next, IOptionsMonitor<GatewayTenancyOptions> tenancyOptionsMonitor, IOptionsMonitor<GatewayCanaryOptions> canaryOptionsMonitor)
{
    private readonly RequestDelegate next = next;
    private readonly IOptionsMonitor<GatewayTenancyOptions> tenancyOptionsMonitor = tenancyOptionsMonitor;
    private readonly IOptionsMonitor<GatewayCanaryOptions> canaryOptionsMonitor = canaryOptionsMonitor;

    public async Task InvokeAsync(HttpContext context)
    {
        var tenancyOptions = tenancyOptionsMonitor.CurrentValue;
        var canaryOptions = canaryOptionsMonitor.CurrentValue;
        var tenantResolution = ResolveTenant(context, tenancyOptions);

        if (!string.IsNullOrWhiteSpace(tenantResolution.TenantId))
        {
            context.Items[TenantContext.ItemKey] = tenantResolution;
            context.Request.Headers[tenancyOptions.HeaderName] = tenantResolution.TenantId;
            context.Request.Headers[tenancyOptions.TenantIdHeaderName] = tenantResolution.TenantId;
            context.Request.Headers[tenancyOptions.TenantSourceHeaderName] = tenantResolution.Source;

            if (IsCanaryTenant(tenantResolution.TenantId, canaryOptions))
            {
                context.Request.Headers[canaryOptions.HeaderName] = bool.TrueString;
            }
        }

        await next(context);
    }

    private static bool IsCanaryTenant(string tenantId, GatewayCanaryOptions options) => options.TenantWhitelist.Any(item => string.Equals(item, tenantId, StringComparison.OrdinalIgnoreCase));

    private static TenantResolutionResult ResolveTenant(HttpContext context, GatewayTenancyOptions options)
    {
        if (context.Request.Headers.TryGetValue(options.HeaderName, out var headerValue)
            && !string.IsNullOrWhiteSpace(headerValue))
        {
            return new TenantResolutionResult(headerValue.ToString(), "header");
        }

        var hostTenant = ResolveTenantFromHost(context.Request.Host.Host);
        if (!string.IsNullOrWhiteSpace(hostTenant))
        {
            return new TenantResolutionResult(hostTenant, "subdomain");
        }

        var pathTenant = ResolveTenantFromPath(context.Request.Path, options.TenantSegmentName);
        if (!string.IsNullOrWhiteSpace(pathTenant))
        {
            return new TenantResolutionResult(pathTenant, "path");
        }

        return TenantResolutionResult.Empty;
    }

    private static string? ResolveTenantFromHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return null;
        }

        var segments = host.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length < 3)
        {
            return null;
        }

        return segments[0];
    }

    private static string? ResolveTenantFromPath(PathString path, string tenantSegmentName)
    {
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments is null || segments.Length < 2)
        {
            return null;
        }

        for (var index = 0; index < segments.Length - 1; index++)
        {
            if (string.Equals(segments[index], tenantSegmentName, StringComparison.OrdinalIgnoreCase))
            {
                return segments[index + 1];
            }
        }

        return null;
    }
}

public record TenantResolutionResult(string? TenantId, string? Source)
{
    public static TenantResolutionResult Empty { get; } = new(null, null);
}

public static class TenantContext
{
    public const string ItemKey = "GatewayTenantResolution";
}

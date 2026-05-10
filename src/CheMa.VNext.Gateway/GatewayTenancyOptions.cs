namespace CheMa.VNext;

public class GatewayTenancyOptions
{
    public const string SectionName = "Gateway:Tenancy";

    public string HeaderName { get; set; } = "__tenant";

    public string TenantIdHeaderName { get; set; } = "X-Tenant-Id";

    public string TenantSourceHeaderName { get; set; } = "X-Tenant-Source";

    public string TenantSegmentName { get; set; } = "tenants";
}

namespace CheMa.VNext;

public class GatewayCanaryOptions
{
    public const string SectionName = "Gateway:Canary";

    public string HeaderName { get; set; } = "X-Gateway-Canary";

    public List<string> TenantWhitelist { get; set; } = [];
}

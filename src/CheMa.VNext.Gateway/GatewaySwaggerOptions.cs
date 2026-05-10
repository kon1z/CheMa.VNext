namespace CheMa.VNext;

public class GatewaySwaggerOptions
{
    public const string SectionName = "Gateway:Swagger";

    public string RoutePrefix { get; set; } = "swagger";

    public List<GatewaySwaggerDocument> Documents { get; set; } = [];

    public Dictionary<string, GatewaySwaggerDownstream> Downstreams { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class GatewaySwaggerDocument
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string JsonUrl { get; set; } = string.Empty;
}

public class GatewaySwaggerDownstream
{
    public string DisplayName { get; set; } = string.Empty;

    public string JsonUrl { get; set; } = string.Empty;
}

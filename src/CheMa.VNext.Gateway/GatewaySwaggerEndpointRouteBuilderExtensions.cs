using System.Net;
using Microsoft.Extensions.Options;

namespace CheMa.VNext;

public static class GatewaySwaggerEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapGatewaySwaggerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/swagger", async (HttpContext context, IOptionsMonitor<GatewaySwaggerOptions> optionsMonitor) =>
        {
            var options = optionsMonitor.CurrentValue;
            context.Response.ContentType = "text/html; charset=utf-8";

            var items = string.Join(Environment.NewLine, options.Documents.Select(document =>
                $"<li><a href=\"{WebUtility.HtmlEncode(document.JsonUrl)}\">{WebUtility.HtmlEncode(document.DisplayName)}</a></li>"));

            await context.Response.WriteAsync($"""
<!doctype html>
<html lang="zh-CN">
<head>
    <meta charset="utf-8" />
    <title>Gateway Swagger</title>
</head>
<body>
    <h1>Gateway Swagger</h1>
    <ul>
        {items}
    </ul>
</body>
</html>
""");
        }).AllowAnonymous();

        endpoints.MapGet("/swagger/documents", (IOptionsMonitor<GatewaySwaggerOptions> optionsMonitor) =>
        {
            return Results.Ok(optionsMonitor.CurrentValue.Documents);
        }).AllowAnonymous();

        endpoints.MapGet("/swagger/docs/{documentName}", async Task<IResult> (
            string documentName,
            IHttpClientFactory httpClientFactory,
            IOptionsMonitor<GatewaySwaggerOptions> optionsMonitor,
            CancellationToken cancellationToken) =>
        {
            var options = optionsMonitor.CurrentValue;
            if (!options.Downstreams.TryGetValue(documentName, out var downstream)
                || string.IsNullOrWhiteSpace(downstream.JsonUrl))
            {
                return Results.NotFound();
            }

            var client = httpClientFactory.CreateClient();
            using var response = await client.GetAsync(downstream.JsonUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Results.StatusCode((int)response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return Results.Text(content, "application/json");
        }).AllowAnonymous();

        return endpoints;
    }
}

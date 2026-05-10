using CheMa.VNext;
using CheMa.VNext.ReverseProxy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Yarp.ReverseProxy.Transforms.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Host.UseAgileConfig();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ITransformProvider, TenantHeaderTransformProvider>();
builder.Services.Configure<GatewayTenancyOptions>(builder.Configuration.GetSection(GatewayTenancyOptions.SectionName));
builder.Services.Configure<GatewayCanaryOptions>(builder.Configuration.GetSection(GatewayCanaryOptions.SectionName));
builder.Services.Configure<GatewaySwaggerOptions>(builder.Configuration.GetSection(GatewaySwaggerOptions.SectionName));

var authAuthority = builder.Configuration["AuthServer:Authority"];
var requireHttpsMetadata = builder.Configuration.GetValue("AuthServer:RequireHttpsMetadata", false);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authAuthority;
        options.RequireHttpsMetadata = requireHttpsMetadata;
        options.Audience = builder.Configuration["AuthServer:Audience"] ?? "VNext";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("gateway-authenticated", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});

app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();
app.MapGatewaySwaggerEndpoints();
app.MapReverseProxy();
app.MapDefaultEndpoints();

await app.RunAsync();

using System;
using CheMa.VNext.MaiHong;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Modularity;

namespace CheMa.VNext;

/// <summary>
/// 基础设施模块，承载外部系统集成、第三方 HTTP 客户端等基础设施实现。
/// </summary>
[DependsOn(
    typeof(VNextDomainModule)
)]
public class VNextInfrastructureModule : AbpModule
{
    /// <inheritdoc />
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<MaiHongGatewayOptions>(
            configuration.GetSection(MaiHongGatewayOptions.SectionName));

        context.Services.AddHttpClient<IMaiHongGateway, MaiHongGateway>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<MaiHongGatewayOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
    }
}

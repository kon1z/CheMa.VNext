using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CheMa.VNext.ExternalServices;

/// <summary>
/// 外部服务 HTTP 客户端构建器扩展。
/// </summary>
public static class ExternalServiceHttpClientBuilderExtensions
{
    /// <summary>
    /// 为外部服务 HTTP 客户端添加 HTTP 2xx 业务错误转换能力。
    /// </summary>
    public static IHttpClientBuilder AddExternalServiceBusinessExceptionHandling<TDetector>(
        this IHttpClientBuilder builder,
        string serviceName)
        where TDetector : class, IExternalServiceBusinessErrorDetector
    {
        builder.Services.AddTransient<TDetector>();

        builder.AddHttpMessageHandler(serviceProvider =>
            new ExternalServiceBusinessExceptionHandler(
                serviceName,
                serviceProvider.GetRequiredService<TDetector>(),
                serviceProvider.GetRequiredService<ILogger<ExternalServiceBusinessExceptionHandler>>()));

        return builder;
    }
}

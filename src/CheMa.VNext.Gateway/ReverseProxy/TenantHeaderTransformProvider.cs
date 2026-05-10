using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace CheMa.VNext.ReverseProxy;

public class TenantHeaderTransformProvider : ITransformProvider
{
    public void ValidateCluster(TransformClusterValidationContext context)
    {
    }

    public void ValidateRoute(TransformRouteValidationContext context)
    {
    }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(transformContext =>
        {
            if (transformContext.HttpContext.Items.TryGetValue(TenantContext.ItemKey, out var tenantResolution)
                && tenantResolution is TenantResolutionResult result
                && !string.IsNullOrWhiteSpace(result.TenantId))
            {
                transformContext.ProxyRequest.Headers.Remove("X-Tenant-Id");
                transformContext.ProxyRequest.Headers.Remove("X-Tenant-Source");
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-Tenant-Id", result.TenantId);
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-Tenant-Source", result.Source);
            }

            return ValueTask.CompletedTask;
        });
    }
}

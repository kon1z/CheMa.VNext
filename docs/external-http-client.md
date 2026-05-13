# 外部非 ABP 第三方 HTTP Client 调用规范示例

本文档说明项目中调用外部非 ABP 第三方服务时的推荐分层和示例代码。

## 适用场景

- 调用第三方平台发送设备指令。
- 查询第三方平台设备状态。
- 第三方接口不是 ABP Remote Service。
- 需要隔离第三方请求、响应、鉴权、签名、错误码等外部协议细节。

## 分层原则

```text
ApplicationService / DomainService
        ↓
IExternalDeviceGateway              // Domain 层抽象
        ↓
ExternalDeviceGateway               // Infrastructure 层实现
        ↓
HttpClient
        ↓
第三方非 ABP HTTP API
```

关键约束：

- 不在 Controller 或 ApplicationService 中直接 `new HttpClient()`。
- 不让第三方 Request/Response DTO 向领域层、应用契约层扩散。
- 业务层依赖 `IExternalDeviceGateway` 抽象。
- HTTP 地址、超时、AppId、AppSecret 等通过配置注入。
- 日志使用结构化模板，不记录 Token、密钥、签名原文等敏感信息。

## 示例文件

### Domain 层抽象与内部模型

```text
src/CheMa.VNext.Domain/Samples/ExternalDevices/IExternalDeviceGateway.cs
src/CheMa.VNext.Domain/Samples/ExternalDevices/ExternalDeviceCommandRequest.cs
src/CheMa.VNext.Domain/Samples/ExternalDevices/ExternalDeviceCommandResult.cs
src/CheMa.VNext.Domain/Samples/ExternalDevices/ExternalDeviceStatusResult.cs
```

这些类型代表系统内部可理解的模型，不绑定第三方字段命名。

### Infrastructure 层示例实现

```text
src/CheMa.VNext.Infrastructure/VNextInfrastructureModule.cs
src/CheMa.VNext.Infrastructure/Samples/ExternalDevices/ExternalDeviceGateway.cs
src/CheMa.VNext.Infrastructure/Samples/ExternalDevices/ExternalDeviceGatewayOptions.cs
src/CheMa.VNext.Infrastructure/Samples/ExternalDevices/ThirdPartyExternalDeviceModels.cs
```

`ThirdPartyExternalDeviceModels.cs` 中的类型是 `internal`，只允许网关实现内部使用。

`CheMa.VNext.Infrastructure` 依赖 `CheMa.VNext.Domain`，负责承载第三方 HTTP Client、外部系统适配器、防腐层实现等基础设施代码。`CheMa.VNext.Application` 只依赖 `IExternalDeviceGateway` 抽象，不依赖 `HttpClient` 或第三方 DTO。

## 配置

Host 示例配置位于：

```text
src/CheMa.VNext.HttpApi.Host/appsettings.json
```

配置节：

```json
{
  "ExternalDeviceGateway": {
    "BaseUrl": "https://example.invalid",
    "AppId": "sample-app-id",
    "AppSecret": "sample-app-secret-use-env-or-agileconfig-in-real-env",
    "TimeoutSeconds": 10
  }
}
```

生产环境不要提交真实 `AppSecret`，应使用环境变量、AgileConfig 或部署系统注入。

## DI 注册

注册位置：

```text
src/CheMa.VNext.Infrastructure/VNextInfrastructureModule.cs
```

注册内容包括：

- `ExternalDeviceGatewayOptions` 配置绑定。
- `AddHttpClient<IExternalDeviceGateway, ExternalDeviceGateway>()` typed client 注册。
- `BaseAddress` 和 `Timeout` 配置。

示例：

```csharp
Configure<ExternalDeviceGatewayOptions>(
    configuration.GetSection(ExternalDeviceGatewayOptions.SectionName));

context.Services.AddHttpClient<IExternalDeviceGateway, ExternalDeviceGateway>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<IOptions<ExternalDeviceGatewayOptions>>()
        .Value;

    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});
```

## 模块依赖

需要在最终宿主模块中依赖 Infrastructure 模块，例如 HTTP API Host：

```csharp
[DependsOn(
    typeof(VNextApplicationModule),
    typeof(VNextInfrastructureModule),
    typeof(VNextEntityFrameworkCoreModule)
)]
public class VNextHttpApiHostModule : AbpModule
{
}
```

同时宿主项目需要引用：

```xml
<ProjectReference Include="..\CheMa.VNext.Infrastructure\CheMa.VNext.Infrastructure.csproj" />
```

## 在应用服务中使用

```csharp
public class DeviceCommandAppService : VNextAppService
{
    private readonly IExternalDeviceGateway externalDeviceGateway;

    public DeviceCommandAppService(IExternalDeviceGateway externalDeviceGateway)
    {
        this.externalDeviceGateway = externalDeviceGateway;
    }

    public async Task SendAsync(string deviceCode, string commandCode)
    {
        var result = await externalDeviceGateway.SendCommandAsync(new ExternalDeviceCommandRequest
        {
            DeviceCode = deviceCode,
            CommandCode = commandCode,
            Parameters = new Dictionary<string, object?>()
        });

        if (!result.Success)
        {
            // 根据业务需要记录失败流水、返回错误 DTO 或触发重试。
        }
    }
}
```

## 错误处理建议

发送指令类操作通常建议返回内部 Result：

- 便于应用服务记录指令流水。
- 便于区分 HTTP 失败、第三方业务失败、超时。
- 避免第三方异常直接影响业务用例边界。

查询类操作可根据业务选择：

- 返回 Result。
- 抛出业务异常或基础设施异常，再由应用服务转换。

当前示例中：

- `SendCommandAsync` 返回 `ExternalDeviceCommandResult`。
- `QueryStatusAsync` 对不可恢复失败抛出异常。

## 生产化建议

后续真实接入第三方平台时建议补充：

- 第三方错误码到内部错误码的映射。
- 指令发送流水表，记录 Pending/Sent/Failed 状态。
- 幂等键或业务请求编号。
- Polly/Resilience 重试、熔断、超时策略。
- 第三方健康检查。
- 集成测试或基于 `HttpMessageHandler` 的单元测试。
- 对敏感配置使用环境变量或 AgileConfig 注入。

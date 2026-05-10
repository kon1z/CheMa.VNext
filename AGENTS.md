# Repository Guidelines

## System Architecture

### 概述
CheMa.VNext 是基于 .NET / ABP 的分层应用，按 Aspire 分布式交付形态组织本地编排与运行。`AppHost` 负责组合运行时依赖与应用项目，业务系统仍遵循 ABP 分层边界。

### 详述
- `CheMa.VNext.AppHost` 编排 PostgreSQL、Redis、OpenObserve、OpenTelemetry Collector、DbMigrator、HttpApi.Host、BackgroundWorker 与 Blazor。
- `CheMa.VNext.ServiceDefaults` 提供横切能力：服务发现、HTTP resilience、健康检查、OpenTelemetry 日志/指标/追踪。
- `CheMa.VNext.HttpApi.Host` 暴露后端 API；`CheMa.VNext.Blazor` / `Blazor.Client` 提供 UI；`BackgroundWorker` 执行后台任务。
- `CheMa.VNext.DbMigrator` 负责数据库迁移与种子数据，应用服务启动前应先完成迁移。

### 示例
```bash
dotnet run --project src/CheMa.VNext.AppHost
dotnet run --project src/CheMa.VNext.DbMigrator
dotnet run --project src/CheMa.VNext.HttpApi.Host
```

## Project Architecture

### 概述
项目采用 ABP 分层架构。新增功能必须按职责放置，避免跨层直接依赖或把业务规则下沉到 API/UI 层。

### 详述
- `src/CheMa.VNext.Domain.Shared`：共享常量、枚举、错误码、本地化资源。
- `src/CheMa.VNext.Domain`：实体、聚合根、领域服务、仓储接口、领域事件与核心业务规则。
- `src/CheMa.VNext.Application.Contracts`：应用服务接口、DTO、权限定义、远程服务契约。
- `src/CheMa.VNext.Application`：应用服务实现、用例编排、DTO 映射、权限检查。
- `src/CheMa.VNext.EntityFrameworkCore`：DbContext、EF Core 配置、迁移相关持久化实现。
- `src/CheMa.VNext.HttpApi` / `HttpApi.Host`：HTTP API 暴露、宿主配置、中间件、认证授权入口。
- `src/CheMa.VNext.HttpApi.Client`：远程 API 客户端代理。
- `src/CheMa.VNext.Blazor` / `Blazor.Client`：前端页面、组件和客户端配置。
- `src/CheMa.VNext.BackgroundJobs` / `BackgroundWorker`：后台作业定义与独立 Worker 宿主。
- `test/`：按生产项目匹配测试，公共测试基础设施放在 `CheMa.VNext.TestBase`。

### 示例
新增订单功能时：实体放 `Domain`，`OrderDto` 与 `IOrderAppService` 放 `Application.Contracts`，`OrderAppService` 放 `Application`，EF 映射放 `EntityFrameworkCore`，接口暴露放 `HttpApi`，测试放对应 `*.Tests` 项目。

## Logging Architecture

### 概述
日志采集基于 `Microsoft.Extensions.Logging`，通过 OpenTelemetry 统一导出。当前 Aspire 编排中使用 OpenTelemetry Collector 接收 OTLP，并转发到 OpenObserve 进行观察、存储与查询。

### 详述
- 应用侧在 `ServiceDefaults` 中统一配置 OpenTelemetry 日志、指标和追踪。
- 仅当 `OTEL_EXPORTER_OTLP_ENDPOINT` 存在时启用 OTLP 导出。
- 日志导出启用 `IncludeFormattedMessage`、`IncludeScopes`、`ParseStateValues`，必须优先写结构化日志模板。
- 资源属性包含 `service.namespace=CheMa.VNext`、`service.name`、`service.version`、`service.instance.id`、`deployment.environment.name`。
- `/health` 与 `/alive` 请求不应污染追踪数据。
- `HttpApi.Host/appsettings.json` 中 `Logging:Sql:SlowThresholdMs = 500` 表示 SQL 慢日志阈值为 500ms。
- OpenObserve 组织、stream、认证方式由 Collector 配置管理；生产值为 `[待确认]`。

### 示例
```bash
set OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
```

## Log Structure

### 概述
日志必须可查询、可关联、可聚合。优先使用 OTEL 语义字段和稳定业务字段，避免仅写拼接字符串。

### 详述
- 级别标准：`Trace` 用于极细粒度诊断；`Debug` 用于开发排查；`Information` 记录关键业务流程；`Warning` 记录可恢复异常或降级；`Error` 记录失败请求、作业失败、外部依赖失败；`Critical` 记录服务不可用或数据一致性风险。
- 推荐字段：`service.name`、`service.namespace`、`deployment.environment.name`、`trace_id`、`span_id`、`user.id`、`tenant.id`、`request.path`、`http.method`、`http.status_code`、`db.system`、`db.statement`、`elapsed_ms`、`exception.type`。
- 业务字段应稳定命名，例如 `OrderId`、`JobId`、`CorrelationId`、`ElapsedMs`。
- 异常日志必须传入异常对象，不只记录 `ex.Message`。
- 不把密码、Token、证书私钥等不可恢复秘密作为常规日志字段。

### 示例
```csharp
logger.LogInformation("Order {OrderId} processed in {ElapsedMs} ms", orderId, elapsedMs);
logger.LogError(ex, "Background job {JobId} failed", jobId);
```

## Coding Style & Naming Conventions

### 概述
遵守根目录 `.editorconfig`、`.prettierrc` 与 ABP 约定。代码应保持分层清晰、可测试、可维护。

### 详述
- C# 使用 4 空格缩进、CRLF、UTF-8；XML/MSBuild 文件使用 2 空格缩进。
- `using` 中 `System` 命名空间优先；移除未使用 using。
- 公共类型和成员使用 PascalCase；局部变量和参数使用 camelCase；接口以 `I` 开头；异步方法以 `Async` 结尾。
- 私有字段遵循 `.editorconfig` 的 camelCase 规则；新增代码应与邻近代码风格保持一致。
- 优先使用对象初始化器、集合初始化器、空传播、模式匹配和文件范围命名空间。
- 不使用 top-level statements 编写业务项目入口之外的代码。
- 应用服务命名为 `XxxAppService`，DTO 命名为 `XxxDto` / `CreateXxxDto` / `UpdateXxxDto`，领域服务命名为 `XxxManager` 或 `XxxDomainService`。
- 公共 API、DTO、领域模型、复杂业务方法应提供 XML 注释；当前编译规则禁用了 `CS1591`，但团队规范要求补齐注释。

### 示例
```csharp
public interface IOrderAppService
{
    Task<OrderDto> GetAsync(Guid id);
}
```

## Build, Test, and Development Commands

### 概述
常用命令在仓库根目录执行。

### 详述与示例
- `dotnet restore CheMa.VNext.sln`：还原 NuGet 包。
- `dotnet build CheMa.VNext.sln`：编译全部项目。
- `dotnet test CheMa.VNext.sln`：运行 xUnit 测试。
- `dotnet run --project src/CheMa.VNext.DbMigrator`：执行迁移与种子数据。
- `dotnet run --project src/CheMa.VNext.HttpApi.Host`：启动 API Host。
- `dotnet run --project src/CheMa.VNext.Blazor`：启动 Blazor UI。
- `abp install-libs`：还原 ABP 客户端库。

## Testing Guidelines

### 概述
测试使用 xUnit 与 ABP test modules。测试应靠近被测层，避免只通过 UI 或 Host 做所有验证。

### 详述
- 应用服务测试放 `test/CheMa.VNext.Application.Tests`。
- 领域规则测试放 `test/CheMa.VNext.Domain.Tests`。
- EF Core 持久化测试放 `test/CheMa.VNext.EntityFrameworkCore.Tests`。
- 测试类以被测对象命名，测试方法使用行为化名称，如 `Should_Create_Order_When_Input_Is_Valid`。

### 示例
```bash
dotnet test CheMa.VNext.sln
```

## Commit & Pull Request Guidelines

### 概述
提交信息保持简洁、可追踪。仓库历史采用 Conventional Commits 风格。

### 详述
- 提交示例：`feat(background): add order sync job`、`fix(apphost): correct otlp endpoint`、`style: apply editorconfig`。
- PR 应包含变更摘要、关联任务或 Issue、迁移/配置说明、测试结果；UI 变更应附截图。
- 涉及公共契约、数据库迁移、配置项或日志字段变更时，在 PR 中明确影响范围。

## Security & Configuration Tips

### 概述
配置应环境化，秘密不进入 Git。即使在内网环境，也不要把高风险秘密设计为常规日志字段。

### 详述
- 不提交真实连接串、证书、Token、OpenObserve 认证头；本地开发配置仅使用 `appsettings.json`、`appsettings.Development.json`、环境变量和 AgileConfig。
- 审查 `appsettings.json`、`appsettings.Development.json` 中 Redis、OpenIddict、OpenTelemetry、数据库与 CORS 配置。
- 生产环境 OpenObserve URL、organization、stream、认证方式为 `[待确认]`，应通过环境变量或部署系统注入。

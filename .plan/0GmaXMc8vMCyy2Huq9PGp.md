# CheMa.VNext.BackgroundWorker 多租户与用户上下文规范化修复计划

## 1. 摘要与范围

### 已确认事实

- `src/CheMa.VNext.BackgroundWorker` 是独立 `Exe`，使用 `Host.CreateApplicationBuilder(args)`，不是 ASP.NET Core HTTP Host。
- `BackgroundWorkerHostedService` 当前通过 `IAbpApplicationWithExternalServiceProvider.InitializeAsync(_serviceProvider)` 手动初始化 ABP。
- `HttpApi.Host` 使用的是标准 Web 模式：`builder.Host.UseAutofac()`、`await builder.AddApplicationAsync<VNextHttpApiHostModule>()`、`await app.InitializeApplicationAsync()`。
- `HttpApi.Host` 管道中存在 `UseAuthentication()`、`UseMultiTenancy()`、`UseUnitOfWork()`、`UseAuthorization()`、`UseAuditing()`，但这些 HTTP 中间件不会自动作用于独立后台进程。
- `VNextBackgroundJobsModule.OnApplicationInitializationAsync` 注册 `SampleQuartzBackgroundWorker`：`context.AddBackgroundWorkerAsync<SampleQuartzBackgroundWorker>()`。
- 多租户已启用：`MultiTenancyConsts.IsEnabled = true`，并在 `VNextDomainModule` 中配置 `AbpMultiTenancyOptions.IsEnabled`。
- 当前后台 worker 只依赖 `VNextApplicationContractsModule`，未依赖 `VNextApplicationModule`；如果后台要复用应用层实现，仅 contracts 不足。
- `HttpApi.Host` 已禁用内置后台作业执行：`AbpBackgroundJobOptions.IsJobExecutionEnabled = false`，说明后台执行被设计为独立 worker 承担。
- 用户已确认：启动阶段 NRE 接近 ABP 初始化；后台宿主可改为 ABP 标准方式；任务上下文以“指定租户”为主，并兼容 `TenantId = null` 的 Host 级任务；多来源包括 Quartz、ABP Job、HTTP 触发、MQ；用户上下文只要求审计/业务操作人 ID 延续，不要求模拟完整登录身份；后台应绕过 `[Authorize]` 的 HTTP 应用服务入口；事件边界以分布式事件/MQ 为主。

### 本次计划范围

1. 修复/规范 `CheMa.VNext.BackgroundWorker` 的 ABP 宿主初始化方式，消除初始化阶段 NRE 风险。
2. 建立统一的后台执行上下文模型，显式传递 `TenantId` 与 `OperatorUserId`/审计用户 ID。
3. 统一 Quartz、ABP BackgroundJob、HTTP 触发、分布式事件/MQ 的上下文传递与进入方式。
4. 避免后台任务依赖 `HttpContext`、`ICurrentUser` 必然有值、或 HTTP `[Authorize]` 入口。
5. 给出验证方案，覆盖启动、多租户过滤、审计用户延续、Host 级任务。

不在本次计划范围：直接实现完整业务订单逻辑、重新设计权限体系、把后台任务伪装为完整登录用户。

## 2. 根因分析

### 2.1 `ICurrentTenant`/`ICurrentUser` 与 `HttpContext` 的关系

- ABP 的 `ICurrentTenant` 是 ambient context，不等同于 `HttpContext`，但在 HTTP 请求中通常由 `UseMultiTenancy()` 根据域名、Header、Cookie、Route 等解析并设置。
- ABP 的 `ICurrentUser` 通常来自当前 `ClaimsPrincipal`，在 HTTP 请求中由认证中间件、OpenIddict validation、动态 claims 等建立。
- 独立 BackgroundWorker、Quartz、ABP Job、MQ handler 都不处在 HTTP 请求管道中，因此不会自动执行：
  - `UseAuthentication()`
  - `UseMultiTenancy()`
  - `UseDynamicClaims()`
  - `UseAuthorization()`
- 所以后台任务里：
  - `ICurrentTenant` 服务本身应可注入，但 `CurrentTenant.Id` 默认可能为空。
  - `ICurrentUser` 服务本身应可注入，但 `CurrentUser.Id` 默认为空，`IsAuthenticated` 通常为 `false`。
  - 如果出现“注入后调用空引用”，要重点排查是否把 `CurrentTenant.Id.Value`、`CurrentUser.Id.Value`、`CurrentUser.UserName!` 等当成必有值，或启动时 DI/ABP 初始化异常导致服务未正确构建。

### 2.2 启动阶段 NRE 的高概率根因

当前 `BackgroundWorker` 使用：

- `builder.Services.AddHostedService<BackgroundWorkerHostedService>()`
- `builder.Services.AddApplication<VNextBackgroundWorkerModule>(options => { options.Services.ReplaceConfiguration(builder.Configuration); options.UseAutofac(); })`
- hosted service 的 `StartAsync` 中手动 `_application.InitializeAsync(_serviceProvider)`

该方式相对脆弱：ABP 初始化被放进普通 hosted service 生命周期中，与 Host 构建、Autofac service provider、其它 hosted service/Quartz 初始化顺序容易产生不一致。用户已确认 NRE 接近 ABP 初始化，且可以改标准方式，因此优先改为 ABP Generic Host 推荐风格。

### 2.3 后台调用 ApplicationService 的架构问题

用户确认后台应绕过 `[Authorize]`，但当前倾向调用 `ApplicationService`。这会产生冲突：

- 面向 HTTP 的 `ApplicationService` 可能包含 `[Authorize]`、依赖 `CurrentUser`、依赖请求上下文、DTO/UI 语义。
- 后台任务是可信执行环境，系统补偿、事件消费、定时任务不应被前端权限拦截。

推荐拆分：

- HTTP `ApplicationService`：负责授权、输入 DTO、用户请求入口。
- 内部业务服务/领域服务：负责实际业务逻辑，可被 HTTP 与后台共同调用。
- 后台 worker/job/handler：进入租户上下文和审计上下文后调用内部服务，而不是直接调用带 `[Authorize]` 的 HTTP 应用服务方法。

## 3. 需求与验收标准

### 功能需求

1. BackgroundWorker 进程能用 ABP 标准 Generic Host 方式启动。
2. 所有后台任务入口必须显式接收或解析执行上下文：
   - `TenantId: Guid?`
   - `OperatorUserId: Guid?`
   - 可选：`CorrelationId`、`Source`、`BusinessKey`
3. 执行任务前必须用 `ICurrentTenant.Change(context.TenantId)` 切换租户；`TenantId = null` 表示 Host 级任务。
4. 对仅审计用户延续的任务，不强制伪造 `ICurrentUser` 登录态；用显式 `OperatorUserId` 作为业务/审计输入。
5. 分布式事件/MQ payload 必须序列化 `TenantId` 与 `OperatorUserId`，不能依赖消费者进程 ambient context。
6. 后台业务逻辑应调用内部服务/领域服务，避免直接调用带 `[Authorize]` 的 HTTP ApplicationService 入口。
7. 所有数据库写操作应在明确的 UOW 内执行。

### 验收标准

- BackgroundWorker 启动不再在 ABP 初始化阶段抛 NRE。
- Quartz worker 能记录当前 `TenantId`，并能在指定租户和 Host 级两种模式下执行。
- ABP Job/MQ handler 能根据 payload 进入正确租户；租户过滤数据隔离正确。
- 用户发起的异步任务能把 `OperatorUserId` 写入业务审计字段或自定义字段。
- 没有代码再假设后台 `CurrentUser.Id` 必然存在。
- 关键路径有单元测试或集成测试覆盖，至少包含：指定租户、Host 级、缺失 OperatorUserId、分布式事件反序列化。

## 4. 架构/设计方向与关键类型

### 4.1 统一后台上下文类型

建议放置位置：`src/CheMa.VNext.BackgroundJobs` 或更通用的 contracts/shared 项目。若 HTTP、MQ、Job payload 都要引用，优先放在 `CheMa.VNext.Application.Contracts` 或一个专门的 shared contracts 项目。

示例类型：

```csharp
public sealed class BackgroundExecutionContextDto
{
    public Guid? TenantId { get; set; }
    public Guid? OperatorUserId { get; set; }
    public string? CorrelationId { get; set; }
    public string? Source { get; set; }
}
```

业务 job/event args 应组合该上下文：

```csharp
public sealed class CompleteOrderInfoEto
{
    public Guid OrderId { get; set; }
    public BackgroundExecutionContextDto ExecutionContext { get; set; } = new();
}
```

### 4.2 后台上下文执行器

新增一个内部服务，集中处理租户切换、UOW、日志作用域与审计用户传递。

建议接口：

```csharp
public interface IBackgroundExecutionContextRunner
{
    Task RunAsync(BackgroundExecutionContextDto context, Func<Task> action);
    Task<TResult> RunAsync<TResult>(BackgroundExecutionContextDto context, Func<Task<TResult>> action);
}
```

实现要点：

- 注入 `ICurrentTenant`。
- 注入 `IUnitOfWorkManager`。
- 可选注入 `ICorrelationIdProvider` 或使用日志 scope。
- 执行时：
  1. `using (_currentTenant.Change(context.TenantId))`
  2. `using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);`
  3. 调用业务 action。
  4. `await uow.CompleteAsync();`
- 不建议默认调用 `ICurrentPrincipalAccessor.Change(...)`，因为用户选择的是“仅审计 ID”，不是模拟登录身份。

### 4.3 审计用户 ID 的处理方式

ABP 内置审计字段通常从 `ICurrentUser.Id` 获取。如果不构造 ClaimsPrincipal，则 ABP 的 `CreatorId`/`LastModifierId` 可能仍为空。用户目标是“审计/业务字段延续”，推荐二选一：

#### 推荐方案 A：业务显式审计字段

在需要延续发起人的业务实体/操作上增加显式字段，例如：

- `OperatorUserId`
- `RequestedByUserId`
- `SourceUserId`

后台内部服务方法显式接收 `operatorUserId` 并赋值。

优点：

- 语义清晰，不伪造登录用户。
- 不依赖 ABP 内部审计机制。
- 适合 MQ/分布式事件、系统补偿任务。

缺点：

- ABP 内置 `CreatorId`/`LastModifierId` 不一定自动填充为原始用户。
- 需要在关键业务实体或日志中增加字段/赋值逻辑。

#### 可选方案 B：只在特定任务中临时构造最小 ClaimsPrincipal

如果某些任务明确要求 ABP 内置审计字段自动使用原用户 ID，可在 runner 中增加可选模式，使用 `ICurrentPrincipalAccessor.Change(principal)` 构造最小 claims：

- `AbpClaimTypes.UserId`
- `AbpClaimTypes.UserName` 可选
- `AbpClaimTypes.TenantId` 当 `TenantId` 非空时设置

优点：

- `ICurrentUser.Id` 和 ABP 审计字段可自动读取。

缺点：

- 容易被误解为“已通过认证/授权”。
- 如果服务里有权限判断，最小 claims 不代表真实权限。
- 用户已选择“仅审计 ID”和“后台绕过授权”，因此不作为默认方案。

本计划默认采用方案 A，并把方案 B 标为局部例外。

### 4.4 内部服务拆分原则

对现有/未来 ApplicationService 采用以下结构：

```text
HTTP Controller / ApplicationService
  - [Authorize]
  - DTO 校验
  - 从 CurrentTenant/CurrentUser 提取 TenantId、OperatorUserId
  - 发布 Job/Event payload
  - 调用内部业务服务

Internal App Service / Domain Service
  - 无 HTTP 授权入口语义
  - 显式参数：tenantId/operatorUserId 或 BackgroundExecutionContextDto
  - 负责真实业务逻辑

Background Worker / Job / EventHandler
  - 从 JobData/Args/Eto 读取 TenantId、OperatorUserId
  - IBackgroundExecutionContextRunner.RunAsync(...)
  - 调用内部业务服务
```

## 5. 分步实施计划与文件路径

### 步骤 1：修正 BackgroundWorker 启动方式

文件：`src/CheMa.VNext.BackgroundWorker/Program.cs`

计划修改：

- 对齐 `HttpApi.Host` 的 Host 配置风格。
- 使用 `builder.Host.AddAppSettingsSecretsJson().UseAutofac().UseSerilog()`。
- 使用 `await builder.AddApplicationAsync<VNextBackgroundWorkerModule>()`。
- `host` 构建后调用 `await host.InitializeApplicationAsync()`，再 `await host.RunAsync()`。
- 移除 `builder.Services.AddHostedService<BackgroundWorkerHostedService>()`。
- 移除 `builder.Services.AddApplication<...>` 手动初始化代码。

预期结构：

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Host.AddAppSettingsSecretsJson()
    .UseAutofac()
    .UseSerilog();
await builder.AddApplicationAsync<VNextBackgroundWorkerModule>();
var host = builder.Build();
await host.InitializeApplicationAsync();
await host.RunAsync();
```

文件：`src/CheMa.VNext.BackgroundWorker/BackgroundWorkerHostedService.cs`

计划修改：

- 删除该文件，或至少从 DI 注册移除后保留不用。
- 推荐删除，避免未来误用双初始化。

### 步骤 2：补齐后台模块依赖

文件：`src/CheMa.VNext.BackgroundWorker/VNextBackgroundWorkerModule.cs`

计划修改：

- 如果后台需要调用应用层实现，依赖 `VNextApplicationModule`，不是只依赖 `VNextApplicationContractsModule`。
- 保留 `VNextEntityFrameworkCoreModule` 与 `VNextBackgroundJobsModule`。

建议：

```csharp
[DependsOn(
    typeof(AbpAutofacModule),
    typeof(VNextEntityFrameworkCoreModule),
    typeof(VNextApplicationModule),
    typeof(VNextBackgroundJobsModule)
)]
```

注意：如果后续完全拆到 DomainService，也仍可保留 `VNextApplicationModule`，便于复用内部应用服务；但不要调用 `[Authorize]` HTTP 入口方法。

### 步骤 3：配置后台进程必要配置源

文件：`src/CheMa.VNext.BackgroundWorker/appsettings.json` 与部署配置。

计划检查/补齐：

- `ConnectionStrings:Default`
- Redis/DistributedCache 配置，如果 Quartz、缓存、分布式锁依赖 Redis。
- `AuthServer`/OpenIddict 仅在 worker 需要校验 token 或调用认证相关服务时需要；本方案默认不依赖。
- 确认 `appsettings.secrets.json` 被加载。当前 csproj 已复制该文件，但 Program 未显式 `AddAppSettingsSecretsJson()`，步骤 1 会补齐。

### 步骤 4：新增统一上下文 DTO/Args 基类

建议文件：

- `src/CheMa.VNext.Application.Contracts/BackgroundWork/BackgroundExecutionContextDto.cs`
- 或 `src/CheMa.VNext.BackgroundJobs/BackgroundWork/BackgroundExecutionContextDto.cs`，如果只在后台模块内用。

推荐放在 `Application.Contracts`，因为 HTTP ApplicationService、Job Args、Eto 都需要引用。

内容：

```csharp
public sealed class BackgroundExecutionContextDto
{
    public Guid? TenantId { get; set; }
    public Guid? OperatorUserId { get; set; }
    public string? CorrelationId { get; set; }
    public string? Source { get; set; }
}
```

### 步骤 5：新增后台上下文 Runner

建议文件：

- 接口：`src/CheMa.VNext.Application.Contracts/BackgroundWork/IBackgroundExecutionContextRunner.cs`
- 实现：`src/CheMa.VNext.Application/BackgroundWork/BackgroundExecutionContextRunner.cs`

实现职责：

- 统一 `ICurrentTenant.Change(context.TenantId)`。
- 统一 `IUnitOfWorkManager.Begin(...)`。
- 记录日志 scope：`TenantId`、`OperatorUserId`、`CorrelationId`。
- 不默认设置 `ICurrentUser`。

伪代码：

```csharp
public async Task RunAsync(BackgroundExecutionContextDto context, Func<Task> action)
{
    using (_currentTenant.Change(context.TenantId))
    using (_logger.BeginScope(new Dictionary<string, object?>
    {
        ["TenantId"] = context.TenantId,
        ["OperatorUserId"] = context.OperatorUserId,
        ["CorrelationId"] = context.CorrelationId
    }))
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        await action();
        await uow.CompleteAsync();
    }
}
```

### 步骤 6：规范 Quartz worker 入口

文件：`src/CheMa.VNext.BackgroundJobs/BackgroundWork/SampleQuartzBackgroundWorker.cs`

计划修改：

- 注入 `IBackgroundExecutionContextRunner` 与真实内部业务服务。
- 从 Quartz `JobDataMap` 或配置/数据库读取 `TenantId`。
- Host 级任务允许 `TenantId = null`。
- 执行时包裹：`runner.RunAsync(executionContext, async () => ...)`。
- 日志输出 `CurrentTenant.Id`，验证上下文切换。

注意：Quartz 定时任务若需要“每个租户单独执行”，不要在一个 job 中隐式混跑所有租户。按用户确认，应由调度/上游为每个租户生成独立任务，或在调度阶段为每个租户创建独立 `JobDataMap`。

### 步骤 7：规范 ABP BackgroundJob Args

新增/修改 job args：

```csharp
public sealed class SomeBackgroundJobArgs
{
    public Guid BusinessId { get; set; }
    public BackgroundExecutionContextDto ExecutionContext { get; set; } = new();
}
```

HTTP 入队时：

```csharp
await _backgroundJobManager.EnqueueAsync(new SomeBackgroundJobArgs
{
    BusinessId = id,
    ExecutionContext = new BackgroundExecutionContextDto
    {
        TenantId = CurrentTenant.Id,
        OperatorUserId = CurrentUser.Id,
        CorrelationId = _correlationIdProvider.Get()
    }
});
```

Job 执行时：

```csharp
await _runner.RunAsync(args.ExecutionContext, async () =>
{
    await _internalService.DoAsync(args.BusinessId, args.ExecutionContext.OperatorUserId);
});
```

### 步骤 8：规范分布式事件/MQ payload

Eto/Message 必须包含：

- 业务主键，例如 `OrderId`
- `ExecutionContext.TenantId`
- `ExecutionContext.OperatorUserId`
- `CorrelationId`

发布方在 HTTP 请求内从 `CurrentTenant.Id`、`CurrentUser.Id` 取值，写入消息。消费者不得从本地 `CurrentTenant`/`CurrentUser` 猜测来源。

消费者：

```csharp
public async Task HandleEventAsync(CompleteOrderInfoEto eventData)
{
    await _runner.RunAsync(eventData.ExecutionContext, async () =>
    {
        await _orderInternalService.CompleteInfoAsync(
            eventData.OrderId,
            eventData.ExecutionContext.OperatorUserId);
    });
}
```

### 步骤 9：拆分 HTTP ApplicationService 与内部业务服务

对类似“创建订单 + 补充订单信息 EventHandler”的流程：

- `OrderAppService.CreateAsync(...)`
  - 保留 `[Authorize]`。
  - 从 `CurrentTenant.Id` / `CurrentUser.Id` 创建 `BackgroundExecutionContextDto`。
  - 调用内部服务创建订单。
  - 发布 `CompleteOrderInfoEto`，payload 携带上下文。

- `OrderInternalAppService` 或 `OrderDomainService`
  - 无 `[Authorize]`。
  - 显式接收 `operatorUserId`。
  - 写业务审计字段。

- `CompleteOrderInfoEventHandler`
  - 从 Eto 读取上下文。
  - 使用 runner 切换租户/UOW。
  - 调内部服务补充信息。

### 步骤 10：清理后台代码中的危险假设

全局搜索并修复：

- `CurrentUser.Id.Value`
- `CurrentTenant.Id.Value`
- `CurrentUser.UserName!`
- 后台 worker/job/handler 中直接依赖 `IHttpContextAccessor.HttpContext`
- 后台直接调用带 `[Authorize]` 的 ApplicationService 方法

改法：

- 使用显式上下文 payload。
- 对 `TenantId = null` 做 Host 级分支。
- 对 `OperatorUserId = null` 做系统任务语义。

## 6. 测试与验证策略

### 6.1 启动验证

命令/方式：运行 `CheMa.VNext.BackgroundWorker`。

预期：

- 日志出现 ABP application initialized。
- `SampleQuartzBackgroundWorker` 被注册。
- 不再出现 ABP 初始化阶段 NRE。

若仍失败：

- 查看是否缺失 `ConnectionStrings:Default`。
- 查看 `appsettings.secrets.json` 是否复制且被加载。
- 查看 Quartz/Redis/DB 配置是否被后台进程读取。

### 6.2 Tenant 切换验证

新增一个测试/临时诊断 worker：

- 输入 `TenantId = A`，在 runner 内记录 `_currentTenant.Id`，查询租户 A 的数据。
- 输入 `TenantId = B`，确认查不到 A 的租户数据。
- 输入 `TenantId = null`，确认执行 Host 级逻辑，不误入某个租户。

### 6.3 用户审计验证

以“创建订单 -> 分布式事件补充订单信息”为例：

1. 用户 U 在租户 T 下调用 HTTP 创建订单。
2. 发布的 Eto 中包含：`TenantId = T`、`OperatorUserId = U`。
3. MQ 消费端日志记录同样的 T/U。
4. 订单补充信息表或业务审计字段中写入 `OperatorUserId = U`。
5. 验证即使消费者进程 `CurrentUser.Id == null`，业务审计仍正确。

### 6.4 授权绕过验证

- HTTP ApplicationService 仍保留 `[Authorize]`，未登录请求失败。
- 后台 handler 不调用该 `[Authorize]` 方法，而调用内部服务，因此不会因 `CurrentUser` 为空而失败。
- 内部服务不包含前端权限判断，但保留必要业务校验，例如订单状态、租户归属、幂等性。

### 6.5 回归测试建议

新增测试覆盖：

- `BackgroundExecutionContextRunner` 在指定租户下切换 `ICurrentTenant.Id`。
- runner 在 `TenantId = null` 下进入 Host 级上下文。
- job args/eto 序列化后仍保留 `TenantId`、`OperatorUserId`。
- handler 在 `OperatorUserId = null` 时按系统任务执行。

## 7. 风险与注意事项

1. 如果强行让后台继续调用 `[Authorize]` 的 ApplicationService，会与“仅审计 ID、不模拟登录身份”的目标冲突。
2. 如果依赖 ABP 内置 `CreatorId`/`LastModifierId` 必须等于原始用户，则需要局部引入 `ICurrentPrincipalAccessor.Change(...)`；但这不应被误用为真实授权身份。
3. `TenantId = null` 有两种含义：Host 级任务或缺失上下文。必须由任务类型明确区分，不能静默把缺失租户当 Host 级执行。
4. Quartz 定时任务没有 HTTP 发起人，通常 `OperatorUserId = null`，属于系统任务；如果需要租户任务，应由调度数据显式提供 TenantId。
5. 分布式事件跨进程后 ambient context 必然丢失，必须靠 payload 恢复。
6. 后台进程配置必须独立完整，不能假设 Web Host 的配置和中间件会共享过来。

## 8. 推荐执行顺序

1. 先改 BackgroundWorker 启动方式，验证启动 NRE 消失。
2. 补齐 `VNextApplicationModule` 依赖和后台配置源。
3. 增加 `BackgroundExecutionContextDto` 与 `IBackgroundExecutionContextRunner`。
4. 改一个最小链路做样板：Quartz 或订单补充信息分布式事件。
5. 按样板推广到 ABP Job、HTTP 触发、MQ handler。
6. 清理后台对 `CurrentUser`/`HttpContext` 的隐式依赖。
7. 增加测试与日志验证。

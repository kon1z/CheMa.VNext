# 新增 ABP Background Jobs 与 Background Worker 项目实施计划

## 已确认事实

- 当前解决方案为 ABP + Aspire 结构，主解决方案文件是 `CheMa.VNext.sln`。
- `src/` 下已有分层项目：`Domain`、`Application`、`EntityFrameworkCore`、`HttpApi.Host`、`DbMigrator`、`Blazor`、`AppHost`、`ServiceDefaults` 等。
- 当前项目使用 `net10.0`，集中包版本管理位于 `Directory.Packages.props`。
- `Domain` 已依赖 `Volo.Abp.BackgroundJobs.Domain`，`EntityFrameworkCore` 已依赖 `Volo.Abp.BackgroundJobs.EntityFrameworkCore`，并且 `VNextDbContext` 对应迁移中已有 ABP BackgroundJobs 持久化基础的可能性。
- 目前没有独立的 `BackgroundJobs` 项目，也没有独立的 `BackgroundWorker` 项目。
- `AppHost` 当前编排 PostgreSQL、Redis、DbMigrator、HttpApi.Host、Blazor，并通过 `ServiceDefaults` 接入服务发现与 OpenTelemetry。

## 用户确认范围

- 新增两个独立项目。
- 命名按当前风格：
  - `CheMa.VNext.BackgroundJobs`
  - `CheMa.VNext.BackgroundWorker`
- 做到“骨架 + 基础接入”，不添加具体业务示例任务。
- Background Worker 采用 ABP 常见的周期型 Worker 方向作为基础结构。
- Background Jobs 是否独立部署未明确；按推荐方案处理。
- 需要全量接入：`.sln`、`AppHost`、相关项目引用都补齐。

## 推荐设计

### 1. `CheMa.VNext.BackgroundJobs`

定位：类库项目，承载后台 Job 定义与后续业务 Job 实现。

原因：ABP Background Job 的核心价值是把 Job 类型作为可注入、可排队、可重试的业务执行单元；通常不需要为每个 Job 类库单独启动一个进程。真正执行 Job 的宿主应由 Web Host 或 Worker Host 持有。当前用户要求两个独立项目，因此该项目作为 Job 代码归集模块最合理。

初始内容：

- `src/CheMa.VNext.BackgroundJobs/CheMa.VNext.BackgroundJobs.csproj`
  - `TargetFramework`：`net10.0`
  - `RootNamespace`：`CheMa.VNext`
  - 引用：
    - `CheMa.VNext.Application.Contracts` 或 `CheMa.VNext.Domain`，视模块依赖最小化选择
    - `Volo.Abp.BackgroundJobs.Abstractions`
- `VNextBackgroundJobsModule.cs`
  - `DependsOn(typeof(VNextApplicationContractsModule))` 或更低层模块
  - `DependsOn(typeof(AbpBackgroundJobsAbstractionsModule))`

不添加具体 Job 类，避免引入无业务含义代码。

### 2. `CheMa.VNext.BackgroundWorker`

定位：独立可执行 Worker Host，负责运行 ABP 应用、注册周期型 Background Worker，并可执行 BackgroundJobs 项目中的任务。

初始内容：

- `src/CheMa.VNext.BackgroundWorker/CheMa.VNext.BackgroundWorker.csproj`
  - `OutputType`：`Exe`
  - `TargetFramework`：`net10.0`
  - `Nullable`：`enable`
  - 引用：
    - `Microsoft.Extensions.Hosting`
    - `Serilog.Extensions.Logging`
    - `Serilog.Sinks.Async`
    - `Serilog.Sinks.Console`
    - `Serilog.Sinks.File`
    - `Volo.Abp.Autofac`
    - `Volo.Abp.Caching.StackExchangeRedis`
    - `Volo.Abp.BackgroundJobs.Domain` 或 ABP 默认执行所需模块
    - `CheMa.VNext.EntityFrameworkCore`
    - `CheMa.VNext.Application.Contracts`
    - `CheMa.VNext.BackgroundJobs`
    - `CheMa.VNext.ServiceDefaults`
- `Program.cs`
  - 参考 `DbMigrator/Program.cs` 的 Generic Host、Serilog、OpenTelemetry 结构。
  - 调用 `builder.AddServiceDefaults()` 或等价 `ConfigureServices` 注册，保持 Aspire 观测一致。
  - 使用 `UseAutofac()`。
  - `AddApplicationAsync<VNextBackgroundWorkerModule>()`、`InitializeApplicationAsync()`、`RunAsync()`。
- `VNextBackgroundWorkerModule.cs`
  - 依赖：
    - `AbpAutofacModule`
    - `VNextEntityFrameworkCoreModule`
    - `VNextApplicationContractsModule`
    - `VNextBackgroundJobsModule`
  - 配置 Redis 分布式缓存前缀 `VNext:`，与 DbMigrator 保持一致。
  - 预留周期 Worker 注册位置：`context.AddBackgroundWorkerAsync<...>()`。由于本次不添加示例任务，先只建立模块入口与依赖，不创建无业务 Worker。
- `appsettings.json`
  - 参考 `DbMigrator/appsettings.json`，保留必要 OpenIddict/Application 配置或最小化配置。
- `appsettings.secrets.json`
  - 如当前项目约定需要文件复制，则创建空 JSON `{}` 或复用现有连接字符串配置方式；避免写入真实密钥。

## 解决方案与 Aspire 接入

### `.sln`

将两个新项目加入 `CheMa.VNext.sln`：

- 项目节点加入 `src` solution folder。
- 增加 Debug/Release Any CPU/x64/x86 配置。
- 增加 NestedProjects 映射到 `src` 文件夹。

优先使用 `dotnet sln add`，减少手工 GUID 错误；若命令不可用，再手工编辑并保持格式。

### `CheMa.VNext.AppHost`

- 给 `src/CheMa.VNext.AppHost/CheMa.VNext.AppHost.csproj` 增加：
  - `ProjectReference Include="..\CheMa.VNext.BackgroundWorker\CheMa.VNext.BackgroundWorker.csproj"`
- 修改 `src/CheMa.VNext.AppHost/Program.cs`：
  - 在 `dbMigrator` 完成后添加：
    - `builder.AddProject<Projects.CheMa_VNext_BackgroundWorker>("background-worker")`
    - `.WithReference(database)`
    - `.WithReference(redis)`
    - `.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317")`
    - `.WaitForCompletion(dbMigrator)`
    - `.WaitFor(database)`
    - `.WaitFor(redis)`
    - `.WaitFor(openTelemetry)`
  - 不让 `HttpApi.Host` 依赖 Worker，避免 Web 服务启动被 Worker 阻塞。

## 相关项目引用

- `HttpApi.Host` 是否引用 `BackgroundJobs`：
  - 推荐加入 `CheMa.VNext.BackgroundJobs` 引用，并在 `VNextHttpApiHostModule` 依赖 `VNextBackgroundJobsModule`。
  - 原因：后续 Web/Application 层可能通过 `IBackgroundJobManager` 入队这些 Job 类型；Host 也需要能发现 Job 类型。
- `BackgroundWorker` 引用 `BackgroundJobs`，作为运行后台执行逻辑的宿主。
- 不把 `BackgroundWorker` 引用回 `HttpApi.Host`，避免宿主循环依赖。

## 实施步骤

1. 创建 `src/CheMa.VNext.BackgroundJobs` 类库项目。
2. 创建 `VNextBackgroundJobsModule.cs`，补齐 ABP 模块依赖。
3. 创建 `src/CheMa.VNext.BackgroundWorker` 控制台/Worker 宿主项目。
4. 创建 `VNextBackgroundWorkerModule.cs` 与 `Program.cs`，复用现有 `DbMigrator` 的日志、Autofac、配置、服务发现/OTEL 风格。
5. 创建 Worker 必要配置文件，并在 csproj 中设置复制到输出目录。
6. 把两个项目加入 `CheMa.VNext.sln`。
7. 修改 `HttpApi.Host.csproj` 和 `VNextHttpApiHostModule.cs`，接入 `BackgroundJobs` 模块。
8. 修改 `AppHost.csproj` 和 `AppHost/Program.cs`，接入 `BackgroundWorker` 编排。
9. 运行构建验证：
   - `dotnet build CheMa.VNext.sln`
10. 如构建失败，按错误修正缺失包、命名空间、模块依赖或 Aspire generated project name。

## 验收标准

- `CheMa.VNext.sln` 中出现两个新项目。
- 两个新项目位于 `src/` 下，命名符合当前项目风格。
- `CheMa.VNext.BackgroundJobs` 是可编译 ABP 模块类库。
- `CheMa.VNext.BackgroundWorker` 是可启动的独立 Host 项目。
- `HttpApi.Host` 能引用并加载 `VNextBackgroundJobsModule`。
- `AppHost` 能编排 `background-worker`，且它等待数据库、Redis、OpenTelemetry、DbMigrator。
- `dotnet build CheMa.VNext.sln` 通过。
- 不引入无业务含义的示例 Job/Worker 类。
- 不写入真实密钥或修改无关业务代码。

## 假设与风险

- 假设 ABP 10.3.0 的模块命名与当前已引用包一致，`Volo.Abp.BackgroundJobs.Abstractions` 可直接提供 `AbpBackgroundJobsAbstractionsModule`。
- 假设 Worker Host 可以复用 `DbMigrator` 的配置加载方式；若缺少连接字符串，需按 Aspire 注入配置验证实际运行。
- 如果 `BackgroundWorker` 不注册任何实际周期 Worker，项目可以启动但不会执行业务动作；这是“骨架 + 基础接入”的预期结果。
- 如果后续要求 Job 由独立进程消费，需要进一步确认是否禁用 `HttpApi.Host` 中的 Job 执行器，避免多实例重复消费策略不清晰。

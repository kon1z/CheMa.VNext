# CheMa.VNext 日志架构与日志结构最佳实践方案

## 1. 已确认范围

- 项目现状：ABP 10.3、.NET 10、Aspire AppHost、OpenTelemetry Collector、OpenObserve、Serilog 文件/控制台、PostgreSQL、Redis。
- 当前日志基础：
  - `CheMa.VNext.HttpApi.Host/Program.cs` 使用 Serilog，写入 `Logs/logs.txt` 和 Console。
  - `VNextHttpApiHostModule` 已启用 `app.UseCorrelationId()`、`app.UseAuditing()`、`app.UseAbpSerilogEnrichers()`。
  - `ServiceDefaults` 已配置 OpenTelemetry logs/traces/metrics，并在存在 `OTEL_EXPORTER_OTLP_ENDPOINT` 时导出。
  - `AppHost` 已运行 OpenObserve 与 OTel Collector，logs/traces/metrics pipeline 均导向 OpenObserve。
  - EF Core 当前未配置详细 SQL 日志，Serilog 里 `Microsoft.EntityFrameworkCore` 为 Warning。
- 目标：设计架构方案优先，不直接实现代码；输出架构、字段结构、事件类型、链路、告警规则和落地建议。

## 2. 用户已锁定的设计决策

- 主存储/查询入口：OpenObserve 唯一主入口。
- 服务范围：全链路统一，覆盖 HTTP、后台任务、分布式事件、慢 SQL/异常；未来支持多后端微服务。
- Body 策略：完整优先。
- HTTP 记录结构：每个请求完成后生成单条完整 `HttpExchange` 事件。
- Body 白名单：默认所有 `/api/**` 全量记录；`health`、`alive`、Swagger、静态资源等不纳入完整 body。
- 鉴权端点：即使是 `/connect/token`、Authorization、Cookie、token、密码等也完整记录；不做脱敏。
- Header 结构：完整原样 headers + 常用 header 顶层索引字段。
- Trace 入口：OpenTelemetry `trace_id` 优先；跨服务依赖 W3C `traceparent/tracestate`。
- ABP 关系：不依赖数据库审计表作为主存；借用 ABP 的用户、租户、Correlation、审计生命周期/上下文概念，最终日志进 OpenObserve。
- 数据库日志：只纳入慢 SQL + SQL 异常。
- 日志失败策略：业务优先，日志失败不得阻断业务响应。
- 保留周期：完整 body 中期保留，建议 30-90 天。
- 使用场景：问题排障、审计追溯、性能治理同等重要。
- 告警：纳入告警规则。

## 3. 推荐总体架构

```text
ASP.NET Core / ABP 服务
  ├─ HTTP 入站日志捕获层
  │   ├─ 读取 request headers/body
  │   ├─ 包装 response stream 捕获 response body
  │   ├─ 合并 ABP 当前用户/租户/Correlation 与 OTel Trace
  │   └─ 生成单条 HttpExchange 结构化事件
  │
  ├─ ABP 审计上下文增强层
  │   ├─ 用户、租户、客户端、Action、异常、耗时
  │   ├─ 不以 ABP AuditLog 数据库表为主存
  │   └─ 用于补充 OpenObserve 事件字段
  │
  ├─ 后台任务 / 分布式事件日志规范
  │   ├─ JobExecution 事件
  │   ├─ DistributedEvent 事件
  │   └─ 统一 trace_id、correlation_id、job_id、event_name、payload
  │
  ├─ EF Core 慢 SQL / 异常日志
  │   ├─ SqlSlow 事件
  │   └─ SqlException 事件
  │
  └─ ILogger / Serilog / OpenTelemetry Logs
      └─ OTLP -> OTel Collector -> OpenObserve
```

### 3.1 核心原则

1. OpenObserve 是唯一排障与审计查询入口。
2. 每类日志都必须结构化，不依赖纯文本 Message 检索。
3. HTTP 请求响应以单事件完整记录，便于一次查询还原全过程。
4. `trace_id` 是最高优先级排障主键；`correlation_id`、`tenant_id`、`user_id` 是辅助审计维度。
5. 日志采集失败不得影响业务响应。
6. 不做数据脱敏，但要在方案中明确风险：OpenObserve 权限、保留周期、访问审计必须严格控制。

## 4. 日志事件分类

### 4.1 `http.exchange`

用途：完整记录一次入站 HTTP 请求与响应。

触发：请求完成后写一条事件。

白名单：
- 完整 body：`/api/**`。
- 完整记录鉴权：`/connect/token` 等鉴权端点按用户要求完整记录。
- 摘要或跳过：`/health`、`/alive`、Swagger、静态资源、开发工具资源。

### 4.2 `job.execution`

用途：记录 ABP BackgroundJob / Quartz BackgroundWorker 执行。

包含：Job 名称、参数 payload、开始/结束时间、耗时、结果、异常、trace/correlation。

### 4.3 `event.distributed`

用途：记录 ABP 分布式事件发布与消费。

包含：event_name、event_id、direction、payload、handler、耗时、异常。

### 4.4 `sql.slow`

用途：记录超过阈值的 SQL。

建议默认阈值：500ms 或 1000ms，可配置。

### 4.5 `sql.exception`

用途：记录 SQL 执行异常。

包含 SQL 文本、参数、异常类型、错误码、耗时、trace_id。

### 4.6 `log.pipeline.failure`

用途：记录日志捕获、序列化、OTel 导出或本地降级失败。

原则：只告警，不阻断业务。

## 5. 标准字段结构

### 5.1 所有事件公共字段

| 字段 | 类型 | 说明 |
|---|---|---|
| `event_type` | string | `http.exchange` / `job.execution` / `event.distributed` / `sql.slow` / `sql.exception` |
| `timestamp` | datetime | 事件生成时间 UTC |
| `service.name` | string | 服务名，来自 OTel Resource |
| `service.namespace` | string | 固定 `CheMa.VNext` |
| `service.version` | string | 程序版本 |
| `service.instance.id` | string | 实例 ID / 机器名 / Pod ID |
| `deployment.environment.name` | string | Development/Staging/Production |
| `trace_id` | string | OTel TraceId，主查询键 |
| `span_id` | string | OTel SpanId |
| `parent_span_id` | string | 父 SpanId，可为空 |
| `correlation_id` | string | ABP CorrelationId |
| `tenant_id` | string | ABP TenantId，可为空 |
| `tenant_name` | string | 租户名，可为空 |
| `user_id` | string | 当前用户 ID，可为空 |
| `user_name` | string | 当前用户名，可为空 |
| `client_id` | string | OAuth/OpenIddict client_id 或调用方服务 |
| `level` | string | Information / Warning / Error |
| `success` | bool | 是否成功 |
| `duration_ms` | number | 耗时 |
| `exception.type` | string | 异常类型 |
| `exception.message` | string | 异常消息 |
| `exception.stacktrace` | string | 堆栈 |

## 6. `http.exchange` 字段结构

| 字段 | 类型 | 说明 |
|---|---|---|
| `http.request.method` | string | GET/POST/PUT/DELETE |
| `http.request.scheme` | string | http/https |
| `http.request.host` | string | Host |
| `http.request.path` | string | Path |
| `http.request.query_string` | string | 原始 QueryString |
| `http.route` | string | 路由模板，若可获取 |
| `http.status_code` | number | 响应状态码 |
| `http.request.content_type` | string | 请求 Content-Type |
| `http.response.content_type` | string | 响应 Content-Type |
| `http.request.content_length` | number | 请求长度 |
| `http.response.content_length` | number | 响应长度 |
| `http.request.headers` | object/array | 原样请求头，保留完整内容 |
| `http.response.headers` | object/array | 原样响应头 |
| `http.request.header.user_agent` | string | 从 headers 提升出的索引字段 |
| `http.request.header.authorization` | string | 完整 Authorization，按用户要求不脱敏 |
| `http.request.header.cookie` | string | 完整 Cookie，按用户要求不脱敏 |
| `http.request.header.content_type` | string | 索引字段 |
| `http.request.header.x_forwarded_for` | string | 索引字段 |
| `http.request.remote_ip` | string | 客户端 IP |
| `http.request.body` | string/object | 完整请求 body |
| `http.response.body` | string/object | 完整响应 body |
| `http.body.capture_mode` | string | `full` / `summary` / `skipped` |
| `http.body.capture_reason` | string | 命中白名单、跳过原因、异常原因 |
| `abp.action.name` | string | ABP Action/AppService 方法名，能获取则填 |
| `abp.application.service` | string | AppService 名称，能获取则填 |

## 7. 后台任务字段结构

### 7.1 `job.execution`

| 字段 | 类型 | 说明 |
|---|---|---|
| `job.name` | string | Job/Worker 名称 |
| `job.type` | string | BackgroundJob / QuartzWorker |
| `job.id` | string | Job ID，可为空 |
| `job.args` | object/string | 完整参数 |
| `job.trigger` | string | Cron/Manual/Event |
| `job.started_at` | datetime | 开始时间 |
| `job.finished_at` | datetime | 结束时间 |
| `job.retry_count` | number | 重试次数 |
| `job.result` | string | Success/Failed/Skipped |

### 7.2 `event.distributed`

| 字段 | 类型 | 说明 |
|---|---|---|
| `event.name` | string | 事件类型名 |
| `event.id` | string | 事件 ID，可为空 |
| `event.direction` | string | publish / consume |
| `event.handler` | string | 消费 Handler 名称 |
| `event.payload` | object/string | 完整事件体 |
| `event.result` | string | Success/Failed |

## 8. SQL 字段结构

| 字段 | 类型 | 说明 |
|---|---|---|
| `db.system` | string | postgresql |
| `db.name` | string | 数据库名 |
| `db.statement` | string | SQL 文本 |
| `db.parameters` | object/string | SQL 参数，内网按要求不脱敏 |
| `db.duration_ms` | number | SQL 耗时 |
| `db.slow_threshold_ms` | number | 慢 SQL 阈值 |
| `db.operation` | string | SELECT/INSERT/UPDATE/DELETE |
| `db.exception.code` | string | 数据库错误码 |

## 9. OpenObserve 流与索引建议

建议至少拆分以下 stream：

- `app_http_exchange`：HTTP 完整请求响应。
- `app_runtime`：普通应用日志、后台任务、事件日志。
- `app_sql`：慢 SQL 与 SQL 异常。
- `app_pipeline`：日志管道自身状态与失败。

推荐高频查询字段：

- `trace_id`
- `service.name`
- `event_type`
- `http.request.path`
- `http.status_code`
- `tenant_id`
- `user_id`
- `client_id`
- `duration_ms`
- `exception.type`
- `job.name`
- `event.name`
- `db.duration_ms`

## 10. 告警规则

| 告警 | 条件建议 | 严重级别 |
|---|---|---|
| HTTP 5xx 错误率 | 5 分钟内 5xx 比例 > 2% 或数量 > N | Critical |
| 慢请求 | `/api/**` P95 > 2s 持续 5 分钟 | Warning |
| 单次超慢请求 | `duration_ms > 10000` | Warning |
| 鉴权失败激增 | `/connect/token` 失败数 5 分钟 > N | Warning |
| SQL 慢查询激增 | `sql.slow` 5 分钟 > N | Warning |
| SQL 异常 | 任意 `sql.exception` | Critical |
| 后台任务失败 | `job.execution success=false` | Critical |
| 分布式事件消费失败 | `event.distributed direction=consume success=false` | Critical |
| 日志管道失败 | `log.pipeline.failure` > 0 | Warning/Critical |
| OTel Collector 不可用 | OpenObserve 在 1-5 分钟内无某服务日志 | Critical |

## 11. 保留与容量策略

- 完整 body 保留：30-90 天。
- 摘要、异常、指标类日志可保留更久。
- OpenObserve 应按 stream 设置不同保留策略。
- 容量估算公式：

```text
每日 HTTP 日志量 ≈ 日请求数 × (平均请求头 + 平均请求 body + 平均响应头 + 平均响应 body + 公共字段开销)
总保留容量 ≈ 每日 HTTP 日志量 × 保留天数 × 副本/压缩系数 + SQL/Job/Event 日志量
```

- 由于用户要求鉴权与 token 完整记录，必须限制 OpenObserve 访问权限、操作审计、导出权限和备份访问权限。

## 12. 推荐落地顺序

1. 统一字段标准：先定义事件类型、公共字段、HTTP 字段、后台任务字段、SQL 字段。
2. OpenObserve stream 规划：确认 stream、索引字段、保留周期、访问权限。
3. HTTP 捕获：在 ABP 审计生命周期附近增强上下文，生成 `http.exchange`，最终通过 ILogger/OTel 输出。
4. ABP 上下文接入：补充 tenant/user/correlation/action/service 信息，但不依赖 AuditLog 数据库作为主存。
5. 后台任务/事件：为 BackgroundJob、Quartz Worker、DistributedEvent 建立统一日志包装规范。
6. SQL 慢查询/异常：通过 EF Core 拦截器或日志事件规范捕获慢 SQL 与异常。
7. 告警：在 OpenObserve 配置上述最小告警。
8. 验证：用一次 API 调用、一次鉴权调用、一次后台 Job、一次事件、一次慢 SQL/异常验证 trace_id 串联和字段完整性。

## 13. 风险与约束

- 完整记录 Authorization、Cookie、token、密码会显著扩大内部权限风险；虽然不脱敏，但 OpenObserve 权限必须按最高敏感级别管理。
- 完整 response body 对大响应、流式响应、文件下载会带来内存与 IO 压力；即使“完整优先”，仍建议保留技术兜底：捕获失败记录 `capture_mode=failed`，业务继续。
- 单事件完整结构查询简单，但超大事件可能影响 OpenObserve 写入、压缩与查询性能。
- 未来拆微服务时，所有服务必须统一 OTel Resource 字段、W3C trace propagation、日志 event schema，否则 trace_id 难以完整串联。
- ABP AuditLog 数据库表不是主存，业务后台若未来需要审计查询界面，需要另行从 OpenObserve API 或独立查询模型设计。

## 14. 验收标准

- 能通过一个 `trace_id` 在 OpenObserve 中查到同一次请求相关的 HTTP、Job/Event、SQL 日志。
- `/api/**` 请求产生一条 `http.exchange`，包含完整 headers、request body、response body、用户、租户、状态码、耗时。
- `/connect/token` 按用户要求完整记录请求/响应和 headers。
- 健康检查、Swagger、静态资源不产生完整 body 日志。
- 慢 SQL 和 SQL 异常能带上同一个 `trace_id`。
- 日志投递失败不影响业务响应，并产生 `log.pipeline.failure`。
- OpenObserve 至少具备错误率、慢请求、SQL 异常、后台任务失败、日志管道失败告警规则。

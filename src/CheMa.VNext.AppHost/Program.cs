// Demo-only full-stack sandbox. Development, test, and production services use external infrastructure and AgileConfig directly.
const string PostgresPassword = "postgres";
const string OpenObserveRootUserEmail = "root@example.com";
const string OpenObserveRootUserPassword = "Complexpasssimple123";
const string OpenObserveDataDir = "/data";
const string OpenObserveOtlpEndpoint = "openobserve:5081";
const string OpenObserveAuthHeader = "Basic cm9vdEBleGFtcGxlLmNvbTpDb21wbGV4cGFzc3NpbXBsZTEyMw==";
const string OpenObserveOrganization = "default";
const string OpenObserveStreamName = "default";
const string OtlpExporterEndpoint = "http://localhost:4317";
const string AgileConfigNodes = "http://localhost:5000";

var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", PostgresPassword, secret: true);
var openObserveRootUserEmail = builder.AddParameter("openobserve-root-user-email", OpenObserveRootUserEmail, secret: true);
var openObserveRootUserPassword = builder.AddParameter("openobserve-root-user-password", OpenObserveRootUserPassword, secret: true);
var openObserveDataDir = builder.AddParameter("openobserve-data-dir", OpenObserveDataDir, secret: true);
var openObserveOtlpEndpoint = builder.AddParameter("openobserve-otlp-endpoint", OpenObserveOtlpEndpoint, secret: true);
var openObserveAuthHeader = builder.AddParameter("openobserve-auth-header", OpenObserveAuthHeader, secret: true);
var openObserveOrganization = builder.AddParameter("openobserve-organization", OpenObserveOrganization, secret: true);
var openObserveStreamName = builder.AddParameter("openobserve-stream-name", OpenObserveStreamName, secret: true);
var timeZone = builder.AddParameter("time-zone", "Asia/Shanghai", secret: true);
var agileConfigAdminConsole = builder.AddParameter("agileconfig-admin-console", "true", secret: true);
var agileConfigDbProvider = builder.AddParameter("agileconfig-db-provider", "npgsql", secret: true);
var otlpExporterEndpoint = builder.AddParameter("otel-exporter-otlp-endpoint", OtlpExporterEndpoint, secret: true);
var agileConfigNodes = builder.AddParameter("agileconfig-nodes", AgileConfigNodes, secret: true);

var openObserve = builder.AddContainer("openobserve", "public.ecr.aws/zinclabs/openobserve", "latest")
    .WithEnvironment("ZO_ROOT_USER_EMAIL", openObserveRootUserEmail)
    .WithEnvironment("ZO_ROOT_USER_PASSWORD", openObserveRootUserPassword)
    .WithEnvironment("ZO_DATA_DIR", openObserveDataDir)
    .WithHttpEndpoint(port: 5080, targetPort: 5080, name: "ui")
    .WithVolume("openobserve-data", "/data")
    .WithLifetime(ContainerLifetime.Persistent);

var openTelemetry = builder.AddContainer("opentelemetry", "otel/opentelemetry-collector-contrib", "0.107.0")
    .WithArgs("--config=/etc/otelcol/config.yaml")
    .WithBindMount("./otel-collector-config.yaml", "/etc/otelcol/config.yaml", isReadOnly: true)
    .WithEnvironment("OPENOBSERVE_OTLP_ENDPOINT", openObserveOtlpEndpoint)
    .WithEnvironment("OPENOBSERVE_AUTH_HEADER", openObserveAuthHeader)
    .WithEnvironment("OPENOBSERVE_ORGANIZATION", openObserveOrganization)
    .WithEnvironment("OPENOBSERVE_STREAMNAME", openObserveStreamName)
    .WithEndpoint(port: 4317, targetPort: 4317, name: "otlp-grpc")
    .WithEndpoint(port: 4318, targetPort: 4318, name: "otlp-http")
    .WaitFor(openObserve);

var postgres = builder.AddPostgres("postgres", password: postgresPassword, port: 5432)
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("Default", "VNext");

var redis = builder.AddRedis("redis", port: 6379)
    .WithLifetime(ContainerLifetime.Persistent);

_ = postgres.AddDatabase("agileconfig-db", "agile_config");

var agileConfig = builder.AddContainer("agileconfig", "kklldog/agile_config", "latest")
    .WithEnvironment("TZ", timeZone)
    .WithEnvironment("adminConsole", agileConfigAdminConsole)
    .WithEnvironment("db__provider", agileConfigDbProvider)
    .WithEnvironment("db__conn", ReferenceExpression.Create(
        $"Host={postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)};" +
        $"Port={postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Port)};" +
        $"Database=agile_config;" +
        $"Username=postgres;" +
        $"Password={postgresPassword};"))
    .WithHttpEndpoint(port: 5000, targetPort: 5000, name: "admin")
    .WithLifetime(ContainerLifetime.Persistent)
    .WaitFor(postgres);

var dbMigrator = builder.AddProject<Projects.CheMa_VNext_DbMigrator>("dbmigrator")
    .WithReference(database)
    .WithReference(redis)
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", otlpExporterEndpoint)
    .WaitFor(database)
    .WaitFor(redis)
    .WaitFor(openTelemetry);

builder.AddProject<Projects.CheMa_VNext_BackgroundWorker>("background-worker")
    .WithReference(database)
    .WithReference(redis)
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", otlpExporterEndpoint)
    .WithEnvironment("AgileConfig__nodes", agileConfigNodes)
    .WaitForCompletion(dbMigrator)
    .WaitFor(database)
    .WaitFor(redis)
    .WaitFor(openTelemetry)
    .WaitFor(agileConfig);

var httpApiHost = builder.AddProject<Projects.CheMa_VNext_HttpApi_Host>("httpapi-host")
    .WithReference(database)
    .WithReference(redis)
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", otlpExporterEndpoint)
    .WithEnvironment("AgileConfig__nodes", agileConfigNodes)
    .WaitForCompletion(dbMigrator)
    .WaitFor(database)
    .WaitFor(redis)
    .WaitFor(openTelemetry)
    .WaitFor(agileConfig);

var gateway = builder.AddProject<Projects.CheMa_VNext_Gateway>("gateway")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", otlpExporterEndpoint)
    .WithEnvironment("AgileConfig__nodes", agileConfigNodes)
    .WaitFor(httpApiHost)
    .WaitFor(openTelemetry)
    .WaitFor(agileConfig);

builder.AddProject<Projects.CheMa_VNext_Blazor>("blazor")
    .WithReference(httpApiHost)
    .WaitFor(httpApiHost)
    .WaitFor(gateway);

builder.Build().Run();

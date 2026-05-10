// Demo-only full-stack sandbox. Development, test, and production services use external infrastructure and AgileConfig directly.
const string PostgresPassword = "postgres";
const string OpenObserveRootUserEmail = "root@example.com";
const string OpenObserveRootUserPassword = "Complexpasssimple123";
const string OpenObserveDataDir = "/data";
const string OpenObserveOtlpEndpoint = "openobserve:5081";
const string OpenObserveAuthHeader = "Basic cm9vdEBleGFtcGxlLmNvbTpDb21wbGV4cGFzc3NpbXBsZTEyMw==";
const string OpenObserveOrganization = "default";
const string OpenObserveStreamName = "default";
const string AgileConfigDbConnectionString = $"Host=postgres;Port=5432;Database=agile_config;Username=postgres;Password={PostgresPassword};";
const string OtlpExporterEndpoint = "http://localhost:4317";
const string AgileConfigNodes = "http://localhost:5000";

var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("POSTGRES_PASSWORD", PostgresPassword, secret: true);
var openObserveRootUserEmail = builder.AddParameter("ZO_ROOT_USER_EMAIL", OpenObserveRootUserEmail, secret: true);
var openObserveRootUserPassword = builder.AddParameter("ZO_ROOT_USER_PASSWORD", OpenObserveRootUserPassword, secret: true);
var openObserveDataDir = builder.AddParameter("ZO_DATA_DIR", OpenObserveDataDir, secret: true);
var openObserveOtlpEndpoint = builder.AddParameter("OPENOBSERVE_OTLP_ENDPOINT", OpenObserveOtlpEndpoint, secret: true);
var openObserveAuthHeader = builder.AddParameter("OPENOBSERVE_AUTH_HEADER", OpenObserveAuthHeader, secret: true);
var openObserveOrganization = builder.AddParameter("OPENOBSERVE_ORGANIZATION", OpenObserveOrganization, secret: true);
var openObserveStreamName = builder.AddParameter("OPENOBSERVE_STREAMNAME", OpenObserveStreamName, secret: true);
var timeZone = builder.AddParameter("TZ", "Asia/Shanghai", secret: false);
var agileConfigAdminConsole = builder.AddParameter("adminConsole", "true", secret: false);
var agileConfigDbProvider = builder.AddParameter("db__provider", "npgsql", secret: false);
var agileConfigDbConnectionString = builder.AddParameter("db__conn", AgileConfigDbConnectionString, secret: true);
var otlpExporterEndpoint = builder.AddParameter("OTEL_EXPORTER_OTLP_ENDPOINT", OtlpExporterEndpoint, secret: true);
var agileConfigNodes = builder.AddParameter("AgileConfig__nodes", AgileConfigNodes, secret: true);

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

var postgres = builder.AddPostgres("postgres", port: 5432)
    .WithEnvironment("POSTGRES_PASSWORD", postgresPassword)
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("Default", "VNext");

var redis = builder.AddRedis("redis", port: 6379)
    .WithLifetime(ContainerLifetime.Persistent);

_ = postgres.AddDatabase("agileconfig-db", "agile_config");

var agileConfig = builder.AddContainer("agileconfig", "kklldog/agile_config", "latest")
    .WithEnvironment("TZ", timeZone)
    .WithEnvironment("adminConsole", agileConfigAdminConsole)
    .WithEnvironment("db__provider", agileConfigDbProvider)
    .WithEnvironment("db__conn", agileConfigDbConnectionString)
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

// Demo-only full-stack sandbox. Development, test, and production services use external infrastructure and AgileConfig directly.
var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var openObserve = builder.AddContainer("openobserve", "public.ecr.aws/zinclabs/openobserve", "latest")
    .WithEnvironment("ZO_ROOT_USER_EMAIL", "root@example.com")
    .WithEnvironment("ZO_ROOT_USER_PASSWORD", "Complexpasssimple123")
    .WithEnvironment("ZO_DATA_DIR", "/data")
    .WithHttpEndpoint(port: 5080, targetPort: 5080, name: "ui")
    .WithVolume("openobserve-data", "/data")
    .WithLifetime(ContainerLifetime.Persistent);

var openTelemetry = builder.AddContainer("opentelemetry", "otel/opentelemetry-collector-contrib", "0.107.0")
    .WithArgs("--config=/etc/otelcol/config.yaml")
    .WithBindMount("./otel-collector-config.yaml", "/etc/otelcol/config.yaml", isReadOnly: true)
    .WithEnvironment("OPENOBSERVE_OTLP_ENDPOINT", "openobserve:5081")
    .WithEnvironment("OPENOBSERVE_AUTH_HEADER", "Basic cm9vdEBleGFtcGxlLmNvbTpDb21wbGV4cGFzc3NpbXBsZTEyMw==")
    .WithEnvironment("OPENOBSERVE_ORGANIZATION", "default")
    .WithEnvironment("OPENOBSERVE_STREAMNAME", "default")
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
    .WithEnvironment("TZ", "Asia/Shanghai")
    .WithEnvironment("adminConsole", "true")
    .WithEnvironment("db__provider", "npgsql")
    .WithEnvironment("db__conn", $"Host=postgres;Port=5432;Database=agile_config;Username=postgres;Password={postgresPassword};")
    .WithHttpEndpoint(port: 5000, targetPort: 5000, name: "admin")
    .WithLifetime(ContainerLifetime.Persistent)
    .WaitFor(postgres);

var dbMigrator = builder.AddProject<Projects.CheMa_VNext_DbMigrator>("dbmigrator")
    .WithReference(database)
    .WithReference(redis)
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317")
    .WaitFor(database)
    .WaitFor(redis)
    .WaitFor(openTelemetry);

builder.AddProject<Projects.CheMa_VNext_BackgroundWorker>("background-worker")
    .WithReference(database)
    .WithReference(redis)
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317")
    .WithEnvironment("AgileConfig__nodes", "http://localhost:5000")
    .WaitForCompletion(dbMigrator)
    .WaitFor(database)
    .WaitFor(redis)
    .WaitFor(openTelemetry)
    .WaitFor(agileConfig);

var httpApiHost = builder.AddProject<Projects.CheMa_VNext_HttpApi_Host>("httpapi-host")
    .WithReference(database)
    .WithReference(redis)
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317")
    .WithEnvironment("AgileConfig__nodes", "http://localhost:5000")
    .WaitForCompletion(dbMigrator)
    .WaitFor(database)
    .WaitFor(redis)
    .WaitFor(openTelemetry)
    .WaitFor(agileConfig);

var gateway = builder.AddProject<Projects.CheMa_VNext_Gateway>("gateway")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317")
    .WithEnvironment("AgileConfig__nodes", "http://localhost:5000")
    .WaitFor(httpApiHost)
    .WaitFor(openTelemetry)
    .WaitFor(agileConfig);

builder.AddProject<Projects.CheMa_VNext_Blazor>("blazor")
    .WithReference(httpApiHost)
    .WaitFor(httpApiHost)
    .WaitFor(gateway);

builder.Build().Run();

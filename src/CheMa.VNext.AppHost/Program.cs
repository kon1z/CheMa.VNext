var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", password: postgresPassword, port: 5432)
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("vnextdb", "VNext");

var redis = builder.AddRedis("redis", port: 6379)
    .WithLifetime(ContainerLifetime.Persistent);

var dbMigrator = builder.AddProject<Projects.CheMa_VNext_DbMigrator>("dbmigrator")
    .WithReference(database)
    .WithReference(redis)
    .WaitFor(database)
    .WaitFor(redis);

var httpApiHost = builder.AddProject<Projects.CheMa_VNext_HttpApi_Host>("httpapi-host")
    .WithReference(database)
    .WithReference(redis)
    .WaitForCompletion(dbMigrator)
    .WaitFor(database)
    .WaitFor(redis);

builder.AddProject<Projects.CheMa_VNext_Blazor>("blazor")
    .WithReference(httpApiHost)
    .WaitFor(httpApiHost);

builder.Build().Run();

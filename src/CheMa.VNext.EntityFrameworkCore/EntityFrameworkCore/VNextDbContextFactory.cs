using System;
using System.IO;
using CheMa.VNext.EntityFrameworkCore.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace CheMa.VNext.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class VNextDbContextFactory : IDesignTimeDbContextFactory<VNextDbContext>
{
    public VNextDbContext CreateDbContext(string[] args)
    {
        VNextEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var connectionString = configuration.GetConnectionString("Default")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default");

        var builder = new DbContextOptionsBuilder<VNextDbContext>()
            .UseNpgsql(connectionString)
            .AddInterceptors(new SqlLoggingCommandInterceptor(
                NullLogger<SqlLoggingCommandInterceptor>.Instance,
                configuration));

        return new VNextDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../CheMa.VNext.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}

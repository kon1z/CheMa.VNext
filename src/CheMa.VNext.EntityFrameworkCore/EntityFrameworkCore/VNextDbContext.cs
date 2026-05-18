using CheMa.VNext.EntityFrameworkCore.Logging;
using CheMa.VNext.OpenPlatform;
using CheMa.VNext.VehicleDevices;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Volo.Abp.Users;

namespace CheMa.VNext.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ReplaceDbContext(typeof(IPermissionManagementDbContext))]
[ReplaceDbContext(typeof(ISettingManagementDbContext))]
[ReplaceDbContext(typeof(IFeatureManagementDbContext))]
[ConnectionStringName("Default")]
public class VNextDbContext :
    AbpDbContext<VNextDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext,
    IPermissionManagementDbContext,
    ISettingManagementDbContext,
    IFeatureManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    public DbSet<VehicleDevice> VehicleDevices { get; set; }
    public DbSet<OpenApp> OpenApps { get; set; }
    public DbSet<OpenApiAccessLog> OpenApiAccessLogs { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }
    // Permission Management
    public DbSet<PermissionGroupDefinitionRecord> PermissionGroups { get; set; }
    public DbSet<PermissionDefinitionRecord> Permissions { get; set; }
    public DbSet<PermissionGrant> PermissionGrants { get; set; }
    public DbSet<ResourcePermissionGrant> ResourcePermissionGrants { get; set; }
    // Setting Management
    public DbSet<Setting> Settings { get; set; }
    public DbSet<SettingDefinitionRecord> SettingDefinitionRecords { get; set; }
    // Feature Management
    public DbSet<FeatureGroupDefinitionRecord> FeatureGroups { get; set; }
    public DbSet<FeatureDefinitionRecord> Features { get; set; }
    public DbSet<FeatureValue> FeatureValues { get; set; }

    #endregion

    public VNextDbContext(DbContextOptions<VNextDbContext> options)
        : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (LazyServiceProvider != null)
        {
            optionsBuilder.AddInterceptors(LazyServiceProvider.LazyGetRequiredService<SqlLoggingCommandInterceptor>());
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        builder.Entity<VehicleDevice>(b =>
        {
            b.ToTable(VNextConsts.DbTablePrefix + "VehicleDevices", VNextConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Brand)
                .IsRequired()
                .HasMaxLength(VehicleDeviceConsts.MaxBrandLength);

            b.Property(x => x.VendorDeviceId)
                .IsRequired()
                .HasMaxLength(VehicleDeviceConsts.MaxVendorDeviceIdLength);

            b.Property(x => x.Vin)
                .IsRequired()
                .HasMaxLength(VehicleDeviceConsts.MaxVinLength);

            b.Property(x => x.Status)
                .IsRequired();

            b.HasIndex(x => x.VehicleId)
                .IsUnique()
                .HasFilter("\"Status\" = 1");

            b.HasIndex(x => new { x.Brand, x.VendorDeviceId })
                .IsUnique()
                .HasFilter("\"Status\" = 1");
        });

        builder.Entity<OpenApp>(b =>
        {
            b.ToTable(VNextConsts.DbTablePrefix + "OpenApps", VNextConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(OpenPlatformConsts.MaxNameLength);

            b.Property(x => x.ClientId)
                .IsRequired()
                .HasMaxLength(OpenPlatformConsts.MaxClientIdLength);

            b.Property(x => x.AppSecretCipherText)
                .IsRequired()
                .HasMaxLength(OpenPlatformConsts.MaxSecretCipherTextLength);

            b.Property(x => x.AppSecretMaskedHint)
                .IsRequired()
                .HasMaxLength(OpenPlatformConsts.MaxSecretMaskedHintLength);

            b.Property(x => x.AllowedIpRanges)
                .HasMaxLength(OpenPlatformConsts.MaxIpRangesLength);

            b.Property(x => x.Description)
                .HasMaxLength(OpenPlatformConsts.MaxDescriptionLength);

            b.Property(x => x.LastAccessIp)
                .HasMaxLength(OpenPlatformConsts.MaxRemoteIpAddressLength);

            b.HasIndex(x => x.ClientId)
                .IsUnique();

            b.HasIndex(x => x.Status);
        });

        builder.Entity<OpenApiAccessLog>(b =>
        {
            b.ToTable(VNextConsts.DbTablePrefix + "OpenApiAccessLogs", VNextConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.ClientId)
                .HasMaxLength(OpenPlatformConsts.MaxClientIdLength);

            b.Property(x => x.RequestPath)
                .IsRequired()
                .HasMaxLength(OpenPlatformConsts.MaxRequestPathLength);

            b.Property(x => x.HttpMethod)
                .IsRequired()
                .HasMaxLength(OpenPlatformConsts.MaxHttpMethodLength);

            b.Property(x => x.QueryString)
                .HasMaxLength(OpenPlatformConsts.MaxQueryStringLength);

            b.Property(x => x.TraceId)
                .HasMaxLength(OpenPlatformConsts.MaxTraceIdLength);

            b.Property(x => x.RemoteIpAddress)
                .HasMaxLength(OpenPlatformConsts.MaxRemoteIpAddressLength);

            b.Property(x => x.UserAgent)
                .HasMaxLength(OpenPlatformConsts.MaxUserAgentLength);

            b.Property(x => x.FailureCode)
                .HasMaxLength(OpenPlatformConsts.MaxFailureCodeLength);

            b.Property(x => x.FailureMessage)
                .HasMaxLength(OpenPlatformConsts.MaxFailureMessageLength);

            b.HasIndex(x => x.ClientId);
            b.HasIndex(x => x.Timestamp);
            b.HasIndex(x => x.Succeeded);
            b.HasIndex(x => x.RequestPath);
        });
    }
}

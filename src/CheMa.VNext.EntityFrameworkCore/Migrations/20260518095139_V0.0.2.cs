using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheMa.VNext.Migrations
{
    /// <inheritdoc />
    public partial class V002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppOpenApiAccessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenAppId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RequestPath = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    HttpMethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    QueryString = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    TraceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RemoteIpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ElapsedMs = table.Column<long>(type: "bigint", nullable: false),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    FailureCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FailureMessage = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ResponseStatusCode = table.Column<int>(type: "integer", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppOpenApiAccessLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppOpenApps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ClientId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AppSecretCipherText = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    AppSecretMaskedHint = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BeginTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllowedIpRanges = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    LastAccessTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAccessIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppOpenApps", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppOpenApiAccessLogs_ClientId",
                table: "AppOpenApiAccessLogs",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AppOpenApiAccessLogs_RequestPath",
                table: "AppOpenApiAccessLogs",
                column: "RequestPath");

            migrationBuilder.CreateIndex(
                name: "IX_AppOpenApiAccessLogs_Succeeded",
                table: "AppOpenApiAccessLogs",
                column: "Succeeded");

            migrationBuilder.CreateIndex(
                name: "IX_AppOpenApiAccessLogs_Timestamp",
                table: "AppOpenApiAccessLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AppOpenApps_ClientId",
                table: "AppOpenApps",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppOpenApps_Status",
                table: "AppOpenApps",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppOpenApiAccessLogs");

            migrationBuilder.DropTable(
                name: "AppOpenApps");
        }
    }
}

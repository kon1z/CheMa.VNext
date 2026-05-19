using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheMa.VNext.Migrations
{
    /// <inheritdoc />
    public partial class V103_AddVehicleControlAuthorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppVehicleControlAuthorizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenAppId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleVin = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DeviceVin = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VendorDeviceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AuthorizationStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuthorizationEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_AppVehicleControlAuthorizations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppVehicleControlAuthorizations_OpenAppId_VehicleDeviceId",
                table: "AppVehicleControlAuthorizations",
                columns: new[] { "OpenAppId", "VehicleDeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppVehicleControlAuthorizations_OpenAppId",
                table: "AppVehicleControlAuthorizations",
                column: "OpenAppId");

            migrationBuilder.CreateIndex(
                name: "IX_AppVehicleControlAuthorizations_VehicleDeviceId",
                table: "AppVehicleControlAuthorizations",
                column: "VehicleDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppVehicleControlAuthorizations_VehicleId",
                table: "AppVehicleControlAuthorizations",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppVehicleControlAuthorizations");
        }
    }
}

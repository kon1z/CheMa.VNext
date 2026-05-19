using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheMa.VNext.Migrations
{
    /// <inheritdoc />
    public partial class V102 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppVehicleDevices_Brand_VendorDeviceId",
                table: "AppVehicleDevices");

            migrationBuilder.DropIndex(
                name: "IX_AppVehicleDevices_VehicleId",
                table: "AppVehicleDevices");

            migrationBuilder.DropColumn(
                name: "BoundTime",
                table: "AppVehicleDevices");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "AppVehicleDevices");

            migrationBuilder.DropColumn(
                name: "UnboundTime",
                table: "AppVehicleDevices");

            migrationBuilder.DropColumn(
                name: "Vin",
                table: "AppVehicleDevices");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "AppVehicleDevices",
                newName: "DeviceType");

            migrationBuilder.AlterColumn<int>(
                name: "DeviceType",
                table: "AppVehicles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "VehicleId",
                table: "AppVehicleDevices",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_AppVehicleDevices_DeviceType_VendorDeviceId",
                table: "AppVehicleDevices",
                columns: new[] { "DeviceType", "VendorDeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppVehicleDevices_VehicleId",
                table: "AppVehicleDevices",
                column: "VehicleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppVehicleDevices_DeviceType_VendorDeviceId",
                table: "AppVehicleDevices");

            migrationBuilder.DropIndex(
                name: "IX_AppVehicleDevices_VehicleId",
                table: "AppVehicleDevices");

            migrationBuilder.RenameColumn(
                name: "DeviceType",
                table: "AppVehicleDevices",
                newName: "Status");

            migrationBuilder.AlterColumn<int>(
                name: "DeviceType",
                table: "AppVehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "VehicleId",
                table: "AppVehicleDevices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BoundTime",
                table: "AppVehicleDevices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "AppVehicleDevices",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UnboundTime",
                table: "AppVehicleDevices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Vin",
                table: "AppVehicleDevices",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AppVehicleDevices_Brand_VendorDeviceId",
                table: "AppVehicleDevices",
                columns: new[] { "Brand", "VendorDeviceId" },
                unique: true,
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AppVehicleDevices_VehicleId",
                table: "AppVehicleDevices",
                column: "VehicleId",
                unique: true,
                filter: "\"Status\" = 1");
        }
    }
}

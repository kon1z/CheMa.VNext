using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheMa.VNext.Migrations
{
    /// <inheritdoc />
    public partial class V013 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeviceType",
                table: "AppVehicles",
                newName: "VendorType");

            migrationBuilder.RenameColumn(
                name: "DeviceType",
                table: "AppVehicleDevices",
                newName: "VendorType");

            migrationBuilder.RenameIndex(
                name: "IX_AppVehicleDevices_DeviceType_VendorDeviceId",
                table: "AppVehicleDevices",
                newName: "IX_AppVehicleDevices_VendorType_VendorDeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VendorType",
                table: "AppVehicles",
                newName: "DeviceType");

            migrationBuilder.RenameColumn(
                name: "VendorType",
                table: "AppVehicleDevices",
                newName: "DeviceType");

            migrationBuilder.RenameIndex(
                name: "IX_AppVehicleDevices_VendorType_VendorDeviceId",
                table: "AppVehicleDevices",
                newName: "IX_AppVehicleDevices_DeviceType_VendorDeviceId");
        }
    }
}

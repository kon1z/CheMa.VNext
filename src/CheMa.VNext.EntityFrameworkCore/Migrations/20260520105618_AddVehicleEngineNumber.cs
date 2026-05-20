using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheMa.VNext.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleEngineNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EngineNumber",
                table: "AppVehicles",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "AppVehicles");

            migrationBuilder.DropColumn(
                name: "Series",
                table: "AppVehicles");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "AppVehicles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EngineNumber",
                table: "AppVehicles");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "AppVehicles",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Series",
                table: "AppVehicles",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "AppVehicles",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }
    }
}

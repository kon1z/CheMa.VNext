using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheMa.VNext.EntityFrameworkCore.Migrations
{
    public partial class V104_OpenPlatformFullContract : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReadyToHelpAPI.ReadyToHelpAPI.Migrations.App
{
    /// <inheritdoc />
    public partial class Remove2Collumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "reports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReadyToHelpAPI.Migrations.Report
{
    /// <inheritdoc />
    public partial class AddReportFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Reports",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Reports",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Reports",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Reports");
        }
    }
}

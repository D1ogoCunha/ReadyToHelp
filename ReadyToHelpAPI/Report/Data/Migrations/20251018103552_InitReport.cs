using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReadyToHelpAPI.Report.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Reports",
                table: "Reports");

            migrationBuilder.RenameTable(
                name: "Reports",
                newName: "reports");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_UserId_ReportDateTime",
                table: "reports",
                newName: "IX_reports_UserId_ReportDateTime");

            migrationBuilder.AddPrimaryKey(
                name: "PK_reports",
                table: "reports",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_reports",
                table: "reports");

            migrationBuilder.RenameTable(
                name: "reports",
                newName: "Reports");

            migrationBuilder.RenameIndex(
                name: "IX_reports_UserId_ReportDateTime",
                table: "Reports",
                newName: "IX_Reports_UserId_ReportDateTime");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reports",
                table: "Reports",
                column: "Id");
        }
    }
}

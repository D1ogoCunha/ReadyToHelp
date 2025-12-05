using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReadyToHelpAPI.ReadyToHelpAPI.Migrations.App
{
    /// <inheritdoc />
    public partial class MakeReportIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_occurrences_ResponsibleEntityId",
                table: "occurrences",
                column: "ResponsibleEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_occurrences_responsible_entity",
                table: "occurrences",
                column: "ResponsibleEntityId",
                principalTable: "responsible_entities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_occurrences_responsible_entity",
                table: "occurrences");

            migrationBuilder.DropIndex(
                name: "IX_occurrences_ResponsibleEntityId",
                table: "occurrences");
        }
    }
}

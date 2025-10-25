using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReadyToHelpAPI.ReadyToHelpAPI.Migrations.App
{
    /// <inheritdoc />
    public partial class RemoveFeedbackExtraColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_feedback_occurrences_OccurrenceId1",
                table: "feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_feedback_users_UserId1",
                table: "feedback");

            migrationBuilder.DropIndex(
                name: "IX_feedback_OccurrenceId1",
                table: "feedback");

            migrationBuilder.DropIndex(
                name: "IX_feedback_UserId1",
                table: "feedback");

            migrationBuilder.DropColumn(
                name: "OccurrenceId1",
                table: "feedback");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "feedback");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OccurrenceId1",
                table: "feedback",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "feedback",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_feedback_OccurrenceId1",
                table: "feedback",
                column: "OccurrenceId1");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_UserId1",
                table: "feedback",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_feedback_occurrences_OccurrenceId1",
                table: "feedback",
                column: "OccurrenceId1",
                principalTable: "occurrences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_feedback_users_UserId1",
                table: "feedback",
                column: "UserId1",
                principalTable: "users",
                principalColumn: "Id");
        }
    }
}

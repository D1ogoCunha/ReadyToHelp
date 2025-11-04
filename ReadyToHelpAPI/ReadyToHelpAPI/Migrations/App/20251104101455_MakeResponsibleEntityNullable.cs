using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReadyToHelpAPI.ReadyToHelpAPI.Migrations
{
    /// <inheritdoc />
    public partial class MakeResponsibleEntityNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ResponsibleEntityId",
                table: "occurrences",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ResponsibleEntityId",
                table: "occurrences",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace ReadyToHelpAPI.ReadyToHelpAPI.Migrations.App
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Geometry>(
                name: "GeoArea",
                table: "responsible_entities",
                type: "geometry(MultiPolygon,4326)",
                nullable: true,
                oldClrType: typeof(Polygon),
                oldType: "geometry");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Polygon>(
                name: "GeoArea",
                table: "responsible_entities",
                type: "geometry",
                nullable: false,
                oldClrType: typeof(Geometry),
                oldType: "geometry(MultiPolygon,4326)",
                oldNullable: true);
        }
    }
}

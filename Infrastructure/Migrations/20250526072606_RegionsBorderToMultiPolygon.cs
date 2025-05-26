using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RegionsBorderToMultiPolygon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<MultiPolygon>(
                name: "border",
                table: "regions",
                type: "geometry (multipolygon)",
                nullable: false,
                oldClrType: typeof(Polygon),
                oldType: "geometry (polygon)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Polygon>(
                name: "border",
                table: "regions",
                type: "geometry (polygon)",
                nullable: false,
                oldClrType: typeof(MultiPolygon),
                oldType: "geometry (multipolygon)");
        }
    }
}

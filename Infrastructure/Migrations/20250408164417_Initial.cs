using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "historical_routes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    image_path = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historical_routes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "regions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    geometry = table.Column<Polygon>(type: "geometry (polygon)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "route_points",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    coordinates = table.Column<Point>(type: "geometry", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    area_description = table.Column<string>(type: "text", nullable: true),
                    historical_description = table.Column<string>(type: "text", nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_route_points", x => x.id);
                    table.ForeignKey(
                        name: "fk_route_points_historical_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "historical_routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "highlighted_region_in_routes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    region_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_highlighted_region_in_routes", x => x.id);
                    table.ForeignKey(
                        name: "fk_highlighted_region_in_routes_historical_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "historical_routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_highlighted_region_in_routes_regions_region_id",
                        column: x => x.region_id,
                        principalTable: "regions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_highlighted_region_in_routes_region_id",
                table: "highlighted_region_in_routes",
                column: "region_id");

            migrationBuilder.CreateIndex(
                name: "ix_highlighted_region_in_routes_route_id",
                table: "highlighted_region_in_routes",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "ix_route_points_route_id",
                table: "route_points",
                column: "route_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "highlighted_region_in_routes");

            migrationBuilder.DropTable(
                name: "route_points");

            migrationBuilder.DropTable(
                name: "regions");

            migrationBuilder.DropTable(
                name: "historical_routes");
        }
    }
}

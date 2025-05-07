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
                name: "historical_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    marker_image_path = table.Column<string>(type: "text", nullable: true),
                    line_color = table.Column<string>(type: "text", nullable: false),
                    line_style = table.Column<int>(type: "integer", nullable: false),
                    marker_legend = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historical_lines", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "regions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    border = table.Column<Polygon>(type: "geometry (polygon)", nullable: false),
                    display_title = table.Column<string>(type: "text", nullable: true),
                    display_title_font_size = table.Column<int>(type: "integer", nullable: false),
                    display_title_position = table.Column<Point>(type: "geometry", nullable: false),
                    show_display_title = table.Column<bool>(type: "boolean", nullable: false),
                    fill_color = table.Column<string>(type: "text", nullable: false),
                    show_indicators = table.Column<bool>(type: "boolean", nullable: false),
                    is_russia = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "route_objects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    coordinates = table.Column<Point>(type: "geometry", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    image_path = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    video_url = table.Column<string>(type: "text", nullable: true),
                    line_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_route_objects", x => x.id);
                    table.ForeignKey(
                        name: "fk_route_objects_historical_lines_line_id",
                        column: x => x.line_id,
                        principalTable: "historical_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "region_in_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    region_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_region_in_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_region_in_lines_historical_lines_line_id",
                        column: x => x.line_id,
                        principalTable: "historical_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_region_in_lines_regions_region_id",
                        column: x => x.region_id,
                        principalTable: "regions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "region_indicators",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    region_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_path = table.Column<string>(type: "text", nullable: true),
                    excursions = table.Column<int>(type: "integer", nullable: false),
                    partners = table.Column<int>(type: "integer", nullable: false),
                    participants = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_region_indicators", x => x.id);
                    table.ForeignKey(
                        name: "fk_region_indicators_regions_region_id",
                        column: x => x.region_id,
                        principalTable: "regions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_region_in_lines_line_id",
                table: "region_in_lines",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "ix_region_in_lines_region_id",
                table: "region_in_lines",
                column: "region_id");

            migrationBuilder.CreateIndex(
                name: "ix_region_indicators_region_id",
                table: "region_indicators",
                column: "region_id");

            migrationBuilder.CreateIndex(
                name: "ix_route_objects_line_id",
                table: "route_objects",
                column: "line_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "region_in_lines");

            migrationBuilder.DropTable(
                name: "region_indicators");

            migrationBuilder.DropTable(
                name: "route_objects");

            migrationBuilder.DropTable(
                name: "regions");

            migrationBuilder.DropTable(
                name: "historical_lines");
        }
    }
}

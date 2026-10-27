using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Olympus.Api.Migrations
{
    /// <inheritdoc />
    public partial class StoreForumImagesInDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "PublicacionesForo");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImagenData",
                table: "PublicacionesForo",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagenMimeType",
                table: "PublicacionesForo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagenData",
                table: "PublicacionesForo");

            migrationBuilder.DropColumn(
                name: "ImagenMimeType",
                table: "PublicacionesForo");

            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "PublicacionesForo",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Olympus.Api.Migrations
{
    /// <inheritdoc />
    public partial class MarkPendingRoutineDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PlanesEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("298b481f-3029-eb6c-e913-b4cfe34f6753"),
                column: "FechaInicio",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "PlanesEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("4704d5d4-00d5-62db-0ff2-4f69c99f7a92"),
                column: "FechaInicio",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "PlanesEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("a0c45bec-4576-857c-bb17-d9936e0de2bd"),
                column: "FechaInicio",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PlanesEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("298b481f-3029-eb6c-e913-b4cfe34f6753"),
                column: "FechaInicio",
                value: new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "PlanesEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("4704d5d4-00d5-62db-0ff2-4f69c99f7a92"),
                column: "FechaInicio",
                value: new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "PlanesEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("a0c45bec-4576-857c-bb17-d9936e0de2bd"),
                column: "FechaInicio",
                value: new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc));
        }
    }
}

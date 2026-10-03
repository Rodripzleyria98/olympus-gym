using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Olympus.Api.Migrations
{
    /// <inheritdoc />
    public partial class OfficialMembershipPasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClasesIncluidas",
                table: "PlanesMembresia",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "PlanesMembresia"
                SET
                    "Nombre" = CASE "Id"
                        WHEN 1 THEN 'Pase Libre'
                        WHEN 2 THEN 'Pase 3 veces por semana (12 clases)'
                        WHEN 3 THEN 'Pase 2 veces por semana (8 clases)'
                    END,
                    "Descripcion" = CASE "Id"
                        WHEN 1 THEN 'Acceso total e ilimitado a la sala de musculación, máquinas y todas las instalaciones durante todo el mes.'
                        WHEN 2 THEN 'Incluye 12 clases o asistencias al mes para entrenar 3 días por semana según tu disponibilidad.'
                        WHEN 3 THEN 'Incluye 8 clases o asistencias al mes para entrenar 2 días por semana.'
                    END,
                    "Precio" = CASE "Id" WHEN 1 THEN 28000.00 WHEN 2 THEN 26000.00 WHEN 3 THEN 22000.00 END,
                    "DuracionDias" = 30,
                    "ClasesIncluidas" = CASE "Id" WHEN 1 THEN NULL WHEN 2 THEN 12 WHEN 3 THEN 8 END,
                    "IsActivo" = TRUE
                WHERE "Id" IN (1, 2, 3);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "PlanesMembresia"
                SET
                    "Nombre" = CASE "Id" WHEN 1 THEN 'Plan Bronce' WHEN 2 THEN 'Plan Plata' WHEN 3 THEN 'Plan Oro' END,
                    "Descripcion" = CASE "Id"
                        WHEN 1 THEN 'Membresía mensual Olympus.'
                        WHEN 2 THEN 'Membresía trimestral Olympus.'
                        WHEN 3 THEN 'Membresía anual Olympus.'
                    END,
                    "Precio" = CASE "Id" WHEN 1 THEN 499.00 WHEN 2 THEN 1299.00 WHEN 3 THEN 3999.00 END,
                    "DuracionDias" = CASE "Id" WHEN 1 THEN 30 WHEN 2 THEN 90 WHEN 3 THEN 365 END
                WHERE "Id" IN (1, 2, 3);
                """);

            migrationBuilder.DropColumn(
                name: "ClasesIncluidas",
                table: "PlanesMembresia");
        }
    }
}

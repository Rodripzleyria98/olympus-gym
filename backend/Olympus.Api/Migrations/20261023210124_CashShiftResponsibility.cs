using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Olympus.Api.Migrations
{
    /// <inheritdoc />
    public partial class CashShiftResponsibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResponsableTurno",
                table: "CajasTurno",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsableTurnoNormalizado",
                table: "CajasTurno",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.Sql("""
                WITH responsables AS (
                    SELECT
                        caja."Id",
                        concat(usuario."Nombre", ' ', usuario."Apellido") AS nombre,
                        upper(regexp_replace(btrim(concat(usuario."Nombre", ' ', usuario."Apellido")), '\s+', ' ', 'g')) AS normalizado,
                        row_number() OVER (
                            PARTITION BY caja."Fecha",
                                upper(regexp_replace(btrim(concat(usuario."Nombre", ' ', usuario."Apellido")), '\s+', ' ', 'g'))
                            ORDER BY caja."Id"
                        ) AS repeticion
                    FROM "CajasTurno" AS caja
                    INNER JOIN "Usuarios" AS usuario ON usuario."Id" = caja."UsuarioAperturaId"
                )
                UPDATE "CajasTurno" AS caja
                SET "ResponsableTurno" = responsables.nombre,
                    "ResponsableTurnoNormalizado" = CASE
                        WHEN responsables.repeticion = 1 THEN responsables.normalizado
                        ELSE left(responsables.normalizado, 200) || '#LEGACY-' || caja."Id"::text
                    END
                FROM responsables
                WHERE caja."Id" = responsables."Id";
                """);

            migrationBuilder.AlterColumn<string>(
                name: "ResponsableTurno",
                table: "CajasTurno",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ResponsableTurnoNormalizado",
                table: "CajasTurno",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CajasTurno_Fecha_ResponsableTurnoNormalizado",
                table: "CajasTurno",
                columns: new[] { "Fecha", "ResponsableTurnoNormalizado" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CajasTurno_Fecha_ResponsableTurnoNormalizado",
                table: "CajasTurno");

            migrationBuilder.DropColumn(
                name: "ResponsableTurno",
                table: "CajasTurno");

            migrationBuilder.DropColumn(
                name: "ResponsableTurnoNormalizado",
                table: "CajasTurno");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Olympus.Api.Migrations
{
    /// <inheritdoc />
    public partial class AllowSameResponsibleAcrossShifts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CajasTurno_Fecha_ResponsableTurnoNormalizado",
                table: "CajasTurno");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CajasTurno_Fecha_ResponsableTurnoNormalizado",
                table: "CajasTurno",
                columns: new[] { "Fecha", "ResponsableTurnoNormalizado" },
                unique: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Olympus.Api.Migrations
{
    /// <inheritdoc />
    public partial class MercadoPagoPaymentTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaPago",
                table: "Pagos",
                newName: "FechaCreacion");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAcreditacion",
                table: "Pagos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PeriodoMeses",
                table: "Pagos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "PreferenciaId",
                table: "Pagos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransaccionExternaId",
                table: "Pagos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_PreferenciaId",
                table: "Pagos",
                column: "PreferenciaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_TransaccionExternaId",
                table: "Pagos",
                column: "TransaccionExternaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pagos_PreferenciaId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_TransaccionExternaId",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "FechaAcreditacion",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "PeriodoMeses",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "PreferenciaId",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "TransaccionExternaId",
                table: "Pagos");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "Pagos",
                newName: "FechaPago");
        }
    }
}

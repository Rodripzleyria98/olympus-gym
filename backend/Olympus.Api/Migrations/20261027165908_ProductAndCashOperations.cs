using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Olympus.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProductAndCashOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Productos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "General");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Productos",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActivo",
                table: "Productos",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Sabor",
                table: "Productos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaHoraApertura",
                table: "CajasTurno",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaHoraCierre",
                table: "CajasTurno",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "IsActivo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Sabor",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FechaHoraApertura",
                table: "CajasTurno");

            migrationBuilder.DropColumn(
                name: "FechaHoraCierre",
                table: "CajasTurno");
        }
    }
}

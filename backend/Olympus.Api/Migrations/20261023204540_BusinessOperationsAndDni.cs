using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Olympus.Api.Migrations
{
    /// <inheritdoc />
    public partial class BusinessOperationsAndDni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Dni",
                table: "Usuarios",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql("""
                WITH usuarios_sin_dni AS (
                    SELECT "Id", row_number() OVER (ORDER BY "Id") AS numero
                    FROM "Usuarios"
                    WHERE "Dni" IS NULL
                )
                UPDATE "Usuarios" AS usuario
                SET "Dni" = 'LEGACY' || lpad(usuarios_sin_dni.numero::text, 14, '0')
                FROM usuarios_sin_dni
                WHERE usuario."Id" = usuarios_sin_dni."Id";
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Dni",
                table: "Usuarios",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CajaTurnoId",
                table: "Pagos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CajasTurno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Turno = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UsuarioAperturaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioCierreId = table.Column<Guid>(type: "uuid", nullable: true),
                    MontoInicial = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    MontoFinalEfectivoEsperado = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    MontoFinalEfectivoReal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    Diferencia = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    TotalCobrosEfectivo = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    TotalCobrosDigitales = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CajasTurno", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CajasTurno_Usuarios_UsuarioAperturaId",
                        column: x => x.UsuarioAperturaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CajasTurno_Usuarios_UsuarioCierreId",
                        column: x => x.UsuarioCierreId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StockActual = table.Column<int>(type: "integer", nullable: false),
                    StockMinimo = table.Column<int>(type: "integer", nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasesDiarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Dni = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    MetodoPago = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RegistradoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CajaTurnoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasesDiarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasesDiarios_CajasTurno_CajaTurnoId",
                        column: x => x.CajaTurnoId,
                        principalTable: "CajasTurno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PasesDiarios_Usuarios_RegistradoPorUsuarioId",
                        column: x => x.RegistradoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VentasProducto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CajaTurnoId = table.Column<int>(type: "integer", nullable: true),
                    MetodoPago = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MontoTotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentasProducto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentasProducto_CajasTurno_CajaTurnoId",
                        column: x => x.CajaTurnoId,
                        principalTable: "CajasTurno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentasProducto_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VentaProductoItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaProductoId = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaProductoItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaProductoItem_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentaProductoItem_VentasProducto_VentaProductoId",
                        column: x => x.VentaProductoId,
                        principalTable: "VentasProducto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("1b962856-2686-f954-4417-c696f7ef0f6b"),
                column: "Dni",
                value: "1b962856268");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("7c60eed9-7435-4cdb-92d3-f4c61429b65c"),
                column: "Dni",
                value: "27000004");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("8a619c3f-8bd3-4b5e-9f55-b36f26f573d1"),
                column: "Dni",
                value: "27000005");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("c9e2a6af-c023-4df4-8911-3d56046199a5"),
                column: "Dni",
                value: "27000001");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("cfb0166f-6718-f4db-549b-3f4c87e72df2"),
                column: "Dni",
                value: "27000006");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("eb4a4f6e-fc9b-2ac8-f1ba-d1021d416841"),
                column: "Dni",
                value: "27000002");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("1b962856-2686-f954-4417-c696f7ef0f6b"),
                column: "Dni",
                value: "27000003");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Dni",
                table: "Usuarios",
                column: "Dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_CajaTurnoId",
                table: "Pagos",
                column: "CajaTurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_CajasTurno_Fecha_Turno",
                table: "CajasTurno",
                columns: new[] { "Fecha", "Turno" },
                unique: true,
                filter: "\"Estado\" = 'Abierta'");

            migrationBuilder.CreateIndex(
                name: "IX_CajasTurno_UsuarioAperturaId",
                table: "CajasTurno",
                column: "UsuarioAperturaId");

            migrationBuilder.CreateIndex(
                name: "IX_CajasTurno_UsuarioCierreId",
                table: "CajasTurno",
                column: "UsuarioCierreId");

            migrationBuilder.CreateIndex(
                name: "IX_PasesDiarios_CajaTurnoId",
                table: "PasesDiarios",
                column: "CajaTurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_PasesDiarios_Dni_Fecha",
                table: "PasesDiarios",
                columns: new[] { "Dni", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_PasesDiarios_RegistradoPorUsuarioId",
                table: "PasesDiarios",
                column: "RegistradoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo",
                table: "Productos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentaProductoItem_ProductoId",
                table: "VentaProductoItem",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaProductoItem_VentaProductoId",
                table: "VentaProductoItem",
                column: "VentaProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_VentasProducto_CajaTurnoId",
                table: "VentasProducto",
                column: "CajaTurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_VentasProducto_UsuarioId",
                table: "VentasProducto",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_CajasTurno_CajaTurnoId",
                table: "Pagos",
                column: "CajaTurnoId",
                principalTable: "CajasTurno",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_CajasTurno_CajaTurnoId",
                table: "Pagos");

            migrationBuilder.DropTable(
                name: "PasesDiarios");

            migrationBuilder.DropTable(
                name: "VentaProductoItem");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "VentasProducto");

            migrationBuilder.DropTable(
                name: "CajasTurno");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Dni",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_CajaTurnoId",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "Dni",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CajaTurnoId",
                table: "Pagos");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Olympus.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanesEntrenamiento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaRevision = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesEntrenamiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanesEntrenamiento_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiasEntrenamiento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanEntrenamientoId = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreDia = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    Enfoque = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiasEntrenamiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiasEntrenamiento_PlanesEntrenamiento_PlanEntrenamientoId",
                        column: x => x.PlanEntrenamientoId,
                        principalTable: "PlanesEntrenamiento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ejercicios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiaEntrenamientoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SeriesYRepeticiones = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ejercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ejercicios_DiasEntrenamiento_DiaEntrenamientoId",
                        column: x => x.DiaEntrenamientoId,
                        principalTable: "DiasEntrenamiento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiasEntrenamiento_PlanEntrenamientoId_Orden",
                table: "DiasEntrenamiento",
                columns: new[] { "PlanEntrenamientoId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_DiaEntrenamientoId_Orden",
                table: "Ejercicios",
                columns: new[] { "DiaEntrenamientoId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanesEntrenamiento_UsuarioId_IsActivo",
                table: "PlanesEntrenamiento",
                columns: new[] { "UsuarioId", "IsActivo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ejercicios");

            migrationBuilder.DropTable(
                name: "DiasEntrenamiento");

            migrationBuilder.DropTable(
                name: "PlanesEntrenamiento");
        }
    }
}

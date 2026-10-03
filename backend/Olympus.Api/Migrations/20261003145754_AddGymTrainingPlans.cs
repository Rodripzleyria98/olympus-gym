using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Olympus.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGymTrainingPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO "Usuarios" ("Id", "Apellido", "Email", "FechaRegistro", "Nombre", "PasswordHash", "Rol")
                VALUES
                    ('1b962856-2686-f954-4417-c696f7ef0f6b', 'Paez', 'fede.paez@olympus.com', TIMESTAMPTZ '2026-09-01 00:00:00+00', 'Fede', '$2a$11$kdAkRdmLwFGjXeiBzhlLhu.qDKMnn8S2O.Oi9lHm2w5djynoWNTVC', 'User'),
                    ('7c60eed9-7435-4cdb-92d3-f4c61429b65c', 'Socio', 'karen@olympus.com', TIMESTAMPTZ '2026-09-01 00:00:00+00', 'Karen', '$2a$11$kdAkRdmLwFGjXeiBzhlLhu.qDKMnn8S2O.Oi9lHm2w5djynoWNTVC', 'User'),
                    ('8a619c3f-8bd3-4b5e-9f55-b36f26f573d1', 'Socio', 'bianca@olympus.com', TIMESTAMPTZ '2026-09-01 00:00:00+00', 'Bianca', '$2a$11$kdAkRdmLwFGjXeiBzhlLhu.qDKMnn8S2O.Oi9lHm2w5djynoWNTVC', 'User'),
                    ('c9e2a6af-c023-4df4-8911-3d56046199a5', 'Garay', 'heber.garay@olympus.com', TIMESTAMPTZ '2026-09-01 00:00:00+00', 'Heber', '$2a$11$kdAkRdmLwFGjXeiBzhlLhu.qDKMnn8S2O.Oi9lHm2w5djynoWNTVC', 'User'),
                    ('cfb0166f-6718-f4db-549b-3f4c87e72df2', 'Boneto', 'matias.boneto@olympus.com', TIMESTAMPTZ '2026-09-01 00:00:00+00', 'Matias', '$2a$11$kdAkRdmLwFGjXeiBzhlLhu.qDKMnn8S2O.Oi9lHm2w5djynoWNTVC', 'User'),
                    ('eb4a4f6e-fc9b-2ac8-f1ba-d1021d416841', 'Acevedo', 'joel.acevedo@olympus.com', TIMESTAMPTZ '2026-09-01 00:00:00+00', 'Joel', '$2a$11$kdAkRdmLwFGjXeiBzhlLhu.qDKMnn8S2O.Oi9lHm2w5djynoWNTVC', 'User')
                ON CONFLICT ("Id") DO UPDATE SET
                    "Apellido" = EXCLUDED."Apellido",
                    "Email" = EXCLUDED."Email",
                    "FechaRegistro" = EXCLUDED."FechaRegistro",
                    "Nombre" = EXCLUDED."Nombre",
                    "PasswordHash" = EXCLUDED."PasswordHash",
                    "Rol" = EXCLUDED."Rol";

                UPDATE "PlanesEntrenamiento"
                SET "IsActivo" = FALSE
                WHERE "IsActivo"
                  AND "UsuarioId" IN (
                    '7c60eed9-7435-4cdb-92d3-f4c61429b65c',
                    '8a619c3f-8bd3-4b5e-9f55-b36f26f573d1',
                    'c9e2a6af-c023-4df4-8911-3d56046199a5'
                  )
                  AND "Titulo" IN ('Plan de Fuerza e Hipertrofia', 'Rutina 3 días');
                """);

            migrationBuilder.InsertData(
                table: "PlanesEntrenamiento",
                columns: new[] { "Id", "FechaInicio", "FechaRevision", "IsActivo", "Titulo", "UsuarioId" },
                values: new object[,]
                {
                    { new Guid("092e9ae5-5732-02bb-6fba-c3f752c0bffb"), new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Tren superior, piernas y abdomen - Karen y Bianca", new Guid("8a619c3f-8bd3-4b5e-9f55-b36f26f573d1") },
                    { new Guid("298b481f-3029-eb6c-e913-b4cfe34f6753"), new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Fuerza, potencia unilateral y resistencia - Fede Paez", new Guid("1b962856-2686-f954-4417-c696f7ef0f6b") },
                    { new Guid("4704d5d4-00d5-62db-0ff2-4f69c99f7a92"), new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Tren superior, brazos y cuidado de rodilla - Matias Boneto", new Guid("cfb0166f-6718-f4db-549b-3f4c87e72df2") },
                    { new Guid("a0c45bec-4576-857c-bb17-d9936e0de2bd"), new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Énfasis pierna y cuádriceps - Joel Acevedo", new Guid("eb4a4f6e-fc9b-2ac8-f1ba-d1021d416841") },
                    { new Guid("dcc1ffd7-706f-a997-36bb-502c1a58c16f"), new DateTime(2026, 9, 29, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "Hipertrofia clásica - Heber Garay", new Guid("c9e2a6af-c023-4df4-8911-3d56046199a5") },
                    { new Guid("e06bf964-f1e7-84d1-767b-b1bd7d7b9316"), new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Tren superior, piernas y abdomen - Karen y Bianca", new Guid("7c60eed9-7435-4cdb-92d3-f4c61429b65c") }
                });

            migrationBuilder.InsertData(
                table: "DiasEntrenamiento",
                columns: new[] { "Id", "Enfoque", "NombreDia", "Orden", "PlanEntrenamientoId" },
                values: new object[,]
                {
                    { new Guid("021969a5-e128-cc45-0527-599a61222a2e"), "Hombros + Brazos + Rodilla", "DÍA 2", 2, new Guid("4704d5d4-00d5-62db-0ff2-4f69c99f7a92") },
                    { new Guid("05053489-cf18-a656-cdad-315afd2fbab0"), "Piernas: Fuerza", "LUNES", 1, new Guid("298b481f-3029-eb6c-e913-b4cfe34f6753") },
                    { new Guid("0b48ccf7-6b00-c8e2-e747-351f85711074"), "Piernas completo + Abdomen", "DÍA 2", 2, new Guid("092e9ae5-5732-02bb-6fba-c3f752c0bffb") },
                    { new Guid("13e40f4a-7ffe-406e-fce8-7e166bfdc10c"), "Tren Superior + Core", "MARTES", 2, new Guid("298b481f-3029-eb6c-e913-b4cfe34f6753") },
                    { new Guid("1454f2dd-e385-19ae-6967-4f077bdf2366"), "Hombros + Brazos", "JUEVES", 4, new Guid("a0c45bec-4576-857c-bb17-d9936e0de2bd") },
                    { new Guid("1be5e5fc-d8d2-aea3-b586-46e4987b6bbd"), "Espalda + Brazos", "VIERNES", 5, new Guid("dcc1ffd7-706f-a997-36bb-502c1a58c16f") },
                    { new Guid("1f223952-203a-c90c-b079-c32eea76f6a2"), "Pecho + Espalda + Brazos + Rodilla", "DÍA 3", 3, new Guid("4704d5d4-00d5-62db-0ff2-4f69c99f7a92") },
                    { new Guid("201414bb-c3ca-3d28-d123-7a1837040081"), "Piernas B | Femorales + Glúteos", "VIERNES", 5, new Guid("a0c45bec-4576-857c-bb17-d9936e0de2bd") },
                    { new Guid("2dc45b9d-3d85-09b8-e227-d3e11e33de5f"), "Tren Superior + Rodilla", "DÍA 1", 1, new Guid("4704d5d4-00d5-62db-0ff2-4f69c99f7a92") },
                    { new Guid("31343d54-9c0d-ba97-d1cd-ea25d178b8a3"), "Espalda + Bíceps + Abdomen", "DÍA 3", 3, new Guid("e06bf964-f1e7-84d1-767b-b1bd7d7b9316") },
                    { new Guid("40b3c891-9565-92bf-ad81-f304d7892a89"), "Espalda + Bíceps", "MARTES", 2, new Guid("dcc1ffd7-706f-a997-36bb-502c1a58c16f") },
                    { new Guid("446f9fec-6c67-68c9-749a-f5d827390276"), "Piernas: Resistencia + Estabilidad", "VIERNES", 5, new Guid("298b481f-3029-eb6c-e913-b4cfe34f6753") },
                    { new Guid("80d5545f-200a-29ca-0ed8-06422ed293c4"), "Pecho + Tríceps", "DÍA 1", 1, new Guid("e06bf964-f1e7-84d1-767b-b1bd7d7b9316") },
                    { new Guid("878f3b62-84d7-b340-5070-473adcaafdb9"), "Espalda + Bíceps + Abdomen", "DÍA 3", 3, new Guid("092e9ae5-5732-02bb-6fba-c3f752c0bffb") },
                    { new Guid("8bf516ed-cc46-99ef-7304-975d03bc2bd3"), "Pecho + Tríceps", "DÍA 1", 1, new Guid("092e9ae5-5732-02bb-6fba-c3f752c0bffb") },
                    { new Guid("93754476-dacc-6685-9d41-8d31914f9b9f"), "Tren Superior + Resistencia", "JUEVES", 4, new Guid("298b481f-3029-eb6c-e913-b4cfe34f6753") },
                    { new Guid("9bb57730-3520-0095-449a-ac5680a1f56e"), "Piernas completo + Abdomen", "DÍA 2", 2, new Guid("e06bf964-f1e7-84d1-767b-b1bd7d7b9316") },
                    { new Guid("9ff48d87-9170-d21f-4f3f-41bca40398af"), "Piernas A | Cuádriceps", "MARTES", 2, new Guid("a0c45bec-4576-857c-bb17-d9936e0de2bd") },
                    { new Guid("bd2d4b07-6c0f-b525-d279-1032ed23adfa"), "Piernas: Potencia + Unilateral", "MIÉRCOLES", 3, new Guid("298b481f-3029-eb6c-e913-b4cfe34f6753") },
                    { new Guid("df1f47cb-47c1-ab97-7952-8389bf9052ea"), "Piernas completas", "MIÉRCOLES", 3, new Guid("dcc1ffd7-706f-a997-36bb-502c1a58c16f") },
                    { new Guid("ee199abb-81d7-87eb-7ccb-bf8f966afeb0"), "Espalda + Bíceps", "MIÉRCOLES", 3, new Guid("a0c45bec-4576-857c-bb17-d9936e0de2bd") },
                    { new Guid("f3627e9f-34ac-a786-f3e5-b930b1514e2e"), "Pecho + Tríceps", "LUNES", 1, new Guid("dcc1ffd7-706f-a997-36bb-502c1a58c16f") },
                    { new Guid("f6fa028d-21af-cec2-83ee-af0d5651b407"), "Hombros + Pecho", "JUEVES", 4, new Guid("dcc1ffd7-706f-a997-36bb-502c1a58c16f") },
                    { new Guid("f79a4195-c83d-edf6-9f4c-b68e4b2fddd1"), "Pecho + Tríceps", "LUNES", 1, new Guid("a0c45bec-4576-857c-bb17-d9936e0de2bd") }
                });

            migrationBuilder.InsertData(
                table: "Ejercicios",
                columns: new[] { "Id", "DiaEntrenamientoId", "Nombre", "Notas", "Orden", "SeriesYRepeticiones" },
                values: new object[,]
                {
                    { new Guid("012e4646-fdb2-c223-0eb2-94f354352aa3"), new Guid("446f9fec-6c67-68c9-749a-f5d827390276"), "Plancha lateral", null, 9, "3x30-40 seg por lado" },
                    { new Guid("01e1d1f0-50e5-5299-e8ed-e556606ccfce"), new Guid("021969a5-e128-cc45-0527-599a61222a2e"), "Elevaciones laterales", null, 4, "3x12-15" },
                    { new Guid("032bbb07-951a-9418-ee40-a7026537aaa1"), new Guid("8bf516ed-cc46-99ef-7304-975d03bc2bd3"), "Press inclinado con mancuernas", null, 3, "3x12" },
                    { new Guid("038e055d-a239-2fc8-f3fa-c2daaa4a5ace"), new Guid("2dc45b9d-3d85-09b8-e227-d3e11e33de5f"), "Elevaciones laterales", null, 5, "3x12-15" },
                    { new Guid("069ec3bc-7e2f-f533-d666-cb16b111e989"), new Guid("446f9fec-6c67-68c9-749a-f5d827390276"), "Extensión de cuádriceps", null, 6, "2x15" },
                    { new Guid("087b14a6-de4c-93d3-6d17-a441d6874746"), new Guid("f79a4195-c83d-edf6-9f4c-b68e4b2fddd1"), "Extensión de tríceps sobre cabeza", null, 7, "3x12-15" },
                    { new Guid("0b9e0ec2-f7df-7535-fd02-9cc6e238ece6"), new Guid("1be5e5fc-d8d2-aea3-b586-46e4987b6bbd"), "Jalón al pecho", null, 1, "4x8-10" },
                    { new Guid("0d2d403f-fed2-6250-a833-980d2c49817a"), new Guid("0b48ccf7-6b00-c8e2-e747-351f85711074"), "Sentadillas profundas", "Calentamiento", 1, "2x10" },
                    { new Guid("0fbf9d40-e024-6915-8ef0-deaa3867aaf2"), new Guid("05053489-cf18-a656-cdad-315afd2fbab0"), "Gemelos de pie", null, 6, "4x12" },
                    { new Guid("10418d71-4f3a-0d6b-7f33-53615a9a7d6a"), new Guid("878f3b62-84d7-b340-5070-473adcaafdb9"), "Curl de bíceps sentado con barra Z", null, 5, "4x10" },
                    { new Guid("131d9dc9-3c06-b2d3-73dd-4231c9d7a695"), new Guid("201414bb-c3ca-3d28-d123-7a1837040081"), "Estocada búlgara", null, 5, "3x10-12 c/pierna" },
                    { new Guid("138c8585-686f-50ba-595f-5dbe9c4d7445"), new Guid("9bb57730-3520-0095-449a-ac5680a1f56e"), "Hip Thrust en máquina", null, 5, "4x10" },
                    { new Guid("15ab88fc-c6ef-c17d-4779-f87e2be87623"), new Guid("f6fa028d-21af-cec2-83ee-af0d5651b407"), "Fondos o press cerrado", null, 7, "3x8-12" },
                    { new Guid("19251d62-93b4-6d09-a8df-ead9c8c9625c"), new Guid("1454f2dd-e385-19ae-6967-4f077bdf2366"), "Face pull", null, 4, "3x12-15" },
                    { new Guid("1966a1e3-8e96-07d1-cb3f-935577811f95"), new Guid("2dc45b9d-3d85-09b8-e227-d3e11e33de5f"), "Jalón al pecho", null, 4, "3x10-12" },
                    { new Guid("19b2bf46-a25e-3f53-abd2-9e0b322246ec"), new Guid("f3627e9f-34ac-a786-f3e5-b930b1514e2e"), "Aperturas en mariposa", null, 4, "3x12-15" },
                    { new Guid("1e330573-98af-ebeb-4b03-b15339c75de8"), new Guid("f6fa028d-21af-cec2-83ee-af0d5651b407"), "Aperturas en mariposa o polea", null, 4, "3x10-15" },
                    { new Guid("207b641b-6656-7c10-001a-b9e0cefaa2a4"), new Guid("1454f2dd-e385-19ae-6967-4f077bdf2366"), "Curl martillo", null, 6, "3x10-12" },
                    { new Guid("214f66ed-c369-6e16-70b7-ef44cc423ff4"), new Guid("1f223952-203a-c90c-b079-c32eea76f6a2"), "Curl bíceps en banco Scott", null, 6, "3x10-12" },
                    { new Guid("228e0415-c265-0d7d-aed1-9e400c2492ff"), new Guid("93754476-dacc-6685-9d41-8d31914f9b9f"), "Press militar con mancuernas", null, 4, "3x10" },
                    { new Guid("23688f06-0f22-26ad-4eb9-5522e70f5eb5"), new Guid("bd2d4b07-6c0f-b525-d279-1032ed23adfa"), "Sentadilla búlgara", "Fuerza", 4, "3x8 por pierna" },
                    { new Guid("24ada14b-400b-6890-bf9c-558cf0b81f1c"), new Guid("93754476-dacc-6685-9d41-8d31914f9b9f"), "Curl bíceps", null, 7, "2x12" },
                    { new Guid("24afed98-565a-f941-846c-6e6058bc9baa"), new Guid("8bf516ed-cc46-99ef-7304-975d03bc2bd3"), "Kickback de tríceps en polea", null, 6, "3x10" },
                    { new Guid("2742556a-be95-4e6e-ff8b-1dd556ebc325"), new Guid("40b3c891-9565-92bf-ad81-f304d7892a89"), "Jalón al pecho", null, 1, "4x6-10" },
                    { new Guid("27bf010e-2a66-2e65-8cd0-7e7e12cbcf56"), new Guid("878f3b62-84d7-b340-5070-473adcaafdb9"), "Plancha", "Abdomen", 8, "1 min" },
                    { new Guid("282003fa-d494-eb6f-cde8-c3cbb1d443f3"), new Guid("f6fa028d-21af-cec2-83ee-af0d5651b407"), "Press militar", null, 1, "4x6-8" },
                    { new Guid("282fbe60-4962-92df-6179-ba759c10326d"), new Guid("0b48ccf7-6b00-c8e2-e747-351f85711074"), "Prensa unilateral", null, 3, "4x12" },
                    { new Guid("2d404713-7d76-a39e-e82d-270f844911a3"), new Guid("021969a5-e128-cc45-0527-599a61222a2e"), "Prensa 45°", "Recorrido corto y cómodo.", 8, "3x10-12" },
                    { new Guid("2e3274e1-5675-db16-d24a-0105ff96e3e5"), new Guid("40b3c891-9565-92bf-ad81-f304d7892a89"), "Curl con barra", null, 6, "3x8-10" },
                    { new Guid("2e9e3057-bf62-40a3-49bb-e3509d4cbf87"), new Guid("40b3c891-9565-92bf-ad81-f304d7892a89"), "Pullover con mancuerna", null, 5, "3x12-15" },
                    { new Guid("2fc0034e-61b6-20c8-b774-5336828ee770"), new Guid("9bb57730-3520-0095-449a-ac5680a1f56e"), "Step UP en cajón", null, 4, "4x10" },
                    { new Guid("30aa2495-5fb1-1da9-1d7a-b345ef1ebef9"), new Guid("ee199abb-81d7-87eb-7ccb-bf8f966afeb0"), "Curl martillo", null, 8, "2-3x12-15" },
                    { new Guid("313dbf3d-d359-88a0-bc4a-cbcfef834ba9"), new Guid("8bf516ed-cc46-99ef-7304-975d03bc2bd3"), "Extensión de tríceps con polea alta", null, 5, "4x10" },
                    { new Guid("34d953c3-f14c-7238-1039-7f88f0a221be"), new Guid("8bf516ed-cc46-99ef-7304-975d03bc2bd3"), "Apertura plana con mancuernas", null, 4, "3x12" },
                    { new Guid("34f080db-9d35-1e03-d06a-2664cfd06e9c"), new Guid("878f3b62-84d7-b340-5070-473adcaafdb9"), "Cruzados en colchoneta", "Abdomen", 9, "3x12" },
                    { new Guid("354d0216-5038-236b-9365-cb70f86708a3"), new Guid("1454f2dd-e385-19ae-6967-4f077bdf2366"), "Curl con barra W", null, 5, "3x8-10" },
                    { new Guid("35986405-dc6c-b6d1-79af-f6060e92ffed"), new Guid("021969a5-e128-cc45-0527-599a61222a2e"), "Curl martillo", null, 6, "3x10-12" },
                    { new Guid("35b778da-121e-f74f-dd95-3dc7fcd917ed"), new Guid("ee199abb-81d7-87eb-7ccb-bf8f966afeb0"), "Pullover con mancuerna", null, 5, "3x12-15" },
                    { new Guid("35e327c8-84e9-aeb2-ff34-d1c67a5899fb"), new Guid("0b48ccf7-6b00-c8e2-e747-351f85711074"), "Abdominales", "Abdomen", 8, "3x15" },
                    { new Guid("368b25d2-9f4c-93e9-9d9c-6671d5ef75a8"), new Guid("f3627e9f-34ac-a786-f3e5-b930b1514e2e"), "Press banca con barra", null, 1, "4x6-8" },
                    { new Guid("36cfe60e-a175-e769-9bd6-5f6823ef887f"), new Guid("13e40f4a-7ffe-406e-fce8-7e166bfdc10c"), "Press inclinado con mancuernas", null, 5, "3x10" },
                    { new Guid("37f6e24e-9875-0775-6cd7-7b2ccecf1286"), new Guid("ee199abb-81d7-87eb-7ccb-bf8f966afeb0"), "Curl inclinado con mancuernas", null, 7, "3x10-12" },
                    { new Guid("3b28771b-60bd-77bf-7178-c19b457f3a22"), new Guid("13e40f4a-7ffe-406e-fce8-7e166bfdc10c"), "Dead bug", null, 8, "3x10 por lado" },
                    { new Guid("3b96aa30-4393-df6b-b0d2-56d0988fde79"), new Guid("9ff48d87-9170-d21f-4f3f-41bca40398af"), "Sentadilla con mancuerna", null, 1, "4x6-8" },
                    { new Guid("40de3b44-a879-bed7-5567-311eca8c6ec5"), new Guid("f6fa028d-21af-cec2-83ee-af0d5651b407"), "Press inclinado", null, 2, "3x8-10" },
                    { new Guid("43d83d48-76da-4dd9-6399-4d7ce9621ea4"), new Guid("9bb57730-3520-0095-449a-ac5680a1f56e"), "Plancha", "Abdomen", 9, "1 min" },
                    { new Guid("45606449-f258-31b4-b055-2c9b0aac8830"), new Guid("201414bb-c3ca-3d28-d123-7a1837040081"), "Extensión de cadera en polea o banda elástica", null, 6, "3x12-15" },
                    { new Guid("48ada616-8539-da39-8a67-9b5cfbbb6524"), new Guid("bd2d4b07-6c0f-b525-d279-1032ed23adfa"), "Lanzamiento de balón medicinal + sentadilla", "Sin golpear el balón. Potencia.", 3, "3x6" },
                    { new Guid("48cd9294-1b40-5ff8-7740-fdaab1ace1ce"), new Guid("0b48ccf7-6b00-c8e2-e747-351f85711074"), "Abductores en polea", null, 6, "3x12" },
                    { new Guid("49b3b90d-c853-d6a6-0a7f-6a7b47ba7f3e"), new Guid("31343d54-9c0d-ba97-d1cd-ea25d178b8a3"), "Cruzados en colchoneta", "Abdomen", 9, "3x12" },
                    { new Guid("4c840285-9f60-4e53-a3d2-daf76ceaab1f"), new Guid("1f223952-203a-c90c-b079-c32eea76f6a2"), "Press pecho en hammer", null, 3, "3x10-12" },
                    { new Guid("4d6e57f0-aa34-d401-2b55-d9590a1344fe"), new Guid("446f9fec-6c67-68c9-749a-f5d827390276"), "Hip thrust", null, 3, "3x10-12" },
                    { new Guid("4daf44e0-3f1f-b310-e4e9-26ff5a210a5e"), new Guid("446f9fec-6c67-68c9-749a-f5d827390276"), "Sentadilla goblet", "Menos carga que el día 1.", 1, "3x12" },
                    { new Guid("4ea5e314-32de-e3fd-e77a-96ac603bbeb2"), new Guid("80d5545f-200a-29ca-0ed8-06422ed293c4"), "Kickback de tríceps en polea", null, 6, "3x10" },
                    { new Guid("4eddb2b3-7872-95f9-026d-4b9f8d775ff3"), new Guid("13e40f4a-7ffe-406e-fce8-7e166bfdc10c"), "Remo con barra", null, 2, "4x6-8" },
                    { new Guid("5399b040-b8fe-7079-a28b-fe737b9f7f0c"), new Guid("1be5e5fc-d8d2-aea3-b586-46e4987b6bbd"), "Curl martillo", null, 6, "3x10-12" },
                    { new Guid("53a9c4c3-531d-8930-c737-fb94cce37506"), new Guid("f3627e9f-34ac-a786-f3e5-b930b1514e2e"), "Extensión de tríceps sobre cabeza", null, 7, "3x12-15" },
                    { new Guid("554c3d09-f139-6409-5d66-3f179f8735fd"), new Guid("2dc45b9d-3d85-09b8-e227-d3e11e33de5f"), "Camilla invertida", null, 8, "3x12-15" },
                    { new Guid("55b269af-7a38-adad-7058-3ce5beb2254f"), new Guid("1be5e5fc-d8d2-aea3-b586-46e4987b6bbd"), "Pullover en polea o mancuerna", null, 4, "3x12-15" },
                    { new Guid("56310411-3692-a6a5-98b3-c8c009243f2c"), new Guid("31343d54-9c0d-ba97-d1cd-ea25d178b8a3"), "Dominadas agarre supino", "Calentamiento", 1, "3x al fallo técnico" },
                    { new Guid("56e1aaed-559f-a2a5-19e7-853c45e0ebcf"), new Guid("9ff48d87-9170-d21f-4f3f-41bca40398af"), "Estocadas caminando", null, 5, "3x10-12 c/pierna" },
                    { new Guid("5b583920-4cde-22f0-2453-97056b00eec6"), new Guid("446f9fec-6c67-68c9-749a-f5d827390276"), "Prensa", null, 2, "3x12" },
                    { new Guid("5b639215-f34a-c8b4-90ef-1a1e76bb026c"), new Guid("0b48ccf7-6b00-c8e2-e747-351f85711074"), "Hip Thrust en máquina", null, 5, "4x10" },
                    { new Guid("5c0f90f4-9c6b-4142-2caa-e24d1eb58fc8"), new Guid("446f9fec-6c67-68c9-749a-f5d827390276"), "Curl femoral", null, 5, "3x12-15" },
                    { new Guid("5c5993a0-b55f-6bc6-5afd-d6bd5e2a6065"), new Guid("021969a5-e128-cc45-0527-599a61222a2e"), "Camilla invertida", null, 9, "3x12-15" },
                    { new Guid("5cd8caa8-8f6b-9451-6829-027b90af0595"), new Guid("40b3c891-9565-92bf-ad81-f304d7892a89"), "Curl inclinado con mancuernas", null, 7, "3x10-12" },
                    { new Guid("5cf6987c-b696-f722-2ca1-f7571d62bc07"), new Guid("1f223952-203a-c90c-b079-c32eea76f6a2"), "Puente de glúteos", null, 9, "3x12-15" },
                    { new Guid("5d525401-a6b8-69cc-f3ae-681eac988652"), new Guid("201414bb-c3ca-3d28-d123-7a1837040081"), "Gemelos", null, 7, "4x12-20" },
                    { new Guid("5f712e5b-3366-31c4-2057-9f4de5cb7dcf"), new Guid("9bb57730-3520-0095-449a-ac5680a1f56e"), "Sentadillas profundas", "Calentamiento", 1, "2x10" },
                    { new Guid("5ff7cbd2-5990-2511-3a39-b57345ef9106"), new Guid("2dc45b9d-3d85-09b8-e227-d3e11e33de5f"), "Press inclinado con mancuernas", null, 3, "3x8-12" },
                    { new Guid("618dcc8e-e109-e4f4-b594-a2472aa51ac5"), new Guid("31343d54-9c0d-ba97-d1cd-ea25d178b8a3"), "Rodilla al pecho en paralelas", "Abdomen", 7, "3x10" },
                    { new Guid("63adc7b5-8b6a-64b2-7720-99f46918e461"), new Guid("f79a4195-c83d-edf6-9f4c-b68e4b2fddd1"), "Press banca con barra", null, 1, "4x6-8" },
                    { new Guid("651628d1-af84-12c1-fb9e-19f6539d7031"), new Guid("df1f47cb-47c1-ab97-7952-8389bf9052ea"), "Gemelos sentado", null, 8, "3x15-20" },
                    { new Guid("6630ee11-f70d-2609-8598-497ba48ae22e"), new Guid("13e40f4a-7ffe-406e-fce8-7e166bfdc10c"), "Pallof press", null, 7, "3x10 por lado" },
                    { new Guid("6760f308-2fb0-d51e-4ed8-ec99c535bd90"), new Guid("9bb57730-3520-0095-449a-ac5680a1f56e"), "Sentadillas frontales con barra", null, 2, "3x10" },
                    { new Guid("67de007a-7fdb-9a5f-654e-90d56d81cb9f"), new Guid("93754476-dacc-6685-9d41-8d31914f9b9f"), "Elevaciones laterales", null, 5, "3x12-15" },
                    { new Guid("687f3c5b-58ab-2295-a7dd-cec9b970961a"), new Guid("021969a5-e128-cc45-0527-599a61222a2e"), "Remo T", null, 2, "3x8-12" },
                    { new Guid("6a1ad330-bda7-4b88-bfdd-a7ac16563683"), new Guid("93754476-dacc-6685-9d41-8d31914f9b9f"), "Resistencia en bicicleta", "Calentamiento + vuelta a la calma", 9, "5 min + 8x(1 min fuerte / 1 min suave) + 5 min" },
                    { new Guid("6c5b3c7d-f0d2-703a-a974-134c6e838f5d"), new Guid("1be5e5fc-d8d2-aea3-b586-46e4987b6bbd"), "Tríceps en polea", null, 8, "3x10-15" },
                    { new Guid("6cc82485-bcdd-6298-dc04-b34faecec9e8"), new Guid("0b48ccf7-6b00-c8e2-e747-351f85711074"), "Pantorrilla parado en máquina", null, 7, "3x12" },
                    { new Guid("71e1043b-f373-0070-b13e-962f2930d595"), new Guid("ee199abb-81d7-87eb-7ccb-bf8f966afeb0"), "Remo con barra o T", null, 2, "4x6-8" },
                    { new Guid("72a59af3-862f-b380-37d6-40d662e7844c"), new Guid("0b48ccf7-6b00-c8e2-e747-351f85711074"), "Step UP en cajón", null, 4, "4x10" },
                    { new Guid("74f5a81a-b3f0-b749-dbce-79035a28e931"), new Guid("05053489-cf18-a656-cdad-315afd2fbab0"), "Hip thrust", null, 4, "3x8" },
                    { new Guid("77e1ab8a-fefa-07dc-90c5-e73b1548e50c"), new Guid("80d5545f-200a-29ca-0ed8-06422ed293c4"), "Press de banca plana con barra", null, 2, "4x12" },
                    { new Guid("78bf1f3f-e3b5-ebb3-daf9-39da3328d665"), new Guid("f79a4195-c83d-edf6-9f4c-b68e4b2fddd1"), "Press inclinado con mancuernas", null, 2, "3x8-10" },
                    { new Guid("791ad23d-c0cb-77b9-96cc-b088fc3d52d3"), new Guid("80d5545f-200a-29ca-0ed8-06422ed293c4"), "Flexiones de brazos", "Calentamiento", 1, "2x10" },
                    { new Guid("79b1c410-955d-713a-1c6d-2639611ea860"), new Guid("f79a4195-c83d-edf6-9f4c-b68e4b2fddd1"), "Press en máquina", null, 3, "3x10-12" },
                    { new Guid("7a07a1df-fd60-4971-0423-768b1de04002"), new Guid("bd2d4b07-6c0f-b525-d279-1032ed23adfa"), "Curl femoral", "Fuerza", 7, "3x10-12" },
                    { new Guid("7ad8ddfe-5d2c-e568-139a-b3dd9b0373ab"), new Guid("f3627e9f-34ac-a786-f3e5-b930b1514e2e"), "Extensión de tríceps en polea", null, 6, "3x10-12" },
                    { new Guid("7bd8db8d-4a16-d7cf-3ee1-ae6b1e7e6d43"), new Guid("93754476-dacc-6685-9d41-8d31914f9b9f"), "Remo sentado", null, 2, "3x10" },
                    { new Guid("7d8eb2c5-cc9f-5ee1-df3f-c4d958dcd895"), new Guid("0b48ccf7-6b00-c8e2-e747-351f85711074"), "Plancha", "Abdomen", 9, "1 min" },
                    { new Guid("7ec71d39-d95f-0ab0-672f-a7e2086d53e0"), new Guid("0b48ccf7-6b00-c8e2-e747-351f85711074"), "Bicho", "Abdomen", 10, "3x12" },
                    { new Guid("8078c628-d373-76e1-2080-77b4434e1c24"), new Guid("1454f2dd-e385-19ae-6967-4f077bdf2366"), "Elevaciones laterales", null, 2, "4x12-15" },
                    { new Guid("81d6964b-2ce4-64e2-6c52-757a5e751280"), new Guid("ee199abb-81d7-87eb-7ccb-bf8f966afeb0"), "Remo sentado en polea o convergente", null, 3, "3x8-12" },
                    { new Guid("81dcfedd-a3b1-1508-f681-4f9f01e81919"), new Guid("9ff48d87-9170-d21f-4f3f-41bca40398af"), "Gemelos sentado", null, 7, "3x15-20" },
                    { new Guid("82b9b5c1-7dc9-2e4c-36c4-7b260a7b5541"), new Guid("df1f47cb-47c1-ab97-7952-8389bf9052ea"), "Peso muerto rumano", null, 3, "3x8-10" },
                    { new Guid("86775f26-a2ee-dd63-801d-75c6667cd853"), new Guid("31343d54-9c0d-ba97-d1cd-ea25d178b8a3"), "Curl de bíceps concentrado con mancuerna", null, 6, "4x10" },
                    { new Guid("87a32a88-5212-53ae-d4bd-5913726d7552"), new Guid("93754476-dacc-6685-9d41-8d31914f9b9f"), "Jalón al pecho", null, 3, "3x10" },
                    { new Guid("87fba69f-4e56-a342-200a-a5d5c12a04c8"), new Guid("05053489-cf18-a656-cdad-315afd2fbab0"), "Peso muerto rumano", null, 2, "4x6-8" },
                    { new Guid("882e3aac-2a98-bd4d-4892-a2031e4601ca"), new Guid("9bb57730-3520-0095-449a-ac5680a1f56e"), "Abdominales", "Abdomen", 8, "3x15" },
                    { new Guid("884b97d5-641c-2fbd-5372-ac3bfd8fdfe3"), new Guid("31343d54-9c0d-ba97-d1cd-ea25d178b8a3"), "Press militar con mancuernas", null, 4, "3x10" },
                    { new Guid("886511d1-a264-3579-acfe-2725e964bcbc"), new Guid("bd2d4b07-6c0f-b525-d279-1032ed23adfa"), "Step-up", "Fuerza", 6, "3x8 por pierna" },
                    { new Guid("8a45cda5-775c-98da-c099-4402ad25261f"), new Guid("1454f2dd-e385-19ae-6967-4f077bdf2366"), "Press militar", null, 1, "4x6-8" },
                    { new Guid("8b354e44-48fc-4279-805e-0f8d5be24545"), new Guid("0b48ccf7-6b00-c8e2-e747-351f85711074"), "Sentadillas frontales con barra", null, 2, "3x10" },
                    { new Guid("8d4437ee-8292-dc38-2c4e-7965dd766afe"), new Guid("878f3b62-84d7-b340-5070-473adcaafdb9"), "Rodilla al pecho en paralelas", "Abdomen", 7, "3x10" },
                    { new Guid("8e9fb810-b3e1-b72f-0bfe-ef3797b538cb"), new Guid("f3627e9f-34ac-a786-f3e5-b930b1514e2e"), "Fondos tríceps", null, 5, "3x8-12" },
                    { new Guid("90b7f384-e283-5ec4-d0f5-f06b9d5cabd9"), new Guid("40b3c891-9565-92bf-ad81-f304d7892a89"), "Remo con barra o T", null, 2, "4x6-8" },
                    { new Guid("919ce769-7670-b585-fd22-1bd4ce63cb2b"), new Guid("2dc45b9d-3d85-09b8-e227-d3e11e33de5f"), "Puente de glúteos", null, 9, "3x12-15" },
                    { new Guid("91b0f763-0c97-140e-185f-8f33a32903fe"), new Guid("9ff48d87-9170-d21f-4f3f-41bca40398af"), "Hack", null, 3, "3x10-12" },
                    { new Guid("926ec09f-7612-bdf3-f19e-307df55df193"), new Guid("13e40f4a-7ffe-406e-fce8-7e166bfdc10c"), "Press militar", null, 3, "3x8" },
                    { new Guid("92898e64-a3e3-9018-7fbe-40da8438fda1"), new Guid("1f223952-203a-c90c-b079-c32eea76f6a2"), "Remo unilateral en polea o mancuerna", null, 2, "4x8-12" },
                    { new Guid("92e37eaf-236b-4cb1-744f-b8ebf6e13c95"), new Guid("bd2d4b07-6c0f-b525-d279-1032ed23adfa"), "Saltos al cajón", "Potencia", 1, "4x4" },
                    { new Guid("9343421b-de5f-367e-3298-7c71950530ac"), new Guid("05053489-cf18-a656-cdad-315afd2fbab0"), "Plancha frontal", null, 7, "3x40-60 seg" },
                    { new Guid("93a3a8f9-d330-c4a2-50c7-6475b10c9db1"), new Guid("2dc45b9d-3d85-09b8-e227-d3e11e33de5f"), "Remo sentado en polea o convergente", null, 2, "4x8-12" },
                    { new Guid("944eba92-c3b3-fde7-a159-c9c1173715b2"), new Guid("13e40f4a-7ffe-406e-fce8-7e166bfdc10c"), "Dominadas / Jalón al pecho", null, 4, "3x8-10" },
                    { new Guid("94a213f7-4d36-6957-fc49-1753b527d499"), new Guid("40b3c891-9565-92bf-ad81-f304d7892a89"), "Jalón unilateral", null, 4, "3x10-12" },
                    { new Guid("95996867-11de-fb3c-f6e1-b07d0df88702"), new Guid("201414bb-c3ca-3d28-d123-7a1837040081"), "Peso muerto rumano", null, 1, "4x6-8" },
                    { new Guid("960c7ce5-f41e-c572-f863-29b2d7b2a791"), new Guid("9ff48d87-9170-d21f-4f3f-41bca40398af"), "Extensión de cuádriceps", null, 4, "3x12-15" },
                    { new Guid("97476bfc-acc1-b38c-6897-43e9407d4349"), new Guid("f79a4195-c83d-edf6-9f4c-b68e4b2fddd1"), "Aperturas en mariposa", null, 4, "3x12-15" },
                    { new Guid("98c00460-7f2f-e108-14e8-b514ffd48a35"), new Guid("446f9fec-6c67-68c9-749a-f5d827390276"), "Copenhagen plank (plancha en banco plano)", null, 8, "3x20-30 seg por lado" },
                    { new Guid("9ac89232-a5d3-cfdf-e7c1-fd0a37d72b25"), new Guid("1f223952-203a-c90c-b079-c32eea76f6a2"), "Press inclinado con barra", null, 1, "4x8-12" },
                    { new Guid("9d9d86c2-26d3-1b4d-0e86-dcc5454bc8c1"), new Guid("1f223952-203a-c90c-b079-c32eea76f6a2"), "Step-up bajo", null, 8, "3x8-10 c/pierna" },
                    { new Guid("9ddc1999-64ef-aa2f-f382-843cbb551927"), new Guid("021969a5-e128-cc45-0527-599a61222a2e"), "Face pull", null, 5, "3x12-15" },
                    { new Guid("a13d2679-f296-b419-d8d8-b1cc7a5c247f"), new Guid("8bf516ed-cc46-99ef-7304-975d03bc2bd3"), "Flexiones de brazos", "Calentamiento", 1, "2x10" },
                    { new Guid("a1f33a82-cac8-86d8-5288-ac201da33b82"), new Guid("05053489-cf18-a656-cdad-315afd2fbab0"), "Sentadilla con barra", null, 1, "4x5-6" },
                    { new Guid("a24c83f8-cf14-0489-cc40-72c23f3c9619"), new Guid("201414bb-c3ca-3d28-d123-7a1837040081"), "Prensa con pies altos", null, 4, "3x10-12" },
                    { new Guid("a39f6504-7a23-afe7-f991-20143da5a6db"), new Guid("1454f2dd-e385-19ae-6967-4f077bdf2366"), "Press francés", null, 7, "3x8-10" },
                    { new Guid("a4deedbe-de0e-4f99-44eb-fb855bcb2f47"), new Guid("8bf516ed-cc46-99ef-7304-975d03bc2bd3"), "Fondos de tríceps en cajón", null, 7, "3x" },
                    { new Guid("a7fb9668-7b0d-fb48-c9c3-f662da365e6c"), new Guid("9bb57730-3520-0095-449a-ac5680a1f56e"), "Bicho", "Abdomen", 10, "3x12" },
                    { new Guid("a989de1b-78dd-8c1a-f6ee-868469ce177f"), new Guid("93754476-dacc-6685-9d41-8d31914f9b9f"), "Tríceps en polea", null, 8, "2x12" },
                    { new Guid("aa6cec0a-9fb4-dd09-c9eb-05a362a90eb9"), new Guid("021969a5-e128-cc45-0527-599a61222a2e"), "Abducción de cadera con banda", null, 10, "3x15-20" },
                    { new Guid("acba2b64-1dce-7240-ca1b-a8a750031b01"), new Guid("df1f47cb-47c1-ab97-7952-8389bf9052ea"), "Hip thrust", null, 6, "3x8-12" },
                    { new Guid("ad6b8222-ec6c-79c3-5aa3-f5beb856d554"), new Guid("021969a5-e128-cc45-0527-599a61222a2e"), "Extensión de tríceps en polea", null, 7, "3x10-12" },
                    { new Guid("ae376f74-2378-1aa5-9a67-b0a717f932ee"), new Guid("2dc45b9d-3d85-09b8-e227-d3e11e33de5f"), "Press banca plano", null, 1, "4x6-10" },
                    { new Guid("aed5558e-a7d6-1de3-a81d-b5b80009886a"), new Guid("f6fa028d-21af-cec2-83ee-af0d5651b407"), "Pájaros / posterior", null, 5, "3x12-15" },
                    { new Guid("afb81822-46dc-8a5b-396b-57bc8f86cf45"), new Guid("446f9fec-6c67-68c9-749a-f5d827390276"), "Gemelos", null, 7, "4x15-20" },
                    { new Guid("afd5fb50-2f17-6c78-7fb7-5572d12d9a34"), new Guid("878f3b62-84d7-b340-5070-473adcaafdb9"), "Remo unilateral en polea", null, 3, "4x12" },
                    { new Guid("b0733595-cd90-5d57-e01e-5ea939f8d212"), new Guid("bd2d4b07-6c0f-b525-d279-1032ed23adfa"), "Peso muerto a una pierna", "Fuerza", 5, "3x8 por pierna" },
                    { new Guid("b404d20d-f2ac-cafa-8f36-bd4d65866347"), new Guid("f6fa028d-21af-cec2-83ee-af0d5651b407"), "Elevaciones laterales", null, 3, "4x12-15" },
                    { new Guid("b4d74cb7-09f5-b2d4-959f-469c856cac2f"), new Guid("ee199abb-81d7-87eb-7ccb-bf8f966afeb0"), "Curl con barra", null, 6, "3x8-10" },
                    { new Guid("b4e12289-6f32-b6d1-3c7f-0dedcc7eb27f"), new Guid("05053489-cf18-a656-cdad-315afd2fbab0"), "Curl femoral", null, 5, "3x10" },
                    { new Guid("b5f3f299-6e80-3b42-d6b8-7625430512f5"), new Guid("80d5545f-200a-29ca-0ed8-06422ed293c4"), "Press inclinado con mancuernas", null, 3, "3x12" },
                    { new Guid("b69358c8-0389-8b77-8bec-3c46dcc1c6df"), new Guid("9bb57730-3520-0095-449a-ac5680a1f56e"), "Prensa unilateral", null, 3, "4x12" },
                    { new Guid("b70a9f99-79b0-5ba6-8add-f3471d147b3b"), new Guid("13e40f4a-7ffe-406e-fce8-7e166bfdc10c"), "Face pull", null, 6, "3x12-15" },
                    { new Guid("ba37e8b0-d510-e80a-ea1b-98c05b6dfdcd"), new Guid("1f223952-203a-c90c-b079-c32eea76f6a2"), "Jalón al pecho", null, 4, "3x10-12" },
                    { new Guid("bad3ea76-5c36-2ad6-0772-15b23e531de5"), new Guid("1be5e5fc-d8d2-aea3-b586-46e4987b6bbd"), "Remo unilateral", null, 3, "3x10-12" },
                    { new Guid("bc879298-7555-1754-41a2-61da74871737"), new Guid("ee199abb-81d7-87eb-7ccb-bf8f966afeb0"), "Jalón unilateral", null, 4, "3x10-12" },
                    { new Guid("bddf5bc0-336a-c86c-a863-2cee48d87876"), new Guid("ee199abb-81d7-87eb-7ccb-bf8f966afeb0"), "Jalón al pecho", null, 1, "4x6-10" },
                    { new Guid("be4c28d6-5f24-f7bc-3fe5-7ebceef0be09"), new Guid("9ff48d87-9170-d21f-4f3f-41bca40398af"), "Gemelos de pie", null, 6, "4x12-15" },
                    { new Guid("be7cadc8-1304-6a76-0356-663540ecfab9"), new Guid("1be5e5fc-d8d2-aea3-b586-46e4987b6bbd"), "Remo con apoyo de pecho", null, 2, "4x8-10" },
                    { new Guid("beca9134-abcb-a9bb-74f1-52e11fdee5bd"), new Guid("446f9fec-6c67-68c9-749a-f5d827390276"), "Estocadas caminando", null, 4, "3x10 por pierna" },
                    { new Guid("bf82804e-4371-953f-6feb-e4e9fc579d24"), new Guid("31343d54-9c0d-ba97-d1cd-ea25d178b8a3"), "Curl de bíceps sentado con barra Z", null, 5, "4x10" },
                    { new Guid("c094f575-b2ea-e810-7de5-c2ba336e460f"), new Guid("f3627e9f-34ac-a786-f3e5-b930b1514e2e"), "Press inclinado con mancuernas", null, 2, "3x8-10" },
                    { new Guid("c1de1f53-955f-de1d-851b-9a140677f933"), new Guid("80d5545f-200a-29ca-0ed8-06422ed293c4"), "Extensión de tríceps con polea alta", null, 5, "4x10" },
                    { new Guid("c1f2c47b-ada7-28d2-d99d-ea7a9bc7ede4"), new Guid("df1f47cb-47c1-ab97-7952-8389bf9052ea"), "Sentadilla", null, 1, "4x6-8" },
                    { new Guid("c31d233b-2f9b-0af0-901a-898daddae17a"), new Guid("9bb57730-3520-0095-449a-ac5680a1f56e"), "Pantorrilla parado en máquina", null, 7, "3x12" },
                    { new Guid("c435af49-0405-1e1e-5d3b-bb037edf7b8d"), new Guid("80d5545f-200a-29ca-0ed8-06422ed293c4"), "Fondos de tríceps en cajón", null, 7, "3x" },
                    { new Guid("c4bf2852-41c3-0aa0-3f3f-f9f136def418"), new Guid("93754476-dacc-6685-9d41-8d31914f9b9f"), "Face pull", null, 6, "3x15" },
                    { new Guid("c4e9c512-0f60-2543-b40a-d4b225b844c5"), new Guid("bd2d4b07-6c0f-b525-d279-1032ed23adfa"), "Saltos horizontales", "Potencia", 2, "3x5" },
                    { new Guid("c4eac55c-af01-8e72-1f9f-9bb49dcbace5"), new Guid("1be5e5fc-d8d2-aea3-b586-46e4987b6bbd"), "Press francés", null, 7, "3x8-10" },
                    { new Guid("c7356b44-0756-183b-ac2f-9c3ad9e8065e"), new Guid("31343d54-9c0d-ba97-d1cd-ea25d178b8a3"), "Jalón al pecho con agarre abierto", null, 2, "4x10" },
                    { new Guid("c9a08d1f-a519-ed6c-63ab-afbdb598db81"), new Guid("df1f47cb-47c1-ab97-7952-8389bf9052ea"), "Prensa 45°", null, 2, "3x8-10" },
                    { new Guid("cb847897-7657-b664-78f0-e2567f306bce"), new Guid("f79a4195-c83d-edf6-9f4c-b68e4b2fddd1"), "Extensión de tríceps en polea", null, 6, "3x10-12" },
                    { new Guid("ccb511dd-96a2-2dc1-7793-0890be227660"), new Guid("2dc45b9d-3d85-09b8-e227-d3e11e33de5f"), "Curl bíceps con barra", null, 6, "3x10-12" },
                    { new Guid("cd165ab5-6a00-5d14-38da-d7d5d06dd786"), new Guid("1f223952-203a-c90c-b079-c32eea76f6a2"), "Pájaros (vuelo posterior)", null, 5, "3x12-15" },
                    { new Guid("d31ad43f-f740-4951-73a9-117bda9bd12a"), new Guid("05053489-cf18-a656-cdad-315afd2fbab0"), "Prensa 45°", null, 3, "3x8" },
                    { new Guid("d3e1f549-7b7d-8030-07af-0e588507620b"), new Guid("40b3c891-9565-92bf-ad81-f304d7892a89"), "Remo sentado en polea o convergente", null, 3, "3x8-12" },
                    { new Guid("d41641f8-7788-e821-0595-fe8d6a06f712"), new Guid("31343d54-9c0d-ba97-d1cd-ea25d178b8a3"), "Plancha", "Abdomen", 8, "1 min" },
                    { new Guid("d4a0b9cd-f5e0-f621-aca0-91851649643b"), new Guid("201414bb-c3ca-3d28-d123-7a1837040081"), "Hip thrust", null, 2, "4x8-10" },
                    { new Guid("d4da19cf-c431-b76a-d05c-2e03c8ce0ec8"), new Guid("93754476-dacc-6685-9d41-8d31914f9b9f"), "Press inclinado", null, 1, "3x8-10" },
                    { new Guid("d5772dd4-9dcb-e2b6-7320-158d2e79fd81"), new Guid("2dc45b9d-3d85-09b8-e227-d3e11e33de5f"), "Extensión de rodilla en máquina", null, 7, "3x12-15" },
                    { new Guid("d5bec07b-6587-4ddb-2fa0-92de635bc0e4"), new Guid("8bf516ed-cc46-99ef-7304-975d03bc2bd3"), "Press de banca plana con barra", null, 2, "4x12" },
                    { new Guid("d608b7f0-4cc5-4ee6-b8b5-703c5e722a82"), new Guid("1454f2dd-e385-19ae-6967-4f077bdf2366"), "Tríceps en polea", null, 8, "3x10-12" },
                    { new Guid("d96e1d2d-97bc-4311-a0f7-9a882839176b"), new Guid("df1f47cb-47c1-ab97-7952-8389bf9052ea"), "Gemelos de pie", null, 7, "4x12-15" },
                    { new Guid("de3bf214-70b5-c40e-5b15-88a7221b647a"), new Guid("f3627e9f-34ac-a786-f3e5-b930b1514e2e"), "Press en máquina", null, 3, "3x10-12" },
                    { new Guid("e04a1649-9502-9f62-6613-5a8313db5a14"), new Guid("df1f47cb-47c1-ab97-7952-8389bf9052ea"), "Camilla invertida", null, 5, "3x10-15" },
                    { new Guid("e08afa30-cbab-03c8-c0a2-bc536368d96d"), new Guid("13e40f4a-7ffe-406e-fce8-7e166bfdc10c"), "Press banca", null, 1, "4x6-8" },
                    { new Guid("e0cc7df2-da4c-3e5e-e0b5-4db7be9ad703"), new Guid("f79a4195-c83d-edf6-9f4c-b68e4b2fddd1"), "Fondos tríceps", null, 5, "3x8-12" },
                    { new Guid("e1008f21-71cf-2b5a-2ae7-452cd874d344"), new Guid("878f3b62-84d7-b340-5070-473adcaafdb9"), "Press militar con mancuernas", null, 4, "3x10" },
                    { new Guid("e14c20ec-c5d8-9286-26f9-f2a16582f6da"), new Guid("201414bb-c3ca-3d28-d123-7a1837040081"), "Camilla invertida", null, 3, "3x10-12" },
                    { new Guid("e8303a31-2eb8-b2f0-1558-e31f3b40c367"), new Guid("bd2d4b07-6c0f-b525-d279-1032ed23adfa"), "Gemelos sentado", "Fuerza", 8, "3x15" },
                    { new Guid("e84fa45d-1cee-85d7-a040-4fbaa114cd3e"), new Guid("9bb57730-3520-0095-449a-ac5680a1f56e"), "Abductores en polea", null, 6, "3x12" },
                    { new Guid("e90ced7f-6594-456a-bb9f-a2e6c69a9412"), new Guid("1f223952-203a-c90c-b079-c32eea76f6a2"), "Tríceps con cuerda", null, 7, "3x10-15" },
                    { new Guid("ea7cdead-e18e-ecfb-48a9-50e3e30e9fc8"), new Guid("878f3b62-84d7-b340-5070-473adcaafdb9"), "Jalón al pecho con agarre abierto", null, 2, "4x10" },
                    { new Guid("eaaab929-2bdc-279e-12e3-36ab408065bd"), new Guid("f6fa028d-21af-cec2-83ee-af0d5651b407"), "Face pull", null, 6, "3x12-15" },
                    { new Guid("ed6dbcb6-4956-f5e9-aa90-fb1dd9ca395d"), new Guid("021969a5-e128-cc45-0527-599a61222a2e"), "Press militar sentado", null, 1, "4x8-10" },
                    { new Guid("ee863b2e-da4d-aaa0-c592-9557312915ff"), new Guid("878f3b62-84d7-b340-5070-473adcaafdb9"), "Dominadas agarre supino", "Calentamiento", 1, "3x al fallo técnico" },
                    { new Guid("ef90116b-d44b-b15b-44f2-20c04c69bcab"), new Guid("80d5545f-200a-29ca-0ed8-06422ed293c4"), "Apertura plana con mancuernas", null, 4, "3x12" },
                    { new Guid("f00f24f1-e4e4-3d3e-ac37-850c767a20ca"), new Guid("31343d54-9c0d-ba97-d1cd-ea25d178b8a3"), "Remo unilateral en polea", null, 3, "4x12" },
                    { new Guid("f06f992a-10fc-0d36-ae98-afa611fd915e"), new Guid("df1f47cb-47c1-ab97-7952-8389bf9052ea"), "Extensión de cuádriceps", null, 4, "3x10-15" },
                    { new Guid("f3f118b1-e2b2-b85f-15c0-0c621d7d8043"), new Guid("021969a5-e128-cc45-0527-599a61222a2e"), "Jalón neutro", null, 3, "3x10-12" },
                    { new Guid("f534e904-9424-3c2e-29e0-3aaca3e1b022"), new Guid("1f223952-203a-c90c-b079-c32eea76f6a2"), "Isométrico de cuádriceps", null, 10, "3x30-45 seg" },
                    { new Guid("f5a09f85-5f5f-07db-d633-f6604fc26c21"), new Guid("9ff48d87-9170-d21f-4f3f-41bca40398af"), "Prensa 45°", null, 2, "4x8-10" },
                    { new Guid("f69d1324-a8c2-d813-96ba-0134eb74f5d1"), new Guid("1454f2dd-e385-19ae-6967-4f077bdf2366"), "Pájaros / posterior", null, 3, "3x12-15" },
                    { new Guid("f7631ad4-cc7c-0080-fc67-6df2a36a6f2c"), new Guid("878f3b62-84d7-b340-5070-473adcaafdb9"), "Curl de bíceps concentrado con mancuerna", null, 6, "4x10" },
                    { new Guid("f83dcb22-07b8-8498-cc06-efc606e3d606"), new Guid("1be5e5fc-d8d2-aea3-b586-46e4987b6bbd"), "Curl con barra W", null, 5, "3x8-10" },
                    { new Guid("f97f42ee-0980-a580-e0ff-e7fb4737a68e"), new Guid("40b3c891-9565-92bf-ad81-f304d7892a89"), "Curl martillo", null, 8, "2-3x12-15" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("012e4646-fdb2-c223-0eb2-94f354352aa3"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("01e1d1f0-50e5-5299-e8ed-e556606ccfce"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("032bbb07-951a-9418-ee40-a7026537aaa1"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("038e055d-a239-2fc8-f3fa-c2daaa4a5ace"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("069ec3bc-7e2f-f533-d666-cb16b111e989"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("087b14a6-de4c-93d3-6d17-a441d6874746"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("0b9e0ec2-f7df-7535-fd02-9cc6e238ece6"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("0d2d403f-fed2-6250-a833-980d2c49817a"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("0fbf9d40-e024-6915-8ef0-deaa3867aaf2"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("10418d71-4f3a-0d6b-7f33-53615a9a7d6a"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("131d9dc9-3c06-b2d3-73dd-4231c9d7a695"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("138c8585-686f-50ba-595f-5dbe9c4d7445"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("15ab88fc-c6ef-c17d-4779-f87e2be87623"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("19251d62-93b4-6d09-a8df-ead9c8c9625c"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("1966a1e3-8e96-07d1-cb3f-935577811f95"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("19b2bf46-a25e-3f53-abd2-9e0b322246ec"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("1e330573-98af-ebeb-4b03-b15339c75de8"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("207b641b-6656-7c10-001a-b9e0cefaa2a4"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("214f66ed-c369-6e16-70b7-ef44cc423ff4"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("228e0415-c265-0d7d-aed1-9e400c2492ff"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("23688f06-0f22-26ad-4eb9-5522e70f5eb5"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("24ada14b-400b-6890-bf9c-558cf0b81f1c"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("24afed98-565a-f941-846c-6e6058bc9baa"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("2742556a-be95-4e6e-ff8b-1dd556ebc325"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("27bf010e-2a66-2e65-8cd0-7e7e12cbcf56"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("282003fa-d494-eb6f-cde8-c3cbb1d443f3"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("282fbe60-4962-92df-6179-ba759c10326d"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("2d404713-7d76-a39e-e82d-270f844911a3"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("2e3274e1-5675-db16-d24a-0105ff96e3e5"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("2e9e3057-bf62-40a3-49bb-e3509d4cbf87"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("2fc0034e-61b6-20c8-b774-5336828ee770"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("30aa2495-5fb1-1da9-1d7a-b345ef1ebef9"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("313dbf3d-d359-88a0-bc4a-cbcfef834ba9"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("34d953c3-f14c-7238-1039-7f88f0a221be"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("34f080db-9d35-1e03-d06a-2664cfd06e9c"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("354d0216-5038-236b-9365-cb70f86708a3"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("35986405-dc6c-b6d1-79af-f6060e92ffed"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("35b778da-121e-f74f-dd95-3dc7fcd917ed"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("35e327c8-84e9-aeb2-ff34-d1c67a5899fb"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("368b25d2-9f4c-93e9-9d9c-6671d5ef75a8"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("36cfe60e-a175-e769-9bd6-5f6823ef887f"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("37f6e24e-9875-0775-6cd7-7b2ccecf1286"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("3b28771b-60bd-77bf-7178-c19b457f3a22"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("3b96aa30-4393-df6b-b0d2-56d0988fde79"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("40de3b44-a879-bed7-5567-311eca8c6ec5"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("43d83d48-76da-4dd9-6399-4d7ce9621ea4"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("45606449-f258-31b4-b055-2c9b0aac8830"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("48ada616-8539-da39-8a67-9b5cfbbb6524"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("48cd9294-1b40-5ff8-7740-fdaab1ace1ce"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("49b3b90d-c853-d6a6-0a7f-6a7b47ba7f3e"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("4c840285-9f60-4e53-a3d2-daf76ceaab1f"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("4d6e57f0-aa34-d401-2b55-d9590a1344fe"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("4daf44e0-3f1f-b310-e4e9-26ff5a210a5e"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("4ea5e314-32de-e3fd-e77a-96ac603bbeb2"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("4eddb2b3-7872-95f9-026d-4b9f8d775ff3"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("5399b040-b8fe-7079-a28b-fe737b9f7f0c"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("53a9c4c3-531d-8930-c737-fb94cce37506"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("554c3d09-f139-6409-5d66-3f179f8735fd"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("55b269af-7a38-adad-7058-3ce5beb2254f"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("56310411-3692-a6a5-98b3-c8c009243f2c"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("56e1aaed-559f-a2a5-19e7-853c45e0ebcf"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("5b583920-4cde-22f0-2453-97056b00eec6"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("5b639215-f34a-c8b4-90ef-1a1e76bb026c"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("5c0f90f4-9c6b-4142-2caa-e24d1eb58fc8"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("5c5993a0-b55f-6bc6-5afd-d6bd5e2a6065"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("5cd8caa8-8f6b-9451-6829-027b90af0595"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("5cf6987c-b696-f722-2ca1-f7571d62bc07"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("5d525401-a6b8-69cc-f3ae-681eac988652"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("5f712e5b-3366-31c4-2057-9f4de5cb7dcf"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("5ff7cbd2-5990-2511-3a39-b57345ef9106"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("618dcc8e-e109-e4f4-b594-a2472aa51ac5"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("63adc7b5-8b6a-64b2-7720-99f46918e461"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("651628d1-af84-12c1-fb9e-19f6539d7031"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("6630ee11-f70d-2609-8598-497ba48ae22e"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("6760f308-2fb0-d51e-4ed8-ec99c535bd90"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("67de007a-7fdb-9a5f-654e-90d56d81cb9f"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("687f3c5b-58ab-2295-a7dd-cec9b970961a"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("6a1ad330-bda7-4b88-bfdd-a7ac16563683"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("6c5b3c7d-f0d2-703a-a974-134c6e838f5d"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("6cc82485-bcdd-6298-dc04-b34faecec9e8"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("71e1043b-f373-0070-b13e-962f2930d595"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("72a59af3-862f-b380-37d6-40d662e7844c"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("74f5a81a-b3f0-b749-dbce-79035a28e931"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("77e1ab8a-fefa-07dc-90c5-e73b1548e50c"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("78bf1f3f-e3b5-ebb3-daf9-39da3328d665"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("791ad23d-c0cb-77b9-96cc-b088fc3d52d3"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("79b1c410-955d-713a-1c6d-2639611ea860"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("7a07a1df-fd60-4971-0423-768b1de04002"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("7ad8ddfe-5d2c-e568-139a-b3dd9b0373ab"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("7bd8db8d-4a16-d7cf-3ee1-ae6b1e7e6d43"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("7d8eb2c5-cc9f-5ee1-df3f-c4d958dcd895"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("7ec71d39-d95f-0ab0-672f-a7e2086d53e0"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("8078c628-d373-76e1-2080-77b4434e1c24"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("81d6964b-2ce4-64e2-6c52-757a5e751280"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("81dcfedd-a3b1-1508-f681-4f9f01e81919"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("82b9b5c1-7dc9-2e4c-36c4-7b260a7b5541"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("86775f26-a2ee-dd63-801d-75c6667cd853"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("87a32a88-5212-53ae-d4bd-5913726d7552"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("87fba69f-4e56-a342-200a-a5d5c12a04c8"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("882e3aac-2a98-bd4d-4892-a2031e4601ca"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("884b97d5-641c-2fbd-5372-ac3bfd8fdfe3"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("886511d1-a264-3579-acfe-2725e964bcbc"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("8a45cda5-775c-98da-c099-4402ad25261f"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("8b354e44-48fc-4279-805e-0f8d5be24545"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("8d4437ee-8292-dc38-2c4e-7965dd766afe"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("8e9fb810-b3e1-b72f-0bfe-ef3797b538cb"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("90b7f384-e283-5ec4-d0f5-f06b9d5cabd9"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("919ce769-7670-b585-fd22-1bd4ce63cb2b"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("91b0f763-0c97-140e-185f-8f33a32903fe"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("926ec09f-7612-bdf3-f19e-307df55df193"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("92898e64-a3e3-9018-7fbe-40da8438fda1"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("92e37eaf-236b-4cb1-744f-b8ebf6e13c95"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("9343421b-de5f-367e-3298-7c71950530ac"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("93a3a8f9-d330-c4a2-50c7-6475b10c9db1"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("944eba92-c3b3-fde7-a159-c9c1173715b2"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("94a213f7-4d36-6957-fc49-1753b527d499"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("95996867-11de-fb3c-f6e1-b07d0df88702"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("960c7ce5-f41e-c572-f863-29b2d7b2a791"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("97476bfc-acc1-b38c-6897-43e9407d4349"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("98c00460-7f2f-e108-14e8-b514ffd48a35"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("9ac89232-a5d3-cfdf-e7c1-fd0a37d72b25"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("9d9d86c2-26d3-1b4d-0e86-dcc5454bc8c1"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("9ddc1999-64ef-aa2f-f382-843cbb551927"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("a13d2679-f296-b419-d8d8-b1cc7a5c247f"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("a1f33a82-cac8-86d8-5288-ac201da33b82"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("a24c83f8-cf14-0489-cc40-72c23f3c9619"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("a39f6504-7a23-afe7-f991-20143da5a6db"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("a4deedbe-de0e-4f99-44eb-fb855bcb2f47"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("a7fb9668-7b0d-fb48-c9c3-f662da365e6c"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("a989de1b-78dd-8c1a-f6ee-868469ce177f"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("aa6cec0a-9fb4-dd09-c9eb-05a362a90eb9"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("acba2b64-1dce-7240-ca1b-a8a750031b01"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("ad6b8222-ec6c-79c3-5aa3-f5beb856d554"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("ae376f74-2378-1aa5-9a67-b0a717f932ee"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("aed5558e-a7d6-1de3-a81d-b5b80009886a"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("afb81822-46dc-8a5b-396b-57bc8f86cf45"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("afd5fb50-2f17-6c78-7fb7-5572d12d9a34"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("b0733595-cd90-5d57-e01e-5ea939f8d212"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("b404d20d-f2ac-cafa-8f36-bd4d65866347"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("b4d74cb7-09f5-b2d4-959f-469c856cac2f"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("b4e12289-6f32-b6d1-3c7f-0dedcc7eb27f"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("b5f3f299-6e80-3b42-d6b8-7625430512f5"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("b69358c8-0389-8b77-8bec-3c46dcc1c6df"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("b70a9f99-79b0-5ba6-8add-f3471d147b3b"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("ba37e8b0-d510-e80a-ea1b-98c05b6dfdcd"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("bad3ea76-5c36-2ad6-0772-15b23e531de5"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("bc879298-7555-1754-41a2-61da74871737"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("bddf5bc0-336a-c86c-a863-2cee48d87876"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("be4c28d6-5f24-f7bc-3fe5-7ebceef0be09"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("be7cadc8-1304-6a76-0356-663540ecfab9"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("beca9134-abcb-a9bb-74f1-52e11fdee5bd"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("bf82804e-4371-953f-6feb-e4e9fc579d24"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("c094f575-b2ea-e810-7de5-c2ba336e460f"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("c1de1f53-955f-de1d-851b-9a140677f933"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("c1f2c47b-ada7-28d2-d99d-ea7a9bc7ede4"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("c31d233b-2f9b-0af0-901a-898daddae17a"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("c435af49-0405-1e1e-5d3b-bb037edf7b8d"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("c4bf2852-41c3-0aa0-3f3f-f9f136def418"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("c4e9c512-0f60-2543-b40a-d4b225b844c5"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("c4eac55c-af01-8e72-1f9f-9bb49dcbace5"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("c7356b44-0756-183b-ac2f-9c3ad9e8065e"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("c9a08d1f-a519-ed6c-63ab-afbdb598db81"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("cb847897-7657-b664-78f0-e2567f306bce"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("ccb511dd-96a2-2dc1-7793-0890be227660"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("cd165ab5-6a00-5d14-38da-d7d5d06dd786"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("d31ad43f-f740-4951-73a9-117bda9bd12a"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("d3e1f549-7b7d-8030-07af-0e588507620b"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("d41641f8-7788-e821-0595-fe8d6a06f712"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("d4a0b9cd-f5e0-f621-aca0-91851649643b"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("d4da19cf-c431-b76a-d05c-2e03c8ce0ec8"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("d5772dd4-9dcb-e2b6-7320-158d2e79fd81"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("d5bec07b-6587-4ddb-2fa0-92de635bc0e4"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("d608b7f0-4cc5-4ee6-b8b5-703c5e722a82"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("d96e1d2d-97bc-4311-a0f7-9a882839176b"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("de3bf214-70b5-c40e-5b15-88a7221b647a"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("e04a1649-9502-9f62-6613-5a8313db5a14"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("e08afa30-cbab-03c8-c0a2-bc536368d96d"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("e0cc7df2-da4c-3e5e-e0b5-4db7be9ad703"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("e1008f21-71cf-2b5a-2ae7-452cd874d344"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("e14c20ec-c5d8-9286-26f9-f2a16582f6da"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("e8303a31-2eb8-b2f0-1558-e31f3b40c367"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("e84fa45d-1cee-85d7-a040-4fbaa114cd3e"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("e90ced7f-6594-456a-bb9f-a2e6c69a9412"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("ea7cdead-e18e-ecfb-48a9-50e3e30e9fc8"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("eaaab929-2bdc-279e-12e3-36ab408065bd"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("ed6dbcb6-4956-f5e9-aa90-fb1dd9ca395d"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("ee863b2e-da4d-aaa0-c592-9557312915ff"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("ef90116b-d44b-b15b-44f2-20c04c69bcab"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("f00f24f1-e4e4-3d3e-ac37-850c767a20ca"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("f06f992a-10fc-0d36-ae98-afa611fd915e"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("f3f118b1-e2b2-b85f-15c0-0c621d7d8043"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("f534e904-9424-3c2e-29e0-3aaca3e1b022"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("f5a09f85-5f5f-07db-d633-f6604fc26c21"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("f69d1324-a8c2-d813-96ba-0134eb74f5d1"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("f7631ad4-cc7c-0080-fc67-6df2a36a6f2c"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("f83dcb22-07b8-8498-cc06-efc606e3d606"));

            migrationBuilder.DeleteData(
                table: "Ejercicios",
                keyColumn: "Id",
                keyValue: new Guid("f97f42ee-0980-a580-e0ff-e7fb4737a68e"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("021969a5-e128-cc45-0527-599a61222a2e"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("05053489-cf18-a656-cdad-315afd2fbab0"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("0b48ccf7-6b00-c8e2-e747-351f85711074"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("13e40f4a-7ffe-406e-fce8-7e166bfdc10c"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("1454f2dd-e385-19ae-6967-4f077bdf2366"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("1be5e5fc-d8d2-aea3-b586-46e4987b6bbd"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("1f223952-203a-c90c-b079-c32eea76f6a2"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("201414bb-c3ca-3d28-d123-7a1837040081"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("2dc45b9d-3d85-09b8-e227-d3e11e33de5f"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("31343d54-9c0d-ba97-d1cd-ea25d178b8a3"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("40b3c891-9565-92bf-ad81-f304d7892a89"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("446f9fec-6c67-68c9-749a-f5d827390276"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("80d5545f-200a-29ca-0ed8-06422ed293c4"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("878f3b62-84d7-b340-5070-473adcaafdb9"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("8bf516ed-cc46-99ef-7304-975d03bc2bd3"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("93754476-dacc-6685-9d41-8d31914f9b9f"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("9bb57730-3520-0095-449a-ac5680a1f56e"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("9ff48d87-9170-d21f-4f3f-41bca40398af"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("bd2d4b07-6c0f-b525-d279-1032ed23adfa"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("df1f47cb-47c1-ab97-7952-8389bf9052ea"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("ee199abb-81d7-87eb-7ccb-bf8f966afeb0"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("f3627e9f-34ac-a786-f3e5-b930b1514e2e"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("f6fa028d-21af-cec2-83ee-af0d5651b407"));

            migrationBuilder.DeleteData(
                table: "DiasEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("f79a4195-c83d-edf6-9f4c-b68e4b2fddd1"));

            migrationBuilder.DeleteData(
                table: "PlanesEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("092e9ae5-5732-02bb-6fba-c3f752c0bffb"));

            migrationBuilder.DeleteData(
                table: "PlanesEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("298b481f-3029-eb6c-e913-b4cfe34f6753"));

            migrationBuilder.DeleteData(
                table: "PlanesEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("4704d5d4-00d5-62db-0ff2-4f69c99f7a92"));

            migrationBuilder.DeleteData(
                table: "PlanesEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("a0c45bec-4576-857c-bb17-d9936e0de2bd"));

            migrationBuilder.DeleteData(
                table: "PlanesEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("dcc1ffd7-706f-a997-36bb-502c1a58c16f"));

            migrationBuilder.DeleteData(
                table: "PlanesEntrenamiento",
                keyColumn: "Id",
                keyValue: new Guid("e06bf964-f1e7-84d1-767b-b1bd7d7b9316"));

        }
    }
}

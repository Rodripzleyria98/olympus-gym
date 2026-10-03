using System.Security.Cryptography;
using System.Text;
using Olympus.Api.Models;

namespace Olympus.Api.Data;

public static class RutinaSeedData
{
    public static readonly Guid HeberId = Guid.Parse("c9e2a6af-c023-4df4-8911-3d56046199a5");
    public static readonly Guid KarenId = Guid.Parse("7c60eed9-7435-4cdb-92d3-f4c61429b65c");
    public static readonly Guid BiancaId = Guid.Parse("8a619c3f-8bd3-4b5e-9f55-b36f26f573d1");
    public static readonly Guid JoelId = StableId("usuario:joel-acevedo");
    public static readonly Guid FedeId = StableId("usuario:fede-paez");
    public static readonly Guid MatiasId = StableId("usuario:matias-boneto");

    private const string DemoPasswordHash = "$2a$11$kdAkRdmLwFGjXeiBzhlLhu.qDKMnn8S2O.Oi9lHm2w5djynoWNTVC";
    private static readonly DateTime PendingStartDate = new(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly PlanSeed[] Seeds = CreateSeeds();
    private static readonly (PlanEntrenamiento[] Plans, DiaEntrenamiento[] Days, EjercicioItem[] Exercises) Entities =
        BuildEntities(Seeds);

    public static readonly Usuario[] Usuarios =
    [
        CreateUser(HeberId, "Heber", "Garay", "heber.garay@olympus.com"),
        CreateUser(JoelId, "Joel", "Acevedo", "joel.acevedo@olympus.com"),
        CreateUser(FedeId, "Fede", "Paez", "fede.paez@olympus.com"),
        CreateUser(KarenId, "Karen", "Socio", "karen@olympus.com"),
        CreateUser(BiancaId, "Bianca", "Socio", "bianca@olympus.com"),
        CreateUser(MatiasId, "Matias", "Boneto", "matias.boneto@olympus.com")
    ];

    public static PlanEntrenamiento[] Planes => Entities.Plans;
    public static DiaEntrenamiento[] Dias => Entities.Days;
    public static EjercicioItem[] Ejercicios => Entities.Exercises;

    private static Usuario CreateUser(Guid id, string nombre, string apellido, string email) => new()
    {
        Id = id,
        Nombre = nombre,
        Apellido = apellido,
        Email = email,
        PasswordHash = DemoPasswordHash,
        Rol = RolUsuario.User,
        FechaRegistro = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    private static PlanSeed[] CreateSeeds()
    {
        var planKarenBiancaInicio = new DateTime(2026, 2, 23, 0, 0, 0, DateTimeKind.Utc);

        return
        [
            new PlanSeed(
                "heber", HeberId, "Hipertrofia clásica - Heber Garay",
                new DateTime(2026, 9, 29, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 10, 29, 0, 0, 0, DateTimeKind.Utc),
                [
                    Day("LUNES", "Pecho + Tríceps",
                        E("Press banca con barra", "4x6-8"),
                        E("Press inclinado con mancuernas", "3x8-10"),
                        E("Press en máquina", "3x10-12"),
                        E("Aperturas en mariposa", "3x12-15"),
                        E("Fondos tríceps", "3x8-12"),
                        E("Extensión de tríceps en polea", "3x10-12"),
                        E("Extensión de tríceps sobre cabeza", "3x12-15")),
                    Day("MARTES", "Espalda + Bíceps",
                        E("Jalón al pecho", "4x6-10"),
                        E("Remo con barra o T", "4x6-8"),
                        E("Remo sentado en polea o convergente", "3x8-12"),
                        E("Jalón unilateral", "3x10-12"),
                        E("Pullover con mancuerna", "3x12-15"),
                        E("Curl con barra", "3x8-10"),
                        E("Curl inclinado con mancuernas", "3x10-12"),
                        E("Curl martillo", "2-3x12-15")),
                    Day("MIÉRCOLES", "Piernas completas",
                        E("Sentadilla", "4x6-8"),
                        E("Prensa 45°", "3x8-10"),
                        E("Peso muerto rumano", "3x8-10"),
                        E("Extensión de cuádriceps", "3x10-15"),
                        E("Camilla invertida", "3x10-15"),
                        E("Hip thrust", "3x8-12"),
                        E("Gemelos de pie", "4x12-15"),
                        E("Gemelos sentado", "3x15-20")),
                    Day("JUEVES", "Hombros + Pecho",
                        E("Press militar", "4x6-8"),
                        E("Press inclinado", "3x8-10"),
                        E("Elevaciones laterales", "4x12-15"),
                        E("Aperturas en mariposa o polea", "3x10-15"),
                        E("Pájaros / posterior", "3x12-15"),
                        E("Face pull", "3x12-15"),
                        E("Fondos o press cerrado", "3x8-12")),
                    Day("VIERNES", "Espalda + Brazos",
                        E("Jalón al pecho", "4x8-10"),
                        E("Remo con apoyo de pecho", "4x8-10"),
                        E("Remo unilateral", "3x10-12"),
                        E("Pullover en polea o mancuerna", "3x12-15"),
                        E("Curl con barra W", "3x8-10"),
                        E("Curl martillo", "3x10-12"),
                        E("Press francés", "3x8-10"),
                        E("Tríceps en polea", "3x10-15"))
                ]),
            new PlanSeed(
                "joel", JoelId, "Énfasis pierna y cuádriceps - Joel Acevedo", null, null,
                [
                    Day("LUNES", "Pecho + Tríceps",
                        E("Press banca con barra", "4x6-8"),
                        E("Press inclinado con mancuernas", "3x8-10"),
                        E("Press en máquina", "3x10-12"),
                        E("Aperturas en mariposa", "3x12-15"),
                        E("Fondos tríceps", "3x8-12"),
                        E("Extensión de tríceps en polea", "3x10-12"),
                        E("Extensión de tríceps sobre cabeza", "3x12-15")),
                    Day("MARTES", "Piernas A | Cuádriceps",
                        E("Sentadilla con mancuerna", "4x6-8"),
                        E("Prensa 45°", "4x8-10"),
                        E("Hack", "3x10-12"),
                        E("Extensión de cuádriceps", "3x12-15"),
                        E("Estocadas caminando", "3x10-12 c/pierna"),
                        E("Gemelos de pie", "4x12-15"),
                        E("Gemelos sentado", "3x15-20")),
                    Day("MIÉRCOLES", "Espalda + Bíceps",
                        E("Jalón al pecho", "4x6-10"),
                        E("Remo con barra o T", "4x6-8"),
                        E("Remo sentado en polea o convergente", "3x8-12"),
                        E("Jalón unilateral", "3x10-12"),
                        E("Pullover con mancuerna", "3x12-15"),
                        E("Curl con barra", "3x8-10"),
                        E("Curl inclinado con mancuernas", "3x10-12"),
                        E("Curl martillo", "2-3x12-15")),
                    Day("JUEVES", "Hombros + Brazos",
                        E("Press militar", "4x6-8"),
                        E("Elevaciones laterales", "4x12-15"),
                        E("Pájaros / posterior", "3x12-15"),
                        E("Face pull", "3x12-15"),
                        E("Curl con barra W", "3x8-10"),
                        E("Curl martillo", "3x10-12"),
                        E("Press francés", "3x8-10"),
                        E("Tríceps en polea", "3x10-12")),
                    Day("VIERNES", "Piernas B | Femorales + Glúteos",
                        E("Peso muerto rumano", "4x6-8"),
                        E("Hip thrust", "4x8-10"),
                        E("Camilla invertida", "3x10-12"),
                        E("Prensa con pies altos", "3x10-12"),
                        E("Estocada búlgara", "3x10-12 c/pierna"),
                        E("Extensión de cadera en polea o banda elástica", "3x12-15"),
                        E("Gemelos", "4x12-20"))
                ]),
            new PlanSeed(
                "fede", FedeId, "Fuerza, potencia unilateral y resistencia - Fede Paez", null, null,
                [
                    Day("LUNES", "Piernas: Fuerza",
                        E("Sentadilla con barra", "4x5-6"),
                        E("Peso muerto rumano", "4x6-8"),
                        E("Prensa 45°", "3x8"),
                        E("Hip thrust", "3x8"),
                        E("Curl femoral", "3x10"),
                        E("Gemelos de pie", "4x12"),
                        E("Plancha frontal", "3x40-60 seg")),
                    Day("MARTES", "Tren Superior + Core",
                        E("Press banca", "4x6-8"),
                        E("Remo con barra", "4x6-8"),
                        E("Press militar", "3x8"),
                        E("Dominadas / Jalón al pecho", "3x8-10"),
                        E("Press inclinado con mancuernas", "3x10"),
                        E("Face pull", "3x12-15"),
                        E("Pallof press", "3x10 por lado"),
                        E("Dead bug", "3x10 por lado")),
                    Day("MIÉRCOLES", "Piernas: Potencia + Unilateral",
                        E("Saltos al cajón", "4x4", "Potencia"),
                        E("Saltos horizontales", "3x5", "Potencia"),
                        E("Lanzamiento de balón medicinal + sentadilla", "3x6", "Sin golpear el balón. Potencia."),
                        E("Sentadilla búlgara", "3x8 por pierna", "Fuerza"),
                        E("Peso muerto a una pierna", "3x8 por pierna", "Fuerza"),
                        E("Step-up", "3x8 por pierna", "Fuerza"),
                        E("Curl femoral", "3x10-12", "Fuerza"),
                        E("Gemelos sentado", "3x15", "Fuerza")),
                    Day("JUEVES", "Tren Superior + Resistencia",
                        E("Press inclinado", "3x8-10"),
                        E("Remo sentado", "3x10"),
                        E("Jalón al pecho", "3x10"),
                        E("Press militar con mancuernas", "3x10"),
                        E("Elevaciones laterales", "3x12-15"),
                        E("Face pull", "3x15"),
                        E("Curl bíceps", "2x12"),
                        E("Tríceps en polea", "2x12"),
                        E("Resistencia en bicicleta", "5 min + 8x(1 min fuerte / 1 min suave) + 5 min", "Calentamiento + vuelta a la calma")),
                    Day("VIERNES", "Piernas: Resistencia + Estabilidad",
                        E("Sentadilla goblet", "3x12", "Menos carga que el día 1."),
                        E("Prensa", "3x12"),
                        E("Hip thrust", "3x10-12"),
                        E("Estocadas caminando", "3x10 por pierna"),
                        E("Curl femoral", "3x12-15"),
                        E("Extensión de cuádriceps", "2x15"),
                        E("Gemelos", "4x15-20"),
                        E("Copenhagen plank (plancha en banco plano)", "3x20-30 seg por lado"),
                        E("Plancha lateral", "3x30-40 seg por lado"))
                ]),
            new PlanSeed(
                "karen-bianca-karen", KarenId, "Tren superior, piernas y abdomen - Karen y Bianca",
                planKarenBiancaInicio, null, CreateKarenBiancaDays()),
            new PlanSeed(
                "karen-bianca-bianca", BiancaId, "Tren superior, piernas y abdomen - Karen y Bianca",
                planKarenBiancaInicio, null, CreateKarenBiancaDays()),
            new PlanSeed(
                "matias", MatiasId, "Tren superior, brazos y cuidado de rodilla - Matias Boneto",
                null, null,
                [
                    Day("DÍA 1", "Tren Superior + Rodilla",
                        E("Press banca plano", "4x6-10"),
                        E("Remo sentado en polea o convergente", "4x8-12"),
                        E("Press inclinado con mancuernas", "3x8-12"),
                        E("Jalón al pecho", "3x10-12"),
                        E("Elevaciones laterales", "3x12-15"),
                        E("Curl bíceps con barra", "3x10-12"),
                        E("Extensión de rodilla en máquina", "3x12-15"),
                        E("Camilla invertida", "3x12-15"),
                        E("Puente de glúteos", "3x12-15")),
                    Day("DÍA 2", "Hombros + Brazos + Rodilla",
                        E("Press militar sentado", "4x8-10"),
                        E("Remo T", "3x8-12"),
                        E("Jalón neutro", "3x10-12"),
                        E("Elevaciones laterales", "3x12-15"),
                        E("Face pull", "3x12-15"),
                        E("Curl martillo", "3x10-12"),
                        E("Extensión de tríceps en polea", "3x10-12"),
                        E("Prensa 45°", "3x10-12", "Recorrido corto y cómodo."),
                        E("Camilla invertida", "3x12-15"),
                        E("Abducción de cadera con banda", "3x15-20")),
                    Day("DÍA 3", "Pecho + Espalda + Brazos + Rodilla",
                        E("Press inclinado con barra", "4x8-12"),
                        E("Remo unilateral en polea o mancuerna", "4x8-12"),
                        E("Press pecho en hammer", "3x10-12"),
                        E("Jalón al pecho", "3x10-12"),
                        E("Pájaros (vuelo posterior)", "3x12-15"),
                        E("Curl bíceps en banco Scott", "3x10-12"),
                        E("Tríceps con cuerda", "3x10-15"),
                        E("Step-up bajo", "3x8-10 c/pierna"),
                        E("Puente de glúteos", "3x12-15"),
                        E("Isométrico de cuádriceps", "3x30-45 seg"))
                ])
        ];
    }

    private static DaySeed[] CreateKarenBiancaDays() =>
    [
        Day("DÍA 1", "Pecho + Tríceps",
            E("Flexiones de brazos", "2x10", "Calentamiento"),
            E("Press de banca plana con barra", "4x12"),
            E("Press inclinado con mancuernas", "3x12"),
            E("Apertura plana con mancuernas", "3x12"),
            E("Extensión de tríceps con polea alta", "4x10"),
            E("Kickback de tríceps en polea", "3x10"),
            E("Fondos de tríceps en cajón", "3x")),
        Day("DÍA 2", "Piernas completo + Abdomen",
            E("Sentadillas profundas", "2x10", "Calentamiento"),
            E("Sentadillas frontales con barra", "3x10"),
            E("Prensa unilateral", "4x12"),
            E("Step UP en cajón", "4x10"),
            E("Hip Thrust en máquina", "4x10"),
            E("Abductores en polea", "3x12"),
            E("Pantorrilla parado en máquina", "3x12"),
            E("Abdominales", "3x15", "Abdomen"),
            E("Plancha", "1 min", "Abdomen"),
            E("Bicho", "3x12", "Abdomen")),
        Day("DÍA 3", "Espalda + Bíceps + Abdomen",
            E("Dominadas agarre supino", "3x al fallo técnico", "Calentamiento"),
            E("Jalón al pecho con agarre abierto", "4x10"),
            E("Remo unilateral en polea", "4x12"),
            E("Press militar con mancuernas", "3x10"),
            E("Curl de bíceps sentado con barra Z", "4x10"),
            E("Curl de bíceps concentrado con mancuerna", "4x10"),
            E("Rodilla al pecho en paralelas", "3x10", "Abdomen"),
            E("Plancha", "1 min", "Abdomen"),
            E("Cruzados en colchoneta", "3x12", "Abdomen"))
    ];

    private static DaySeed Day(string nombre, string enfoque, params ExerciseSeed[] exercises) =>
        new(nombre, enfoque, exercises);

    private static ExerciseSeed E(string nombre, string sets, string? notes = null) =>
        new(nombre, sets, notes);

    private static (PlanEntrenamiento[] Plans, DiaEntrenamiento[] Days, EjercicioItem[] Exercises)
        BuildEntities(IEnumerable<PlanSeed> seeds)
    {
        var plans = new List<PlanEntrenamiento>();
        var days = new List<DiaEntrenamiento>();
        var exercises = new List<EjercicioItem>();

        foreach (var planSeed in seeds)
        {
            var planId = StableId($"rutina:{planSeed.Key}");
            plans.Add(new PlanEntrenamiento
            {
                Id = planId,
                UsuarioId = planSeed.UsuarioId,
                Titulo = planSeed.Titulo,
                FechaInicio = planSeed.FechaInicio ?? PendingStartDate,
                FechaRevision = planSeed.FechaRevision,
                IsActivo = true
            });

            for (var dayIndex = 0; dayIndex < planSeed.Days.Length; dayIndex++)
            {
                var daySeed = planSeed.Days[dayIndex];
                var dayId = StableId($"rutina:{planSeed.Key}:dia:{dayIndex + 1}");
                days.Add(new DiaEntrenamiento
                {
                    Id = dayId,
                    PlanEntrenamientoId = planId,
                    NombreDia = daySeed.NombreDia,
                    Orden = dayIndex + 1,
                    Enfoque = daySeed.Enfoque
                });

                for (var exerciseIndex = 0; exerciseIndex < daySeed.Ejercicios.Length; exerciseIndex++)
                {
                    var exercise = daySeed.Ejercicios[exerciseIndex];
                    exercises.Add(new EjercicioItem
                    {
                        Id = StableId($"rutina:{planSeed.Key}:dia:{dayIndex + 1}:ejercicio:{exerciseIndex + 1}"),
                        DiaEntrenamientoId = dayId,
                        Orden = exerciseIndex + 1,
                        Nombre = exercise.Nombre,
                        SeriesYRepeticiones = exercise.SeriesYRepeticiones,
                        Notas = exercise.Notas
                    });
                }
            }
        }

        return (plans.ToArray(), days.ToArray(), exercises.ToArray());
    }

    private static Guid StableId(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"OlympusGym:{key}"));
        return new Guid(hash.AsSpan(0, 16));
    }

    private sealed record PlanSeed(
        string Key,
        Guid UsuarioId,
        string Titulo,
        DateTime? FechaInicio,
        DateTime? FechaRevision,
        DaySeed[] Days);

    private sealed record DaySeed(string NombreDia, string Enfoque, ExerciseSeed[] Ejercicios);

    private sealed record ExerciseSeed(string Nombre, string SeriesYRepeticiones, string? Notas);
}
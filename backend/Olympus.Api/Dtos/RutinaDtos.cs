using System.ComponentModel.DataAnnotations;

namespace Olympus.Api.Dtos;

public record EjercicioRutinaDto(
    Guid Id,
    int Orden,
    string Nombre,
    string SeriesYRepeticiones,
    string? Notas);

public record DiaRutinaDto(
    Guid Id,
    string NombreDia,
    int Orden,
    string Enfoque,
    IReadOnlyList<EjercicioRutinaDto> Ejercicios);

public record RutinaDto(
    Guid Id,
    Guid UsuarioId,
    string NombreSocio,
    string Titulo,
    DateTime? FechaInicio,
    DateTime? FechaRevision,
    bool IsActivo,
    IReadOnlyList<DiaRutinaDto> Dias);

public record RutinaAsignacionDto(Guid UsuarioId, string NombreSocio);

public record RutinaCatalogoDto(
    string Titulo,
    IReadOnlyList<RutinaAsignacionDto> Asignaciones,
    int Dias,
    int Ejercicios,
    DateTime? FechaInicio,
    DateTime? FechaRevision,
    bool EsMiRutina);

public record EjercicioEntrenamientoDto(
    int Orden,
    string Nombre,
    string SeriesYRepeticiones,
    string? Notas);

public record DiaEntrenamientoDto(
    string NombreDia,
    int Orden,
    string Enfoque,
    List<EjercicioEntrenamientoDto> Ejercicios);

public class AsignarRutinaDto
{
    [Required]
    public Guid UsuarioId { get; set; }

    [Required, StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }
    public DateTime? FechaRevision { get; set; }

    [Required, MinLength(1), MaxLength(7)]
    public List<DiaEntrenamientoDto> Dias { get; set; } = [];
}

public record PlantillaRutinaDto(
    string Titulo,
    DateTime? FechaInicio,
    DateTime? FechaRevision,
    IReadOnlyList<DiaRutinaDto> Dias);

public class EjercicioRutinaRequestDto
{
    [Range(1, int.MaxValue)]
    public int Orden { get; set; }

    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string SeriesYRepeticiones { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notas { get; set; }
}

public class DiaRutinaRequestDto
{
    [Required, StringLength(30)]
    public string NombreDia { get; set; } = string.Empty;

    [Range(1, 7)]
    public int Orden { get; set; }

    [Required, StringLength(150)]
    public string Enfoque { get; set; } = string.Empty;

    public List<EjercicioRutinaRequestDto> Ejercicios { get; set; } = [];
}

public class CrearRutinaRequestDto
{
    [Required]
    public Guid UsuarioId { get; set; }

    [Required, StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    public DateTime? FechaRevision { get; set; }

    [Required, MinLength(1), MaxLength(7)]
    public List<DiaRutinaRequestDto> Dias { get; set; } = [];
}

public class ActualizarRutinaRequestDto
{
    [Required, StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    public DateTime? FechaRevision { get; set; }
    public bool IsActivo { get; set; }

    [Required, MinLength(1), MaxLength(7)]
    public List<DiaRutinaRequestDto> Dias { get; set; } = [];
}
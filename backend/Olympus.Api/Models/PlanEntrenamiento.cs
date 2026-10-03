namespace Olympus.Api.Models;

public class PlanEntrenamiento
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public required string Titulo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaRevision { get; set; }
    public bool IsActivo { get; set; } = true;

    public Usuario Usuario { get; set; } = null!;
    public ICollection<DiaEntrenamiento> Dias { get; set; } = new List<DiaEntrenamiento>();
}
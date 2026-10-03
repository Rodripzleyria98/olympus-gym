namespace Olympus.Api.Models;

public class EjercicioItem
{
    public Guid Id { get; set; }
    public Guid DiaEntrenamientoId { get; set; }
    public int Orden { get; set; }
    public required string Nombre { get; set; }
    public required string SeriesYRepeticiones { get; set; }
    public string? Notas { get; set; }

    public DiaEntrenamiento DiaEntrenamiento { get; set; } = null!;
}
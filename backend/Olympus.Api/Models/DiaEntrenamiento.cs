namespace Olympus.Api.Models;

public class DiaEntrenamiento
{
    public Guid Id { get; set; }
    public Guid PlanEntrenamientoId { get; set; }
    public required string NombreDia { get; set; }
    public int Orden { get; set; }
    public required string Enfoque { get; set; }

    public PlanEntrenamiento PlanEntrenamiento { get; set; } = null!;
    public ICollection<EjercicioItem> Ejercicios { get; set; } = new List<EjercicioItem>();
}
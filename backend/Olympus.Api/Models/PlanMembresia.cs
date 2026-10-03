namespace Olympus.Api.Models;

public class PlanMembresia
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int DuracionDias { get; set; }
    public int? ClasesIncluidas { get; set; }
    public bool IsActivo { get; set; } = true;

    public ICollection<MembresiaUsuario> Membresias { get; set; } = new List<MembresiaUsuario>();
}
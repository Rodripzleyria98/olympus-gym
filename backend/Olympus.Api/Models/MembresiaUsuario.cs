namespace Olympus.Api.Models;

public enum EstadoMembresia
{
    Pendiente,
    Vigente,
    Expirada,
    Cancelada
}

public class MembresiaUsuario
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public int PlanMembresiaId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public EstadoMembresia Estado { get; set; } = EstadoMembresia.Pendiente;

    public Usuario Usuario { get; set; } = null!;
    public PlanMembresia PlanMembresia { get; set; } = null!;
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
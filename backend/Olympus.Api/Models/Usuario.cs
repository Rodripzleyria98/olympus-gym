namespace Olympus.Api.Models;

public enum RolUsuario
{
    Admin,
    User
}

public class Usuario
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public RolUsuario Rol { get; set; } = RolUsuario.User;
    public DateTime FechaRegistro { get; set; }

    public ICollection<MembresiaUsuario> Membresias { get; set; } = new List<MembresiaUsuario>();
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    public ICollection<PublicacionForo> PublicacionesForo { get; set; } = new List<PublicacionForo>();
    public ICollection<PlanEntrenamiento> PlanesEntrenamiento { get; set; } = new List<PlanEntrenamiento>();
}
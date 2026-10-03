namespace Olympus.Api.Models;

public enum EstadoPago
{
    Aprobado,
    Pendiente,
    Rechazado
}

public class Pago
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid MembresiaUsuarioId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public required string MetodoPago { get; set; }
    public string? ComprobanteUrl { get; set; }
    public EstadoPago EstadoPago { get; set; } = EstadoPago.Pendiente;

    public Usuario Usuario { get; set; } = null!;
    public MembresiaUsuario MembresiaUsuario { get; set; } = null!;
}
namespace Olympus.Api.Models;

public class PaseDiario
{
    public int Id { get; set; }
    public required string NombreCompleto { get; set; }
    public required string Dni { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public required string MetodoPago { get; set; }
    public Guid RegistradoPorUsuarioId { get; set; }
    public int? CajaTurnoId { get; set; }

    public Usuario RegistradoPorUsuario { get; set; } = null!;
    public CajaTurno? CajaTurno { get; set; }
}

namespace Olympus.Api.Models;

public class CajaTurno
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public required string Turno { get; set; }
    public required string ResponsableTurno { get; set; }
    public required string ResponsableTurnoNormalizado { get; set; }
    public Guid UsuarioAperturaId { get; set; }
    public Guid? UsuarioCierreId { get; set; }
    public decimal MontoInicial { get; set; }
    public decimal MontoFinalEfectivoEsperado { get; set; }
    public decimal? MontoFinalEfectivoReal { get; set; }
    public decimal? Diferencia { get; set; }
    public decimal TotalCobrosEfectivo { get; set; }
    public decimal TotalCobrosDigitales { get; set; }
    public string? Observaciones { get; set; }
    public required string Estado { get; set; }

    public Usuario UsuarioApertura { get; set; } = null!;
    public Usuario? UsuarioCierre { get; set; }
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    public ICollection<VentaProducto> Ventas { get; set; } = new List<VentaProducto>();
    public ICollection<PaseDiario> PasesDiarios { get; set; } = new List<PaseDiario>();
}

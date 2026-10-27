namespace Olympus.Api.Models;

public class Producto
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public string? Sabor { get; set; }
    public string? Descripcion { get; set; }
    public required string Categoria { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public decimal Precio { get; set; }
    public string? Codigo { get; set; }
    public bool IsActivo { get; set; } = true;
}

public class VentaProducto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public Guid UsuarioId { get; set; }
    public int? CajaTurnoId { get; set; }
    public required string MetodoPago { get; set; }
    public decimal MontoTotal { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public CajaTurno? CajaTurno { get; set; }
    public ICollection<VentaProductoItem> Items { get; set; } = new List<VentaProductoItem>();
}

public class VentaProductoItem
{
    public int Id { get; set; }
    public int VentaProductoId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    public VentaProducto VentaProducto { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}

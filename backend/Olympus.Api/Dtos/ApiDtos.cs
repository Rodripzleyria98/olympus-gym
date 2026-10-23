using System.ComponentModel.DataAnnotations;
using Olympus.Api.Models;

namespace Olympus.Api.Dtos;

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPrevious,
    bool HasNext)
{
    public static PagedResponse<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResponse<T>(items, page, pageSize, totalCount, totalPages, page > 1, page < totalPages);
    }
}

public record UsuarioDto(Guid Id, string Nombre, string Apellido, string Email, string Dni, string Rol);

public record AuthResponseDto(string Token, DateTime ExpiresAt, UsuarioDto Usuario);

public class RegisterRequestDto
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{7,12}$", ErrorMessage = "El DNI debe contener entre 7 y 12 dígitos.")]
    public string Dni { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9\s]).{8,100}$",
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres, una mayúscula, un número y un carácter especial.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public record UsuarioAdminDto(
    Guid Id,
    string Nombre,
    string Apellido,
    string Email,
    string Dni,
    DateTime FechaRegistro,
    bool IsActivo,
    bool EstaAlDia,
    DateTime? FechaVencimiento,
    int DiasRestantes);

public record UsuarioRolAdminDto(
    Guid Id,
    string Nombre,
    string Apellido,
    string Email,
    string Dni,
    string Rol);

public class CambiarRolUsuarioRequestDto
{
    [Required]
    public string Rol { get; set; } = string.Empty;
}

public record PagoDto(
    Guid Id,
    decimal Monto,
    DateTime FechaPago,
    string MetodoPago,
    string? ComprobanteUrl,
    string EstadoPago);

public record MembresiaHistorialDto(
    Guid Id,
    string Plan,
    DateTime FechaInicio,
    DateTime FechaFin,
    string Estado,
    IReadOnlyList<PagoDto> Pagos);

public record HistorialUsuarioDto(
    UsuarioDto Usuario,
    IReadOnlyList<MembresiaHistorialDto> Membresias,
    IReadOnlyList<PagoDto> Pagos);

public class CambiarEstadoUsuarioRequestDto
{
    public Guid? MembresiaUsuarioId { get; set; }
    public EstadoMembresia? EstadoMembresia { get; set; }
    public Guid? PagoId { get; set; }
    public EstadoPago? EstadoPago { get; set; }
}

public record PublicacionDto(
    int Id,
    string Titulo,
    string Contenido,
    string? ImagenUrl,
    DateTime FechaPublicacion,
    Guid AutorId,
    string AutorNombre);

public class PublicacionRequestDto
{
    [Required, StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required, StringLength(10000)]
    public string Contenido { get; set; } = string.Empty;

    public IFormFile? Imagen { get; set; }
}

public record PlanMembresiaDto(
    int Id,
    string Nombre,
    string Descripcion,
    decimal Precio,
    int DuracionDias,
    int? ClasesIncluidas);

public class EditarPlanMembresiaRequestDto
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal Precio { get; set; }
}

public record DatosTransferenciaDto(string Titular, string Cbu, string Alias);

public class CrearPreferenciaPagoRequestDto
{
    [Range(1, int.MaxValue)]
    public int PlanMembresiaId { get; set; }
}

public record CrearPreferenciaPagoResponseDto(
    Guid PagoId,
    string PreferenceId,
    string InitPoint,
    string SandboxInitPoint);

public record CrearTransferenciaRequestDto(int PlanMembresiaId);

public record EstadoPagoResponseDto(Guid PagoId, string PlanNombre, decimal Monto, string EstadoPago);

public record MercadoPagoWebhookNotificationDto(string? Type, MercadoPagoWebhookDataDto? Data);

public record MercadoPagoWebhookDataDto(string? Id);

public record PerfilResponseDto(
    UsuarioDto Usuario,
    bool IsActivo,
    string EstadoMembresia,
    string? Plan,
    DateTime? FechaInicio,
    DateTime? FechaVencimiento,
    int DiasRestantes,
    string? EstadoPago,
    decimal? MontoPago);

public record AccesoSocioResponseDto(bool PuedeIngresar);

public class ActualizarPerfilRequestDto
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{7,12}$")]
    public string Dni { get; set; } = string.Empty;
}

public record CajaTurnoDto(
    int Id, DateTime Fecha, string Turno, string ResponsableTurno, Guid UsuarioAperturaId, Guid? UsuarioCierreId,
    decimal MontoInicial, decimal MontoFinalEfectivoEsperado, decimal? MontoFinalEfectivoReal,
    decimal? Diferencia, decimal TotalCobrosEfectivo, decimal TotalCobrosDigitales,
    string? Observaciones, string Estado);

public class AbrirCajaRequestDto
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string ResponsableTurno { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999.99")]
    public decimal MontoInicial { get; set; }
}

public class CerrarCajaRequestDto
{
    [Range(typeof(decimal), "0", "999999999.99")]
    public decimal MontoFinalEfectivoReal { get; set; }
    [StringLength(2000)]
    public string? Observaciones { get; set; }
}

public record ProductoDto(int Id, string Nombre, int StockActual, int StockMinimo, decimal Precio, string? Codigo);

public class CrearProductoRequestDto
{
    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;
    [Range(0, int.MaxValue)]
    public int StockActual { get; set; }
    [Range(0, int.MaxValue)]
    public int StockMinimo { get; set; }
    [Range(typeof(decimal), "0.01", "999999999.99")]
    public decimal Precio { get; set; }
    [StringLength(100)]
    public string? Codigo { get; set; }
}

public class AjustarStockRequestDto
{
    [Range(0, int.MaxValue)]
    public int StockActual { get; set; }
}

public class VentaProductoRequestDto
{
    [Required, StringLength(30)]
    public string MetodoPago { get; set; } = string.Empty;
    [Required, MinLength(1), MaxLength(50)]
    public List<ItemVentaProductoDto> Items { get; set; } = [];
}

public class ItemVentaProductoDto
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }
    [Range(1, 10000)]
    public int Cantidad { get; set; }
}

public record VentaProductoResponseDto(int Id, decimal MontoTotal, string MetodoPago, IReadOnlyList<ItemVentaResponseDto> Items);
public record ItemVentaResponseDto(int ProductoId, string Producto, int Cantidad, decimal PrecioUnitario);

public class CrearPaseDiarioRequestDto
{
    [Required, StringLength(200)]
    public string NombreCompleto { get; set; } = string.Empty;
    [Required, RegularExpression(@"^\d{7,12}$")]
    public string Dni { get; set; } = string.Empty;
    [Required, RegularExpression(@"^(Efectivo|MercadoPago|Transferencia)$")]
    public string MetodoPago { get; set; } = string.Empty;
}

public record PaseDiarioResponseDto(int Id, string NombreCompleto, string Dni, DateTime Fecha, decimal Monto, string MetodoPago);

public class PagoEfectivoRequestDto
{
    [Required]
    public Guid UsuarioId { get; set; }

    [Range(1, int.MaxValue)]
    public int PlanMembresiaId { get; set; }
    public string MetodoPago { get; set; } = "Efectivo";
}
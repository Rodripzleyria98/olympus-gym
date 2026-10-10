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

public record UsuarioDto(Guid Id, string Nombre, string Apellido, string Email, string Rol);

public record AuthResponseDto(string Token, DateTime ExpiresAt, UsuarioDto Usuario);

public class RegisterRequestDto
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Apellido { get; set; } = string.Empty;

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
    DateTime FechaRegistro,
    bool IsActivo,
    bool EstaAlDia,
    DateTime? FechaVencimiento,
    int DiasRestantes);

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

    [Url, StringLength(2048)]
    public string? ImagenUrl { get; set; }
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
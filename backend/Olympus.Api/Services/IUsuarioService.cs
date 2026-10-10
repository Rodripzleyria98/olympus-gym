using Olympus.Api.Dtos;

namespace Olympus.Api.Services;

public interface IUsuarioService
{
    Task<PagedResponse<UsuarioAdminDto>> GetUsuariosAsync(
        string? buscar,
        string? estado,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
    Task<HistorialUsuarioDto?> GetHistorialAsync(Guid usuarioId, CancellationToken cancellationToken);
    Task<bool> CambiarEstadoAsync(Guid usuarioId, CambiarEstadoUsuarioRequestDto request, CancellationToken cancellationToken);
}
using Olympus.Api.Dtos;

namespace Olympus.Api.Services;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioAdminDto>> GetUsuariosAsync(string? buscar, string? estado, CancellationToken cancellationToken);
    Task<HistorialUsuarioDto?> GetHistorialAsync(Guid usuarioId, CancellationToken cancellationToken);
    Task<bool> CambiarEstadoAsync(Guid usuarioId, CambiarEstadoUsuarioRequestDto request, CancellationToken cancellationToken);
}
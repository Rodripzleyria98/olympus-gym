using Olympus.Api.Dtos;

namespace Olympus.Api.Services;

public interface IMembresiaService
{
    Task<IReadOnlyList<PlanMembresiaDto>> GetPlanesAsync(CancellationToken cancellationToken);
    Task<PlanMembresiaDto?> UpdatePlanAsync(int planId, EditarPlanMembresiaRequestDto request, CancellationToken cancellationToken);
    Task<PerfilResponseDto?> GetPerfilAsync(Guid usuarioId, CancellationToken cancellationToken);
}
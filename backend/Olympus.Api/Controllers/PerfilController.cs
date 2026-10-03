using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olympus.Api.Dtos;
using Olympus.Api.Services;

namespace Olympus.Api.Controllers;

[ApiController]
[Route("api/perfil")]
[Authorize]
public class PerfilController(IMembresiaService membresiaService) : ControllerBase
{
    [HttpGet("mi-estado")]
    public async Task<ActionResult<PerfilResponseDto>> GetMiEstado(CancellationToken cancellationToken)
    {
        var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var perfil = await membresiaService.GetPerfilAsync(usuarioId, cancellationToken);
        return perfil is null ? NotFound() : Ok(perfil);
    }
}
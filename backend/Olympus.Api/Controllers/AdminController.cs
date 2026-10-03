using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olympus.Api.Dtos;
using Olympus.Api.Services;

namespace Olympus.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController(IUsuarioService usuarioService, IMembresiaService membresiaService) : ControllerBase
{
    [HttpGet("usuarios")]
    public async Task<ActionResult<IReadOnlyList<UsuarioAdminDto>>> GetUsuarios(
        [FromQuery] string? buscar,
        [FromQuery] string? estado,
        CancellationToken cancellationToken) =>
        Ok(await usuarioService.GetUsuariosAsync(buscar, estado, cancellationToken));

    [HttpGet("usuarios/{id:guid}/historial")]
    public async Task<ActionResult<HistorialUsuarioDto>> GetHistorial(Guid id, CancellationToken cancellationToken)
    {
        var historial = await usuarioService.GetHistorialAsync(id, cancellationToken);
        return historial is null ? NotFound() : Ok(historial);
    }

    [HttpPut("usuarios/{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(
        Guid id,
        CambiarEstadoUsuarioRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await usuarioService.CambiarEstadoAsync(id, request, cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("membresias/{id:int}")]
    public async Task<ActionResult<PlanMembresiaDto>> EditarMembresia(
        int id,
        EditarPlanMembresiaRequestDto request,
        CancellationToken cancellationToken)
    {
        var plan = await membresiaService.UpdatePlanAsync(id, request, cancellationToken);
        return plan is null ? NotFound() : Ok(plan);
    }
}
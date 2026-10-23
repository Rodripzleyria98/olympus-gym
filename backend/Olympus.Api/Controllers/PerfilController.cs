using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Olympus.Api.Data;
using Olympus.Api.Models;
using Olympus.Api.Dtos;
using Olympus.Api.Services;

namespace Olympus.Api.Controllers;

[ApiController]
[Route("api/perfil")]
[Authorize]
public class PerfilController(IMembresiaService membresiaService, OlympusDbContext dbContext) : ControllerBase
{
    [HttpGet("mi-estado")]
    public async Task<ActionResult<PerfilResponseDto>> GetMiEstado(CancellationToken cancellationToken)
    {
        var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var perfil = await membresiaService.GetPerfilAsync(usuarioId, cancellationToken);
        return perfil is null ? NotFound() : Ok(perfil);
    }

    [HttpPut]
    public async Task<IActionResult> Actualizar(
        ActualizarPerfilRequestDto request,
        CancellationToken cancellationToken)
    {
        var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(item => item.Id == usuarioId, cancellationToken);
        if (usuario is null) return NotFound();

        var dni = request.Dni.Trim();
        if (await dbContext.Usuarios.AnyAsync(item => item.Id != usuarioId && item.Dni == dni, cancellationToken))
            return Conflict(new { message = "El DNI ya está registrado." });

        usuario.Nombre = request.Nombre.Trim();
        usuario.Apellido = request.Apellido.Trim();
        usuario.Dni = dni;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
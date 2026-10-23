using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Olympus.Api.Data;
using Olympus.Api.Models;
using Olympus.Api.Dtos;
using Olympus.Api.Services;

namespace Olympus.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "StaffPolicy")]
public class AdminController(
    IUsuarioService usuarioService,
    IMembresiaService membresiaService,
    OlympusDbContext dbContext) : ControllerBase
{
    [HttpGet("usuarios/roles")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IReadOnlyList<UsuarioRolAdminDto>>> GetUsuariosRoles(
        CancellationToken cancellationToken) =>
        Ok(await dbContext.Usuarios.AsNoTracking()
            .OrderBy(usuario => usuario.Nombre)
            .ThenBy(usuario => usuario.Apellido)
            .Select(usuario => new UsuarioRolAdminDto(
                usuario.Id, usuario.Nombre, usuario.Apellido, usuario.Email, usuario.Dni, usuario.Rol.ToString()))
            .ToListAsync(cancellationToken));

    [HttpPut("usuarios/{id:guid}/rol")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CambiarRol(
        Guid id,
        CambiarRolUsuarioRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<RolUsuario>(request.Rol.Trim(), ignoreCase: true, out var nuevoRol) ||
            !Enum.IsDefined(nuevoRol) ||
            nuevoRol == RolUsuario.User)
            return BadRequest(new { message = "El rol debe ser Admin, Profesor o Socio." });

        var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (usuario is null) return NotFound();
        if (usuario.Rol == nuevoRol) return NoContent();

        if (usuario.Rol == RolUsuario.Admin && nuevoRol != RolUsuario.Admin &&
            await dbContext.Usuarios.CountAsync(item => item.Rol == RolUsuario.Admin, cancellationToken) == 1)
            return Conflict(new { message = "No se puede quitar el rol al único administrador." });

        usuario.Rol = nuevoRol;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("usuarios")]
    public async Task<ActionResult<PagedResponse<UsuarioAdminDto>>> GetUsuarios(
        [FromQuery] string? buscar,
        [FromQuery] string? estado,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        Ok(await usuarioService.GetUsuariosAsync(
            buscar,
            estado,
            Math.Max(page, 1),
            Math.Clamp(pageSize, 1, 50),
            cancellationToken));

    [HttpGet("usuarios/{id:guid}/historial")]
    public async Task<ActionResult<HistorialUsuarioDto>> GetHistorial(Guid id, CancellationToken cancellationToken)
    {
        var historial = await usuarioService.GetHistorialAsync(id, cancellationToken);
        return historial is null ? NotFound() : Ok(historial);
    }

    [HttpPut("usuarios/{id:guid}/estado")]
    [Authorize(Policy = "AdminOnly")]
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
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PlanMembresiaDto>> EditarMembresia(
        int id,
        EditarPlanMembresiaRequestDto request,
        CancellationToken cancellationToken)
    {
        var plan = await membresiaService.UpdatePlanAsync(id, request, cancellationToken);
        return plan is null ? NotFound() : Ok(plan);
    }

    [HttpPut("usuarios/{id:guid}/perfil")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ActualizarPerfilSocio(
        Guid id,
        ActualizarPerfilRequestDto request,
        CancellationToken cancellationToken)
    {
        var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (usuario is null) return NotFound();
        var dni = request.Dni.Trim();
        if (await dbContext.Usuarios.AnyAsync(item => item.Id != id && item.Dni == dni, cancellationToken))
            return Conflict(new { message = "El DNI ya está registrado." });
        usuario.Nombre = request.Nombre.Trim();
        usuario.Apellido = request.Apellido.Trim();
        usuario.Dni = dni;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
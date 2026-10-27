using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Olympus.Api.Data;
using Olympus.Api.Models;
using Olympus.Api.Dtos;
using Olympus.Api.Services;
using Npgsql;

namespace Olympus.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "StaffPolicy")]
public class AdminController(
    IUsuarioService usuarioService,
    IMembresiaService membresiaService,
    OlympusDbContext dbContext) : ControllerBase
{
    [HttpPost("membresias/asignar")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SocioConMembresiaAdminResponseDto>> AsignarMembresia(
        AsignarMembresiaAdminRequestDto request,
        CancellationToken cancellationToken)
    {
        var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(item =>
            item.Id == request.UsuarioId && (item.Rol == RolUsuario.Socio || item.Rol == RolUsuario.User),
            cancellationToken);
        if (usuario is null) return NotFound(new { message = "No se encontró un socio registrado." });
        var plan = await dbContext.PlanesMembresia.FirstOrDefaultAsync(item =>
            item.Id == request.PlanMembresiaId && item.IsActivo, cancellationToken);
        if (plan is null) return NotFound(new { message = "No se encontró el plan activo." });

        var caja = request.TipoOtorgamiento == "Bonificada"
            ? null
            : await ObtenerCajaAbiertaAsync(cancellationToken);
        if (request.TipoOtorgamiento != "Bonificada" && caja is null)
            return Conflict(new { message = "Debe haber una caja abierta para registrar el cobro en efectivo." });

        var response = await CrearMembresiaAsync(
            usuario, plan, request.TipoOtorgamiento, request.FechaInicio, caja, cancellationToken);
        return Ok(response);
    }

    [HttpPost("usuarios/crear-con-membresia")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SocioConMembresiaAdminResponseDto>> CrearSocioConMembresia(
        RegistrarSocioConMembresiaRequestDto request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var dni = request.Dni.Trim();
        if (await dbContext.Usuarios.AnyAsync(usuario => usuario.Email == email, cancellationToken))
            return Conflict(new { message = "El correo ya está registrado." });
        if (await dbContext.Usuarios.AnyAsync(usuario => usuario.Dni == dni, cancellationToken))
            return Conflict(new { message = "El DNI ya está registrado." });
        var plan = await dbContext.PlanesMembresia.FirstOrDefaultAsync(item =>
            item.Id == request.PlanMembresiaId && item.IsActivo, cancellationToken);
        if (plan is null) return NotFound(new { message = "No se encontró el plan activo." });

        var caja = request.TipoOtorgamiento == "Bonificada"
            ? null
            : await ObtenerCajaAbiertaAsync(cancellationToken);
        if (request.TipoOtorgamiento != "Bonificada" && caja is null)
            return Conflict(new { message = "Debe haber una caja abierta para registrar el cobro en efectivo." });

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Dni = dni,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Rol = RolUsuario.Socio,
            FechaRegistro = DateTime.UtcNow
        };
        dbContext.Usuarios.Add(usuario);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var response = await CrearMembresiaAsync(
                usuario, plan, request.TipoOtorgamiento, request.FechaInicio, caja, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return CreatedAtAction(nameof(GetHistorial), new { id = usuario.Id }, response);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new { message = "El correo o DNI ya está registrado." });
        }
    }

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

    [HttpGet("pagos/pendientes")]
    public async Task<ActionResult<IReadOnlyList<PagoPendienteAdminDto>>> GetPagosPendientes(
        CancellationToken cancellationToken) =>
        Ok(await usuarioService.GetPagosPendientesAsync(cancellationToken));

    [HttpPut("pagos/{pagoId:guid}/validar")]
    public async Task<IActionResult> ValidarPagoPendiente(Guid pagoId, CancellationToken cancellationToken)
    {
        if (!await usuarioService.ValidarPagoPendienteAsync(pagoId, cancellationToken))
            return Conflict(new { message = "El pago ya fue procesado o no es una transferencia pendiente de validar." });
        return NoContent();
    }

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

    private async Task<CajaTurno?> ObtenerCajaAbiertaAsync(CancellationToken cancellationToken)
    {
        return await dbContext.CajasTurno.OrderByDescending(caja => caja.FechaHoraApertura.HasValue)
            .ThenByDescending(caja => caja.FechaHoraApertura).ThenByDescending(caja => caja.Id)
            .FirstOrDefaultAsync(caja => caja.Estado == "Abierta", cancellationToken);
    }

    private async Task<SocioConMembresiaAdminResponseDto> CrearMembresiaAsync(
        Usuario usuario,
        PlanMembresia plan,
        string tipoOtorgamiento,
        DateOnly? fechaInicioPersonalizada,
        CajaTurno? caja,
        CancellationToken cancellationToken)
    {
        var ahora = DateTime.UtcNow;
        var fechaFinVigente = await dbContext.MembresiasUsuario
            .Where(item => item.UsuarioId == usuario.Id &&
                item.Estado == EstadoMembresia.Vigente && item.FechaFin >= ahora)
            .Select(item => (DateTime?)item.FechaFin)
            .MaxAsync(cancellationToken);
        var inicio = fechaInicioPersonalizada is { } fechaPersonalizada
            ? CajaTurnoHelper.FechaLocalAUtc(fechaPersonalizada)
            : (fechaFinVigente is { } fin && fin > ahora ? fin : ahora);
        var membresia = new MembresiaUsuario
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            PlanMembresiaId = plan.Id,
            FechaInicio = inicio,
            FechaFin = inicio.AddDays(plan.DuracionDias),
            Estado = tipoOtorgamiento == "Transferencia" ? EstadoMembresia.Pendiente : EstadoMembresia.Vigente
        };
        dbContext.MembresiasUsuario.Add(membresia);

        if (tipoOtorgamiento is "Efectivo" or "Transferencia")
        {
            dbContext.Pagos.Add(new Pago
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuario.Id,
                MembresiaUsuarioId = membresia.Id,
                CajaTurnoId = caja!.Id,
                Monto = plan.Precio,
                FechaCreacion = ahora,
                FechaAcreditacion = tipoOtorgamiento == "Efectivo" ? ahora : null,
                MetodoPago = tipoOtorgamiento,
                PeriodoMeses = Math.Max(1, (int)Math.Round(plan.DuracionDias / 30.0, MidpointRounding.AwayFromZero)),
                EstadoPago = tipoOtorgamiento == "Efectivo" ? EstadoPago.Aprobado : EstadoPago.Pendiente
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new SocioConMembresiaAdminResponseDto(
            usuario.Id,
            $"{usuario.Nombre} {usuario.Apellido}",
            usuario.Dni,
            plan.Nombre,
            membresia.FechaInicio,
            membresia.FechaFin,
            tipoOtorgamiento);
    }
}
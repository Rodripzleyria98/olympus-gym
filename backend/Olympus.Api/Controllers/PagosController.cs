using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Olympus.Api.Dtos;
using Olympus.Api.Data;
using Olympus.Api.Models;
using Olympus.Api.Services;

namespace Olympus.Api.Controllers;

[ApiController]
[Route("api/pagos")]
[Authorize(Roles = "User,Socio,Admin,Profesor")]
public class PagosController(
    PagoService pagoService,
    IConfiguration configuration,
    ILogger<PagosController> logger,
    OlympusDbContext dbContext) : ControllerBase
{
    [HttpGet("datos-transferencia")]
    [Authorize(Roles = "User,Socio")]
    public ActionResult<DatosTransferenciaDto> ObtenerDatosTransferencia() => Ok(new DatosTransferenciaDto(
        configuration["Transferencia:Titular"] ?? string.Empty,
        configuration["Transferencia:CBU"] ?? string.Empty,
        configuration["Transferencia:Alias"] ?? string.Empty));

    [HttpPost("crear-preferencia")]
    [Authorize(Roles = "User,Socio")]
    public async Task<ActionResult<CrearPreferenciaPagoResponseDto>> CrearPreferencia(
        CrearPreferenciaPagoRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!pagoService.IsConfiguredForPayments)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Mercado Pago no está configurado"
            });
        }

        var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await pagoService.CrearPreferenciaRenovacionAsync(
            usuarioId, request.PlanMembresiaId, cancellationToken);

        return response is null
            ? NotFound(new { message = "No se encontró el socio o la membresía seleccionada." })
            : CreatedAtAction(nameof(ObtenerEstado), new { pagoId = response.PagoId }, response);
    }

    [HttpPost("crear-transferencia")]
    [Authorize(Roles = "User,Socio")]
    public async Task<ActionResult<EstadoPagoResponseDto>> CrearTransferencia(
        CrearTransferenciaRequestDto request,
        CancellationToken cancellationToken)
    {
        var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await pagoService.CrearSolicitudTransferenciaAsync(
            usuarioId, request.PlanMembresiaId, cancellationToken);

        return response is null
            ? NotFound(new { message = "No se encontró el socio o la membresía seleccionada." })
            : CreatedAtAction(nameof(ObtenerEstado), new { pagoId = response.PagoId }, response);
    }

    [HttpPost("efectivo")]
    [Authorize(Roles = "Admin,Profesor")]
    public async Task<ActionResult<EstadoPagoResponseDto>> RegistrarCobro(
        PagoEfectivoRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.MetodoPago != "Efectivo")
            return BadRequest(new { message = "Este endpoint solo registra cobros en efectivo." });
        var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(item =>
            item.Id == request.UsuarioId &&
            (item.Rol == RolUsuario.User || item.Rol == RolUsuario.Socio), cancellationToken);
        var plan = await dbContext.PlanesMembresia.FirstOrDefaultAsync(item =>
            item.Id == request.PlanMembresiaId && item.IsActivo, cancellationToken);
        if (usuario is null || plan is null) return NotFound();

        var (fechaTurno, turno) = CajaTurnoHelper.TurnoActual();
        var caja = await dbContext.CajasTurno.FirstOrDefaultAsync(item =>
            item.Fecha == fechaTurno && item.Turno == turno && item.Estado == "Abierta", cancellationToken);
        if (request.MetodoPago == "Efectivo" && caja is null)
            return Conflict(new { message = "Debe haber una caja abierta para cobrar en efectivo." });

        var ahora = DateTime.UtcNow;
        var vigenteHasta = await dbContext.MembresiasUsuario
            .Where(item => item.UsuarioId == usuario.Id && item.Estado == EstadoMembresia.Vigente && item.FechaFin >= ahora)
            .Select(item => (DateTime?)item.FechaFin)
            .MaxAsync(cancellationToken);
        var inicio = vigenteHasta is { } fechaFin && fechaFin > ahora ? fechaFin : ahora;
        var periodoMeses = Math.Max(1, (int)Math.Round(plan.DuracionDias / 30.0, MidpointRounding.AwayFromZero));
        var membresia = new MembresiaUsuario
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            PlanMembresiaId = plan.Id,
            FechaInicio = inicio,
            FechaFin = inicio.AddMonths(periodoMeses),
            Estado = EstadoMembresia.Vigente
        };
        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            MembresiaUsuarioId = membresia.Id,
            CajaTurnoId = caja?.Id,
            Monto = plan.Precio,
            FechaCreacion = ahora,
            FechaAcreditacion = ahora,
            MetodoPago = request.MetodoPago,
            PeriodoMeses = periodoMeses,
            EstadoPago = EstadoPago.Aprobado
        };
        dbContext.MembresiasUsuario.Add(membresia);
        dbContext.Pagos.Add(pago);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new EstadoPagoResponseDto(pago.Id, plan.Nombre, pago.Monto, pago.EstadoPago.ToString()));
    }

    [HttpGet("{pagoId:guid}")]
    public async Task<ActionResult<EstadoPagoResponseDto>> ObtenerEstado(
        Guid pagoId,
        CancellationToken cancellationToken)
    {
        var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var estado = await pagoService.ObtenerEstadoAsync(usuarioId, pagoId, cancellationToken);
        return estado is null ? NotFound() : Ok(estado);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(
        [FromQuery] string? type,
        [FromQuery] string? topic,
        [FromQuery] string? id,
        [FromQuery(Name = "data.id")] string? dataId,
        [FromHeader(Name = "x-signature")] string? signature,
        [FromHeader(Name = "x-request-id")] string? requestId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] MercadoPagoWebhookNotificationDto? notification,
        CancellationToken cancellationToken)
    {
        var eventType = type ?? topic ?? notification?.Type;
        if (!string.Equals(eventType, "payment", StringComparison.OrdinalIgnoreCase))
        {
            return Ok();
        }

        var resourceId = dataId ?? id ?? notification?.Data?.Id;
        var webhookSecret = configuration["MercadoPago:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(resourceId) ||
            string.IsNullOrWhiteSpace(webhookSecret) ||
            !PagoService.ValidarFirmaWebhook(resourceId, requestId, signature, webhookSecret))
        {
            return Unauthorized();
        }

        if (!long.TryParse(resourceId, out var mercadoPagoPaymentId))
        {
            return BadRequest();
        }

        try
        {
            await pagoService.ProcesarNotificacionAsync(mercadoPagoPaymentId, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(exception, "No se pudo procesar la notificación de pago {PaymentId}.", mercadoPagoPaymentId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return Ok();
    }

}
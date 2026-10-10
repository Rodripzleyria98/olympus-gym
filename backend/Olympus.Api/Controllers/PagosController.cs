using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Olympus.Api.Dtos;
using Olympus.Api.Services;

namespace Olympus.Api.Controllers;

[ApiController]
[Route("api/pagos")]
[Authorize(Roles = "User,Admin")]
public class PagosController(
    PagoService pagoService,
    IConfiguration configuration,
    ILogger<PagosController> logger) : ControllerBase
{
    [HttpGet("datos-transferencia")]
    [Authorize(Roles = "User")]
    public ActionResult<DatosTransferenciaDto> ObtenerDatosTransferencia() => Ok(new DatosTransferenciaDto(
        configuration["Transferencia:Titular"] ?? string.Empty,
        configuration["Transferencia:CBU"] ?? string.Empty,
        configuration["Transferencia:Alias"] ?? string.Empty));

    [HttpPost("crear-preferencia")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<CrearPreferenciaPagoResponseDto>> CrearPreferencia(
        CrearPreferenciaPagoRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!pagoService.IsConfiguredForPayments)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Mercado Pago no está configurado con credenciales y URLs válidas para este entorno."
            });
        }

        var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await pagoService.CrearPreferenciaRenovacionAsync(
            usuarioId, request.PlanMembresiaId, cancellationToken);

        return response is null
            ? NotFound(new { message = "No se encontró el socio o la membresía seleccionada." })
            : CreatedAtAction(nameof(ObtenerEstado), new { pagoId = response.PagoId }, response);
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
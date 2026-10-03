using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olympus.Api.Dtos;
using Olympus.Api.Services;

namespace Olympus.Api.Controllers;

[ApiController]
[Route("api/pagos")]
[Authorize(Roles = "User,Admin")]
public class PagosController(IMembresiaService membresiaService) : ControllerBase
{
    [HttpPost("checkout")]
    public async Task<ActionResult<PagoCheckoutResponseDto>> Checkout(
        PagoCheckoutRequestDto request,
        CancellationToken cancellationToken)
    {
        Guid usuarioId;
        if (User.IsInRole("Admin"))
        {
            if (request.UsuarioId is null)
            {
                return BadRequest(new { message = "El administrador debe indicar el socio al que aplicará el pago." });
            }

            usuarioId = request.UsuarioId.Value;
        }
        else
        {
            usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (request.UsuarioId is not null && request.UsuarioId != usuarioId)
            {
                return Forbid();
            }
        }

        var checkout = await membresiaService.CreateApprovedCheckoutAsync(
            usuarioId, request.PlanMembresiaId, request.MetodoPago, cancellationToken);
        return checkout is null
            ? NotFound(new { message = "No se encontró el socio o la membresía seleccionada." })
            : Ok(checkout);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olympus.Api.Dtos;
using Olympus.Api.Services;

namespace Olympus.Api.Controllers;

[ApiController]
[Route("api/membresias")]
public class MembresiasController(IMembresiaService membresiaService) : ControllerBase
{
    [HttpGet]
    [HttpGet("planes")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<PlanMembresiaDto>>> GetPlanes(CancellationToken cancellationToken) =>
        Ok(await membresiaService.GetPlanesAsync(cancellationToken));
}
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Olympus.Api.Data;
using Olympus.Api.Dtos;
using Olympus.Api.Models;

namespace Olympus.Api.Controllers;

[ApiController]
[Route("api/foro")]
public class ForoController(OlympusDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PublicacionDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await dbContext.PublicacionesForo.AsNoTracking()
            .OrderByDescending(publicacion => publicacion.FechaPublicacion)
            .Select(publicacion => new PublicacionDto(
                publicacion.Id,
                publicacion.Titulo,
                publicacion.Contenido,
                publicacion.ImagenUrl,
                publicacion.FechaPublicacion,
                publicacion.AutorId,
                publicacion.Autor.Nombre + " " + publicacion.Autor.Apellido))
            .ToListAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PublicacionDto>> Create(
        PublicacionRequestDto request,
        CancellationToken cancellationToken)
    {
        var publicacion = new PublicacionForo
        {
            Titulo = request.Titulo.Trim(),
            Contenido = request.Contenido.Trim(),
            ImagenUrl = request.ImagenUrl,
            FechaPublicacion = DateTime.UtcNow,
            AutorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
        };

        dbContext.PublicacionesForo.Add(publicacion);
        await dbContext.SaveChangesAsync(cancellationToken);
        await dbContext.Entry(publicacion).Reference(item => item.Autor).LoadAsync(cancellationToken);
        var response = new PublicacionDto(
            publicacion.Id,
            publicacion.Titulo,
            publicacion.Contenido,
            publicacion.ImagenUrl,
            publicacion.FechaPublicacion,
            publicacion.AutorId,
            $"{publicacion.Autor.Nombre} {publicacion.Autor.Apellido}");
        return Created($"/api/foro/{publicacion.Id}", response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, PublicacionRequestDto request, CancellationToken cancellationToken)
    {
        var publicacion = await dbContext.PublicacionesForo.FindAsync([id], cancellationToken);
        if (publicacion is null)
        {
            return NotFound();
        }

        publicacion.Titulo = request.Titulo.Trim();
        publicacion.Contenido = request.Contenido.Trim();
        publicacion.ImagenUrl = request.ImagenUrl;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var publicacion = await dbContext.PublicacionesForo.FindAsync([id], cancellationToken);
        if (publicacion is null)
        {
            return NotFound();
        }

        dbContext.PublicacionesForo.Remove(publicacion);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
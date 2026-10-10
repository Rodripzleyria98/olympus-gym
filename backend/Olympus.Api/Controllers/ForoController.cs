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
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<PublicacionDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var publicaciones = dbContext.PublicacionesForo.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var termino = search.Trim().ToLowerInvariant();
            publicaciones = publicaciones.Where(publicacion =>
                publicacion.Titulo.ToLower().Contains(termino) ||
                publicacion.Contenido.ToLower().Contains(termino) ||
                (publicacion.Autor.Nombre + " " + publicacion.Autor.Apellido).ToLower().Contains(termino));
        }

        var totalCount = await publicaciones.CountAsync(cancellationToken);
        var offset = (int)Math.Min(((long)page - 1) * pageSize, int.MaxValue);
        var items = await publicaciones
            .OrderByDescending(publicacion => publicacion.FechaPublicacion)
            .ThenByDescending(publicacion => publicacion.Id)
            .Skip(offset)
            .Take(pageSize)
            .Select(publicacion => new PublicacionDto(
                publicacion.Id,
                publicacion.Titulo,
                publicacion.Contenido,
                publicacion.ImagenUrl,
                publicacion.FechaPublicacion,
                publicacion.AutorId,
                publicacion.Autor.Nombre + " " + publicacion.Autor.Apellido))
            .ToListAsync(cancellationToken);

        return Ok(PagedResponse<PublicacionDto>.Create(items, page, pageSize, totalCount));
    }

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
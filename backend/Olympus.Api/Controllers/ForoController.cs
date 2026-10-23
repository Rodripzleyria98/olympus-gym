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
public class ForoController(OlympusDbContext dbContext, IWebHostEnvironment environment) : ControllerBase
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

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<PublicacionDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var publicacion = await dbContext.PublicacionesForo.AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new PublicacionDto(
                item.Id,
                item.Titulo,
                item.Contenido,
                item.ImagenUrl,
                item.FechaPublicacion,
                item.AutorId,
                item.Autor.Nombre + " " + item.Autor.Apellido))
            .FirstOrDefaultAsync(cancellationToken);
        return publicacion is null ? NotFound() : Ok(publicacion);
    }

    [HttpPost]
    [HttpPost("posts")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PublicacionDto>> Create(
        [FromForm] PublicacionRequestDto request,
        CancellationToken cancellationToken)
    {
        var imageError = ValidateImage(request.Imagen);
        if (imageError is not null) return BadRequest(new { message = imageError });
        var imageUrl = await SaveImageAsync(request.Imagen, cancellationToken);
        var publicacion = new PublicacionForo
        {
            Titulo = request.Titulo.Trim(),
            Contenido = request.Contenido.Trim(),
            ImagenUrl = imageUrl,
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
        return CreatedAtAction(nameof(GetById), new { id = publicacion.Id }, response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromForm] PublicacionRequestDto request,
        CancellationToken cancellationToken)
    {
        var publicacion = await dbContext.PublicacionesForo.FindAsync([id], cancellationToken);
        if (publicacion is null)
        {
            return NotFound();
        }

        var imageError = ValidateImage(request.Imagen);
        if (imageError is not null) return BadRequest(new { message = imageError });
        publicacion.Titulo = request.Titulo.Trim();
        publicacion.Contenido = request.Contenido.Trim();
        if (request.Imagen is not null)
            publicacion.ImagenUrl = await SaveImageAsync(request.Imagen, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string? ValidateImage(IFormFile? image)
    {
        if (image is null) return null;
        if (image.Length == 0) return "El archivo de imagen está vacío.";
        if (image.Length > 5 * 1024 * 1024)
            return "La imagen no puede superar los 5 MB.";

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedExtensions.Contains(extension) ||
            !allowedContentTypes.Contains(image.ContentType, StringComparer.OrdinalIgnoreCase))
            return "Formato de imagen no permitido. Usa JPG, PNG o WEBP.";

        return null;
    }

    private async Task<string?> SaveImageAsync(IFormFile? image, CancellationToken cancellationToken)
    {
        if (image is null) return null;

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        var folder = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "foro");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(folder, fileName);
        await using var stream = System.IO.File.Create(path);
        await image.CopyToAsync(stream, cancellationToken);
        return $"/uploads/foro/{fileName}";
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
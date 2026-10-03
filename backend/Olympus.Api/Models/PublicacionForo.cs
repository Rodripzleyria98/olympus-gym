namespace Olympus.Api.Models;

public class PublicacionForo
{
    public int Id { get; set; }
    public required string Titulo { get; set; }
    public required string Contenido { get; set; }
    public string? ImagenUrl { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public Guid AutorId { get; set; }

    public Usuario Autor { get; set; } = null!;
}
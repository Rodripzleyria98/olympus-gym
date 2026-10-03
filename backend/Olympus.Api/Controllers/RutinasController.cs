using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Olympus.Api.Data;
using Olympus.Api.Dtos;
using Olympus.Api.Models;

namespace Olympus.Api.Controllers;

[ApiController]
[Route("api")]
public class RutinasController(OlympusDbContext dbContext) : ControllerBase
{
    [HttpGet("rutinas/mi-rutina")]
    [Authorize(Policy = "UserPolicy")]
    public async Task<ActionResult<RutinaDto>> GetMiRutina(CancellationToken cancellationToken)
    {
        var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var rutina = await GetActiveRoutineAsync(usuarioId, cancellationToken);
        return rutina is null ? NotFound(new { message = "No tienes una rutina activa asignada." }) : Ok(rutina);
    }

    [HttpGet("admin/rutinas/usuario/{usuarioId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<RutinaDto>> GetRutinaUsuario(Guid usuarioId, CancellationToken cancellationToken)
    {
        var rutina = await GetActiveRoutineAsync(usuarioId, cancellationToken);
        return rutina is null ? NotFound(new { message = "El socio no tiene una rutina activa." }) : Ok(rutina);
    }

    [HttpGet("admin/rutinas/plantillas")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IReadOnlyList<PlantillaRutinaDto>>> GetPlantillas(CancellationToken cancellationToken)
    {
        var templateIds = RutinaSeedData.Planes.Select(plan => plan.Id).ToArray();
        var templates = await dbContext.PlanesEntrenamiento.AsNoTracking().AsSplitQuery()
            .Include(plan => plan.Dias)
            .ThenInclude(dia => dia.Ejercicios)
            .Where(plan => templateIds.Contains(plan.Id))
            .OrderBy(plan => plan.Titulo)
            .ToListAsync(cancellationToken);

        return Ok(templates
            .GroupBy(plan => plan.Titulo)
            .Select(group =>
            {
                var template = group.First();
                return new PlantillaRutinaDto(
                    template.Titulo,
                    template.FechaInicio == DateTime.MinValue ? null : template.FechaInicio,
                    template.FechaRevision,
                    MapRoutineDays(template.Dias));
            })
            .OrderBy(template => template.Titulo)
            .ToArray());
    }

    [HttpGet("rutinas/catalogo")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<RutinaCatalogoDto>>> GetCatalogo(CancellationToken cancellationToken)
    {
        var planIds = RutinaSeedData.Planes.Select(plan => plan.Id).ToArray();
        var planes = await dbContext.PlanesEntrenamiento.AsNoTracking().AsSplitQuery()
            .Include(plan => plan.Usuario)
            .Include(plan => plan.Dias)
            .ThenInclude(dia => dia.Ejercicios)
            .Where(plan => plan.IsActivo && planIds.Contains(plan.Id))
            .ToListAsync(cancellationToken);
        var usuarioId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
        var catalogo = planes
            .GroupBy(plan => plan.Titulo)
            .Select(grupo =>
            {
                var primeraAsignacion = grupo.First();
                var fechaInicio = grupo
                    .Select(plan => plan.FechaInicio == DateTime.MinValue ? (DateTime?)null : plan.FechaInicio)
                    .FirstOrDefault(fecha => fecha.HasValue);
                var fechaRevision = grupo
                    .Select(plan => plan.FechaRevision)
                    .FirstOrDefault(fecha => fecha.HasValue);
                var asignaciones = grupo
                    .Select(plan => new RutinaAsignacionDto(
                        plan.UsuarioId,
                        $"{plan.Usuario.Nombre} {plan.Usuario.Apellido}"))
                    .DistinctBy(asignacion => asignacion.UsuarioId)
                    .OrderBy(asignacion => asignacion.NombreSocio)
                    .ToArray();

                return new RutinaCatalogoDto(
                    grupo.Key,
                    asignaciones,
                    primeraAsignacion.Dias.Count,
                    grupo.Sum(plan => plan.Dias.Sum(dia => dia.Ejercicios.Count)) / grupo.Count(),
                    fechaInicio,
                    fechaRevision,
                    asignaciones.Any(asignacion => asignacion.UsuarioId == usuarioId));
            })
            .OrderBy(plan => plan.Titulo)
            .ToArray();

        return Ok(catalogo);
    }

    [HttpPost("admin/rutinas")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<RutinaDto>> CreateOrRenew(
        CrearRutinaRequestDto request,
        CancellationToken cancellationToken)
    {
        var usuario = await dbContext.Usuarios
            .FirstOrDefaultAsync(item => item.Id == request.UsuarioId && item.Rol == RolUsuario.User, cancellationToken);
        if (usuario is null)
        {
            return NotFound(new { message = "No se encontró el socio indicado." });
        }

        var nuevaRutina = await AssignRoutineAsync(
            usuario,
            request.Titulo,
            DateTime.UtcNow,
            request.FechaRevision,
            request.Dias,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetRutinaUsuario),
            new { usuarioId = usuario.Id },
            nuevaRutina);
    }

    [HttpPost("admin/rutinas/asignar")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<RutinaDto>> Asignar(
        AsignarRutinaDto request,
        CancellationToken cancellationToken)
    {
        var usuario = await dbContext.Usuarios
            .FirstOrDefaultAsync(item => item.Id == request.UsuarioId && item.Rol == RolUsuario.User, cancellationToken);
        if (usuario is null)
        {
            return NotFound(new { message = "No se encontró el socio indicado." });
        }

        var routine = await AssignRoutineAsync(
            usuario,
            request.Titulo,
            request.FechaInicio,
            request.FechaRevision,
            request.Dias.Select(day => new DiaRutinaRequestDto
            {
                NombreDia = day.NombreDia,
                Orden = day.Orden,
                Enfoque = day.Enfoque,
                Ejercicios = day.Ejercicios.Select(exercise => new EjercicioRutinaRequestDto
                {
                    Orden = exercise.Orden,
                    Nombre = exercise.Nombre,
                    SeriesYRepeticiones = exercise.SeriesYRepeticiones,
                    Notas = exercise.Notas
                }).ToList()
            }),
            cancellationToken);

        return Ok(routine);
    }

    [HttpPut("admin/rutinas/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<RutinaDto>> Update(
        Guid id,
        ActualizarRutinaRequestDto request,
        CancellationToken cancellationToken)
    {
        var rutina = await dbContext.PlanesEntrenamiento
            .AsSplitQuery()
            .Include(plan => plan.Dias)
            .ThenInclude(dia => dia.Ejercicios)
            .FirstOrDefaultAsync(plan => plan.Id == id, cancellationToken);
        if (rutina is null)
        {
            return NotFound();
        }

        if (request.IsActivo)
        {
            var rutinasActivas = await dbContext.PlanesEntrenamiento
                .Where(plan => plan.UsuarioId == rutina.UsuarioId && plan.IsActivo && plan.Id != rutina.Id)
                .ToListAsync(cancellationToken);
            foreach (var activa in rutinasActivas)
            {
                activa.IsActivo = false;
            }
        }

        rutina.Titulo = request.Titulo.Trim();
        rutina.FechaRevision = request.FechaRevision;
        rutina.IsActivo = request.IsActivo;
        dbContext.Ejercicios.RemoveRange(rutina.Dias.SelectMany(dia => dia.Ejercicios));
        dbContext.DiasEntrenamiento.RemoveRange(rutina.Dias);
        rutina.Dias = MapDayEntities(request.Dias);

        await dbContext.SaveChangesAsync(cancellationToken);
        var nombreSocio = await dbContext.Usuarios.AsNoTracking()
            .Where(usuario => usuario.Id == rutina.UsuarioId)
            .Select(usuario => usuario.Nombre + " " + usuario.Apellido)
            .SingleAsync(cancellationToken);
        return Ok(MapRoutine(rutina, nombreSocio));
    }

    private async Task<RutinaDto?> GetActiveRoutineAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var rutina = await dbContext.PlanesEntrenamiento.AsNoTracking().AsSplitQuery()
            .Include(plan => plan.Usuario)
            .Include(plan => plan.Dias)
            .ThenInclude(dia => dia.Ejercicios)
            .Where(plan => plan.UsuarioId == usuarioId && plan.IsActivo)
            .OrderByDescending(plan => plan.FechaInicio)
            .FirstOrDefaultAsync(cancellationToken);

        return rutina is null ? null : MapRoutine(rutina, $"{rutina.Usuario.Nombre} {rutina.Usuario.Apellido}");
    }

    private async Task<RutinaDto> AssignRoutineAsync(
        Usuario usuario,
        string titulo,
        DateTime fechaInicio,
        DateTime? fechaRevision,
        IEnumerable<DiaRutinaRequestDto> dias,
        CancellationToken cancellationToken)
    {
        var rutinasAnteriores = await dbContext.PlanesEntrenamiento
            .Where(plan => plan.UsuarioId == usuario.Id && plan.IsActivo)
            .ToListAsync(cancellationToken);
        foreach (var anterior in rutinasAnteriores)
        {
            anterior.IsActivo = false;
        }

        var rutina = new PlanEntrenamiento
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            Titulo = titulo.Trim(),
            FechaInicio = fechaInicio == DateTime.MinValue ? DateTime.UtcNow : fechaInicio,
            FechaRevision = fechaRevision,
            IsActivo = true,
            Dias = MapDayEntities(dias)
        };
        dbContext.PlanesEntrenamiento.Add(rutina);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapRoutine(rutina, $"{usuario.Nombre} {usuario.Apellido}");
    }

    private static ICollection<DiaEntrenamiento> MapDayEntities(IEnumerable<DiaRutinaRequestDto> dias) =>
        dias.Select(dia => new DiaEntrenamiento
        {
            Id = Guid.NewGuid(),
            NombreDia = dia.NombreDia.Trim(),
            Orden = dia.Orden,
            Enfoque = dia.Enfoque.Trim(),
            Ejercicios = dia.Ejercicios.Select(ejercicio => new EjercicioItem
            {
                Id = Guid.NewGuid(),
                Orden = ejercicio.Orden,
                Nombre = ejercicio.Nombre.Trim(),
                SeriesYRepeticiones = ejercicio.SeriesYRepeticiones.Trim(),
                Notas = string.IsNullOrWhiteSpace(ejercicio.Notas) ? null : ejercicio.Notas.Trim()
            }).ToList()
        }).ToList();

    private static IReadOnlyList<DiaRutinaDto> MapRoutineDays(IEnumerable<DiaEntrenamiento> dias) =>
        dias.OrderBy(dia => dia.Orden)
            .Select(dia => new DiaRutinaDto(
                dia.Id,
                dia.NombreDia,
                dia.Orden,
                dia.Enfoque,
                dia.Ejercicios.OrderBy(ejercicio => ejercicio.Orden)
                    .Select(ejercicio => new EjercicioRutinaDto(
                        ejercicio.Id,
                        ejercicio.Orden,
                        ejercicio.Nombre,
                        ejercicio.SeriesYRepeticiones,
                        ejercicio.Notas))
                    .ToArray()))
            .ToArray();

    private static RutinaDto MapRoutine(PlanEntrenamiento rutina, string nombreSocio) => new(
        rutina.Id,
        rutina.UsuarioId,
        nombreSocio,
        rutina.Titulo,
        rutina.FechaInicio == DateTime.MinValue ? null : rutina.FechaInicio,
        rutina.FechaRevision,
        rutina.IsActivo,
        MapRoutineDays(rutina.Dias));
}
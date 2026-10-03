using Microsoft.EntityFrameworkCore;
using Olympus.Api.Data;
using Olympus.Api.Dtos;
using Olympus.Api.Models;

namespace Olympus.Api.Services;

public class MembresiaService(OlympusDbContext dbContext) : IMembresiaService
{
    public async Task<IReadOnlyList<PlanMembresiaDto>> GetPlanesAsync(CancellationToken cancellationToken) =>
        await dbContext.PlanesMembresia.AsNoTracking()
            .Where(plan => plan.IsActivo)
            .OrderBy(plan => plan.Id)
            .Select(plan => new PlanMembresiaDto(
                plan.Id, plan.Nombre, plan.Descripcion, plan.Precio, plan.DuracionDias, plan.ClasesIncluidas))
            .ToListAsync(cancellationToken);

    public async Task<PlanMembresiaDto?> UpdatePlanAsync(
        int planId,
        EditarPlanMembresiaRequestDto request,
        CancellationToken cancellationToken)
    {
        var plan = await dbContext.PlanesMembresia
            .FirstOrDefaultAsync(item => item.Id == planId, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        plan.Nombre = request.Nombre.Trim();
        plan.Descripcion = request.Descripcion.Trim();
        plan.Precio = request.Precio;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PlanMembresiaDto(
            plan.Id, plan.Nombre, plan.Descripcion, plan.Precio, plan.DuracionDias, plan.ClasesIncluidas);
    }

    public async Task<PagoCheckoutResponseDto?> CreateApprovedCheckoutAsync(
        Guid usuarioId,
        int planId,
        string metodoPago,
        CancellationToken cancellationToken)
    {
        var usuario = await dbContext.Usuarios
            .FirstOrDefaultAsync(item => item.Id == usuarioId && item.Rol == RolUsuario.User, cancellationToken);
        var plan = await dbContext.PlanesMembresia
            .FirstOrDefaultAsync(item => item.Id == planId && item.IsActivo, cancellationToken);
        if (usuario is null || plan is null)
        {
            return null;
        }

        var ahora = DateTime.UtcNow;
        var membresiaActivaHasta = await dbContext.MembresiasUsuario
            .Where(item => item.UsuarioId == usuarioId &&
                item.Estado == EstadoMembresia.Vigente && item.FechaFin >= ahora)
            .Select(item => (DateTime?)item.FechaFin)
            .MaxAsync(cancellationToken);
        var fechaInicio = membresiaActivaHasta is { } fin && fin > ahora ? fin : ahora;
        var membresia = new MembresiaUsuario
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            PlanMembresiaId = plan.Id,
            FechaInicio = fechaInicio,
            FechaFin = fechaInicio.AddDays(plan.DuracionDias),
            Estado = EstadoMembresia.Vigente
        };
        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            MembresiaUsuarioId = membresia.Id,
            Monto = plan.Precio,
            FechaPago = ahora,
            MetodoPago = metodoPago,
            EstadoPago = EstadoPago.Aprobado
        };

        dbContext.MembresiasUsuario.Add(membresia);
        dbContext.Pagos.Add(pago);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagoCheckoutResponseDto(
            pago.Id,
            membresia.Id,
            usuario.Id,
            plan.Nombre,
            pago.Monto,
            pago.MetodoPago,
            pago.FechaPago,
            membresia.FechaInicio,
            membresia.FechaFin,
            pago.EstadoPago.ToString());
    }

    public async Task<PerfilResponseDto?> GetPerfilAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await dbContext.Usuarios.AsNoTracking()
            .Where(item => item.Id == usuarioId)
            .Select(item => new UsuarioDto(item.Id, item.Nombre, item.Apellido, item.Email, item.Rol.ToString()))
            .FirstOrDefaultAsync(cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        var ahora = DateTime.UtcNow;
        var membresia = await dbContext.MembresiasUsuario.AsNoTracking()
            .Where(item => item.UsuarioId == usuarioId)
            .OrderByDescending(item => item.Estado == EstadoMembresia.Vigente && item.FechaFin >= ahora)
            .ThenByDescending(item => item.FechaInicio)
            .Select(item => new
            {
                item.Estado,
                item.FechaInicio,
                item.FechaFin,
                Plan = item.PlanMembresia.Nombre,
                Pago = item.Pagos.OrderByDescending(pago => pago.FechaPago)
                    .Select(pago => new { pago.EstadoPago, pago.Monto })
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var activo = membresia is not null &&
            membresia.Estado == EstadoMembresia.Vigente && membresia.FechaFin >= ahora;
        var diasRestantes = activo && membresia is not null
            ? Math.Max(0, (int)Math.Ceiling((membresia.FechaFin - ahora).TotalDays))
            : 0;

        return new PerfilResponseDto(
            usuario,
            activo,
            membresia?.Estado.ToString() ?? "SinMembresia",
            membresia?.Plan,
            membresia?.FechaInicio,
            membresia?.FechaFin,
            diasRestantes,
            membresia?.Pago?.EstadoPago.ToString(),
            membresia?.Pago?.Monto);
    }
}
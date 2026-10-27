using Microsoft.EntityFrameworkCore;
using Olympus.Api.Data;
using Olympus.Api.Dtos;
using Olympus.Api.Models;

namespace Olympus.Api.Services;

public class UsuarioService(OlympusDbContext dbContext) : IUsuarioService
{
    public async Task<PagedResponse<UsuarioAdminDto>> GetUsuariosAsync(
        string? buscar,
        string? estado,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var ahora = DateTime.UtcNow;
        var usuarios = dbContext.Usuarios.AsNoTracking()
            .Where(usuario => usuario.Rol == RolUsuario.User || usuario.Rol == RolUsuario.Socio);

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim().ToLower();
            usuarios = usuarios.Where(usuario =>
                (usuario.Nombre + " " + usuario.Apellido).ToLower().Contains(termino) ||
                usuario.Email.ToLower().Contains(termino) ||
                usuario.Dni.Contains(termino));
        }

        if (string.Equals(estado, "activo", StringComparison.OrdinalIgnoreCase))
        {
            usuarios = usuarios.Where(usuario => usuario.Membresias.Any(membresia =>
                membresia.Estado == EstadoMembresia.Vigente &&
                membresia.FechaFin >= ahora));
        }
        else if (string.Equals(estado, "inactivo", StringComparison.OrdinalIgnoreCase))
        {
            usuarios = usuarios.Where(usuario => !usuario.Membresias.Any(membresia =>
                membresia.Estado == EstadoMembresia.Vigente &&
                membresia.FechaFin >= ahora));
        }

        var totalCount = await usuarios.CountAsync(cancellationToken);
        var offset = (int)Math.Min(((long)page - 1) * pageSize, int.MaxValue);
        var resultados = await usuarios
            .OrderBy(usuario => usuario.Nombre)
            .ThenBy(usuario => usuario.Apellido)
            .Skip(offset)
            .Take(pageSize)
            .Select(usuario => new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Apellido,
                usuario.Email,
                usuario.Dni,
                usuario.FechaRegistro,
                IsActivo = usuario.Membresias.Any(membresia =>
                    membresia.Estado == EstadoMembresia.Vigente && membresia.FechaFin >= ahora),
                FechaVencimiento = usuario.Membresias
                    .OrderByDescending(membresia => membresia.FechaFin)
                    .Select(membresia => (DateTime?)membresia.FechaFin)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var items = resultados.Select(usuario => new UsuarioAdminDto(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email,
            usuario.Dni,
            usuario.FechaRegistro,
            usuario.IsActivo,
            usuario.IsActivo,
            usuario.FechaVencimiento,
            usuario.FechaVencimiento is { } fin ? Math.Max(0, (int)Math.Ceiling((fin - ahora).TotalDays)) : 0))
            .ToArray();

        return PagedResponse<UsuarioAdminDto>.Create(items, page, pageSize, totalCount);
    }

    public async Task<HistorialUsuarioDto?> GetHistorialAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await dbContext.Usuarios.AsNoTracking()
            .Where(item => item.Id == usuarioId)
            .Select(item => new UsuarioDto(item.Id, item.Nombre, item.Apellido, item.Email, item.Dni, item.Rol.ToString()))
            .FirstOrDefaultAsync(cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        var membresias = await dbContext.MembresiasUsuario.AsNoTracking()
            .Where(item => item.UsuarioId == usuarioId)
            .OrderByDescending(item => item.FechaInicio)
            .Select(item => new MembresiaHistorialDto(
                item.Id,
                item.PlanMembresia.Nombre,
                item.FechaInicio,
                item.FechaFin,
                item.Estado.ToString(),
                item.Pagos.Select(pago => new PagoDto(
                    pago.Id,
                    pago.Monto,
                    pago.FechaCreacion,
                    pago.MetodoPago,
                    pago.ComprobanteUrl,
                    pago.EstadoPago.ToString())).ToArray()))
            .ToListAsync(cancellationToken);

        var pagos = await dbContext.Pagos.AsNoTracking()
            .Where(pago => pago.UsuarioId == usuarioId)
            .OrderByDescending(pago => pago.FechaCreacion)
            .Select(pago => new PagoDto(
                pago.Id,
                pago.Monto,
                pago.FechaCreacion,
                pago.MetodoPago,
                pago.ComprobanteUrl,
                pago.EstadoPago.ToString()))
            .ToListAsync(cancellationToken);

        return new HistorialUsuarioDto(usuario, membresias, pagos);
    }

    public async Task<IReadOnlyList<PagoPendienteAdminDto>> GetPagosPendientesAsync(
        CancellationToken cancellationToken) =>
        await dbContext.Pagos.AsNoTracking()
            .Where(pago => pago.EstadoPago == EstadoPago.Pendiente &&
                (pago.Usuario.Rol == RolUsuario.Socio || pago.Usuario.Rol == RolUsuario.User))
            .OrderBy(pago => pago.FechaCreacion)
            .Select(pago => new PagoPendienteAdminDto(
                pago.Id,
                pago.UsuarioId,
                pago.Usuario.Nombre + " " + pago.Usuario.Apellido,
                pago.Usuario.Dni,
                pago.MembresiaUsuario.PlanMembresia.Nombre,
                pago.FechaCreacion,
                pago.Monto,
                pago.MetodoPago,
                pago.CajaTurnoId,
                pago.CajaTurno == null ? "Sin caja asignada" : pago.CajaTurno.Turno))
            .ToListAsync(cancellationToken);

    public async Task<bool> ValidarPagoPendienteAsync(Guid pagoId, CancellationToken cancellationToken)
    {
        var pago = await dbContext.Pagos
            .Include(item => item.MembresiaUsuario)
            .ThenInclude(item => item.PlanMembresia)
            .FirstOrDefaultAsync(item => item.Id == pagoId, cancellationToken);
        if (pago is null || pago.EstadoPago != EstadoPago.Pendiente ||
            !string.Equals(pago.MetodoPago, "Transferencia", StringComparison.OrdinalIgnoreCase))
            return false;

        var ahora = DateTime.UtcNow;

        if (pago.CajaTurnoId is null)
        {
            pago.CajaTurnoId = await dbContext.CajasTurno
                .Where(caja => caja.Estado == "Abierta")
                .OrderByDescending(caja => caja.FechaHoraApertura.HasValue)
                .ThenByDescending(caja => caja.FechaHoraApertura).ThenByDescending(caja => caja.Id)
                .Select(caja => (int?)caja.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        pago.EstadoPago = EstadoPago.Aprobado;
        pago.FechaAcreditacion = ahora;
        pago.MembresiaUsuario.FechaFin = pago.MembresiaUsuario.FechaInicio
            .AddDays(pago.MembresiaUsuario.PlanMembresia.DuracionDias);
        pago.MembresiaUsuario.Estado = EstadoMembresia.Vigente;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> CambiarEstadoAsync(
        Guid usuarioId,
        CambiarEstadoUsuarioRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.PagoId is null && (request.MembresiaUsuarioId is null || request.EstadoMembresia is null))
        {
            throw new ArgumentException("Indica el pago que se validará o la membresía y su nuevo estado.");
        }

        if (request.PagoId is not null)
        {
            if (request.EstadoPago is null)
            {
                throw new ArgumentException("Indica el estado que tendrá el pago.");
            }

            var pago = await dbContext.Pagos
                .Include(item => item.MembresiaUsuario)
                .ThenInclude(item => item.PlanMembresia)
                .FirstOrDefaultAsync(item => item.Id == request.PagoId && item.UsuarioId == usuarioId, cancellationToken);

            if (pago is null)
            {
                return false;
            }

            pago.EstadoPago = request.EstadoPago.Value;
            if (pago.EstadoPago == EstadoPago.Aprobado)
            {
                var membresia = pago.MembresiaUsuario;
                membresia.Estado = EstadoMembresia.Vigente;
                membresia.FechaFin = membresia.FechaInicio.AddDays(membresia.PlanMembresia.DuracionDias);
            }
        }

        if (request.MembresiaUsuarioId is not null && request.EstadoMembresia is not null)
        {
            var membresia = await dbContext.MembresiasUsuario
                .Include(item => item.PlanMembresia)
                .FirstOrDefaultAsync(item =>
                    item.Id == request.MembresiaUsuarioId && item.UsuarioId == usuarioId, cancellationToken);

            if (membresia is null)
            {
                return false;
            }

            membresia.Estado = request.EstadoMembresia.Value;
            if (membresia.Estado == EstadoMembresia.Vigente && membresia.FechaFin < DateTime.UtcNow)
            {
                membresia.FechaInicio = DateTime.UtcNow;
                membresia.FechaFin = membresia.FechaInicio.AddDays(membresia.PlanMembresia.DuracionDias);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
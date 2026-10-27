using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Olympus.Api.Data;
using Olympus.Api.Dtos;
using Olympus.Api.Models;
using Olympus.Api.Services;
using Npgsql;

namespace Olympus.Api.Controllers;

[ApiController]
[Route("api/caja")]
[Authorize(Roles = "Admin")]
public class CajaController(OlympusDbContext dbContext) : ControllerBase
{
    [HttpPost("abrir")]
    public async Task<ActionResult<CajaTurnoDto>> Abrir(
        AbrirCajaRequestDto request,
        CancellationToken cancellationToken)
    {
        var (fecha, _) = CajaTurnoHelper.TurnoActual();
        var responsable = NormalizarResponsable(request.ResponsableTurno);
        if (string.IsNullOrWhiteSpace(responsable))
            return BadRequest(new { message = "Indica el nombre de la persona responsable del turno." });

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(20261027)",
            cancellationToken);
        if (await dbContext.CajasTurno.AnyAsync(caja =>
                caja.Estado == "Abierta",
                cancellationToken))
            return Conflict(new { message = "Ya hay una caja abierta. Cierra el turno actual antes de abrir otra." });
        var caja = new CajaTurno
        {
            Fecha = fecha,
            Turno = request.Turno,
            ResponsableTurno = request.ResponsableTurno.Trim(),
            ResponsableTurnoNormalizado = responsable,
            UsuarioAperturaId = UsuarioActualId(),
            FechaHoraApertura = DateTime.UtcNow,
            MontoInicial = request.MontoInicial,
            Estado = "Abierta"
        };
        dbContext.CajasTurno.Add(caja);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new { message = "Ya hay una caja abierta. Cierra el turno actual antes de abrir otra." });
        }

        return CreatedAtAction(nameof(Actual), CajaDto(caja));
    }

    [HttpPost("cerrar")]
    public async Task<ActionResult<CajaTurnoDto>> Cerrar(
        CerrarCajaRequestDto request,
        CancellationToken cancellationToken)
    {
        var caja = await dbContext.CajasTurno.OrderByDescending(item => item.FechaHoraApertura.HasValue)
            .ThenByDescending(item => item.FechaHoraApertura).ThenByDescending(item => item.Id)
            .FirstOrDefaultAsync(item => item.Estado == "Abierta", cancellationToken);
        if (caja is null) return Conflict(new { message = "No hay una caja abierta." });

        var totalesTurno = await ObtenerTotalesAsync([caja.Id], caja.Fecha, cancellationToken);
        var esperado = caja.MontoInicial + totalesTurno.Efectivo;
        var fechaHoraCierre = DateTime.UtcNow;
        caja.Observaciones = request.Observaciones?.Trim();
        caja.UsuarioCierreId = UsuarioActualId();
        caja.FechaHoraCierre = fechaHoraCierre;
        caja.TotalCobrosEfectivo = totalesTurno.Efectivo;
        caja.TotalCobrosDigitales = totalesTurno.Transferencias + totalesTurno.Otros;
        caja.MontoFinalEfectivoEsperado = esperado;
        caja.MontoFinalEfectivoReal = request.MontoFinalEfectivoReal;
        caja.Diferencia = request.MontoFinalEfectivoReal is { } real ? real - esperado : null;
        caja.Estado = "Cerrada";
        var actualizados = await dbContext.CajasTurno.Where(item => item.Id == caja.Id && item.Estado == "Abierta")
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Observaciones, caja.Observaciones)
                .SetProperty(item => item.UsuarioCierreId, caja.UsuarioCierreId)
                .SetProperty(item => item.FechaHoraCierre, caja.FechaHoraCierre)
                .SetProperty(item => item.TotalCobrosEfectivo, caja.TotalCobrosEfectivo)
                .SetProperty(item => item.TotalCobrosDigitales, caja.TotalCobrosDigitales)
                .SetProperty(item => item.MontoFinalEfectivoEsperado, caja.MontoFinalEfectivoEsperado)
                .SetProperty(item => item.MontoFinalEfectivoReal, caja.MontoFinalEfectivoReal)
                .SetProperty(item => item.Diferencia, caja.Diferencia)
                .SetProperty(item => item.Estado, caja.Estado), cancellationToken);
        if (actualizados == 0) return Conflict(new { message = "La caja ya fue cerrada por otra operación." });
        return Ok(CajaDto(caja));
    }

    [HttpGet("actual")]
    public async Task<ActionResult<CajaActualDto>> Actual(CancellationToken cancellationToken)
    {
        var (fechaActual, turnoActual) = CajaTurnoHelper.TurnoActual();
        var caja = await dbContext.CajasTurno.AsNoTracking()
            .OrderByDescending(item => item.FechaHoraApertura.HasValue)
            .ThenByDescending(item => item.FechaHoraApertura).ThenByDescending(item => item.Id)
            .FirstOrDefaultAsync(item => item.Estado == "Abierta", cancellationToken);
        var detalle = caja is null ? null : await ObtenerDetalleAsync(caja, cancellationToken);
        return Ok(new CajaActualDto(caja?.Fecha ?? fechaActual, caja?.Turno ?? turnoActual, detalle));
    }

    [HttpGet("{id:int}/detalle")]
    public async Task<ActionResult<CajaDetalleDto>> ObtenerDetalle(
        int id,
        CancellationToken cancellationToken)
    {
        var caja = await dbContext.CajasTurno.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return caja is null ? NotFound() : Ok(await ObtenerDetalleAsync(caja, cancellationToken));
    }

    [HttpGet("historial")]
    public async Task<ActionResult<IReadOnlyList<CajaTurnoDto>>> Historial(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
    {
        var query = dbContext.CajasTurno.AsNoTracking().Where(caja => caja.Estado == "Cerrada");
        if (desde.HasValue) query = query.Where(caja => caja.Fecha >= desde.Value.Date);
        if (hasta.HasValue) query = query.Where(caja => caja.Fecha < hasta.Value.Date.AddDays(1));
        var cajas = await query.OrderByDescending(caja => caja.Fecha).ThenByDescending(caja => caja.Id)
            .Select(caja => new CajaTurnoDto(
                caja.Id, caja.Fecha, caja.Turno, caja.ResponsableTurno, caja.UsuarioAperturaId, caja.UsuarioCierreId,
                caja.FechaHoraApertura, caja.FechaHoraCierre, caja.MontoInicial, caja.MontoFinalEfectivoEsperado,
                caja.MontoFinalEfectivoReal, caja.Diferencia, caja.TotalCobrosEfectivo, caja.TotalCobrosDigitales,
                caja.Observaciones, caja.Estado))
            .ToListAsync(cancellationToken);
        return Ok(cajas);
    }

    private async Task<CajaDetalleDto> ObtenerDetalleAsync(CajaTurno caja, CancellationToken cancellationToken)
    {
        var cobrosMembresia = await dbContext.Pagos.AsNoTracking()
            .Where(pago => pago.CajaTurnoId == caja.Id)
            .OrderByDescending(pago => pago.FechaAcreditacion ?? pago.FechaCreacion)
            .Select(pago => new CajaPagoMembresiaDto(
                pago.Id,
                pago.Usuario.Nombre + " " + pago.Usuario.Apellido,
                pago.MembresiaUsuario.PlanMembresia.Nombre,
                pago.FechaAcreditacion ?? pago.FechaCreacion,
                pago.Monto,
                pago.MetodoPago,
                pago.EstadoPago.ToString()))
            .ToListAsync(cancellationToken);
        var ventas = await dbContext.VentasProducto.AsNoTracking()
            .Where(venta => venta.CajaTurnoId == caja.Id)
            .OrderByDescending(venta => venta.Fecha)
            .Select(venta => new CajaVentaDto(
                venta.Id,
                venta.Fecha,
                venta.MontoTotal,
                venta.MetodoPago,
                venta.Items.Select(item => new CajaVentaItemDto(
                    item.Producto.Nombre, item.Cantidad, item.PrecioUnitario)).ToArray()))
            .ToListAsync(cancellationToken);
        var pases = await dbContext.PasesDiarios.AsNoTracking()
            .Where(pase => pase.CajaTurnoId == caja.Id)
            .OrderByDescending(pase => pase.Fecha)
            .Select(pase => new CajaPaseDto(
                pase.Id, pase.NombreCompleto, pase.Dni, pase.Fecha, pase.Monto, pase.MetodoPago))
            .ToListAsync(cancellationToken);
        var cajasDia = await dbContext.CajasTurno.AsNoTracking()
            .Where(item => item.Fecha == caja.Fecha)
            .Select(item => item.Id)
            .ToArrayAsync(cancellationToken);
        var totalesTurno = await ObtenerTotalesAsync([caja.Id], caja.Fecha, cancellationToken);
        var totalesDia = await ObtenerTotalesAsync(cajasDia, caja.Fecha, cancellationToken);
        return new CajaDetalleDto(CajaDto(caja), cobrosMembresia, ventas, pases, totalesTurno, totalesDia);
    }

    private async Task<CajaDiaTotalesDto> ObtenerTotalesAsync(
        int[] cajaIds,
        DateTime fecha,
        CancellationToken cancellationToken)
    {
        if (cajaIds.Length == 0) return new CajaDiaTotalesDto(fecha, 0, 0, 0, 0);

        var pagos = await dbContext.Pagos.AsNoTracking()
            .Where(pago => pago.CajaTurnoId.HasValue && cajaIds.Contains(pago.CajaTurnoId.Value) &&
                pago.EstadoPago == EstadoPago.Aprobado)
            .Select(pago => new { pago.MetodoPago, pago.Monto })
            .ToListAsync(cancellationToken);
        var ventas = await dbContext.VentasProducto.AsNoTracking()
            .Where(venta => venta.CajaTurnoId.HasValue && cajaIds.Contains(venta.CajaTurnoId.Value))
            .Select(venta => new { venta.MetodoPago, Monto = venta.MontoTotal })
            .ToListAsync(cancellationToken);
        var pases = await dbContext.PasesDiarios.AsNoTracking()
            .Where(pase => pase.CajaTurnoId.HasValue && cajaIds.Contains(pase.CajaTurnoId.Value))
            .Select(pase => new { pase.MetodoPago, Monto = pase.Monto })
            .ToListAsync(cancellationToken);

        var ingresos = pagos.Concat(ventas).Concat(pases).ToArray();
        var efectivo = ingresos.Where(ingreso => ingreso.MetodoPago == "Efectivo").Sum(ingreso => ingreso.Monto);
        var transferencias = ingresos.Where(ingreso => ingreso.MetodoPago == "Transferencia").Sum(ingreso => ingreso.Monto);
        var otros = ingresos.Where(ingreso => ingreso.MetodoPago != "Efectivo" && ingreso.MetodoPago != "Transferencia")
            .Sum(ingreso => ingreso.Monto);
        return new CajaDiaTotalesDto(fecha, efectivo, transferencias, otros, efectivo + transferencias + otros);
    }

    internal static string NormalizarResponsable(string responsable) =>
        string.Join(' ', responsable.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();

    internal static CajaTurnoDto CajaDto(CajaTurno caja) => new(
        caja.Id, caja.Fecha, caja.Turno, caja.ResponsableTurno, caja.UsuarioAperturaId, caja.UsuarioCierreId,
        caja.FechaHoraApertura, caja.FechaHoraCierre, caja.MontoInicial, caja.MontoFinalEfectivoEsperado,
        caja.MontoFinalEfectivoReal, caja.Diferencia, caja.TotalCobrosEfectivo, caja.TotalCobrosDigitales,
        caja.Observaciones, caja.Estado);

    internal Guid UsuarioActualId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[ApiController]
[Route("api/productos")]
[Authorize]
public class ProductosController(OlympusDbContext dbContext) : ControllerBase
{
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Profesor")]
    public async Task<ActionResult<ProductoDto>> Obtener(int id, CancellationToken cancellationToken)
    {
        var producto = await dbContext.Productos.AsNoTracking()
            .Where(item => item.Id == id && item.IsActivo)
            .Select(item => new ProductoDto(item.Id, item.Nombre, item.Sabor, item.Descripcion, item.Categoria,
                item.StockActual, item.StockMinimo, item.Precio, item.Codigo, item.IsActivo))
            .FirstOrDefaultAsync(cancellationToken);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Profesor")]
    public async Task<ActionResult<IReadOnlyList<ProductoDto>>> Catalogo(CancellationToken cancellationToken) =>
        Ok(await dbContext.Productos.AsNoTracking().OrderBy(item => item.Nombre)
            .Where(item => item.IsActivo)
            .Select(item => new ProductoDto(item.Id, item.Nombre, item.Sabor, item.Descripcion, item.Categoria,
                item.StockActual, item.StockMinimo, item.Precio, item.Codigo, item.IsActivo))
            .ToListAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoDto>> Crear(
        CrearProductoRequestDto request,
        CancellationToken cancellationToken)
    {
        var producto = new Producto
        {
            Nombre = request.Nombre.Trim(),
            Sabor = string.IsNullOrWhiteSpace(request.Sabor) ? null : request.Sabor.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(),
            Categoria = request.Categoria.Trim(),
            StockActual = request.StockActual,
            StockMinimo = request.StockMinimo,
            Precio = request.Precio,
            Codigo = string.IsNullOrWhiteSpace(request.Codigo) ? null : request.Codigo.Trim(),
            IsActivo = true
        };
        if (producto.Codigo is not null &&
            await dbContext.Productos.AnyAsync(item => item.Codigo == producto.Codigo, cancellationToken))
            return Conflict(new { message = "El código de producto ya está registrado." });
        dbContext.Productos.Add(producto);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new { message = "El código de producto ya está registrado." });
        }
        return CreatedAtAction(nameof(Obtener), new { id = producto.Id }, new ProductoDto(
            producto.Id, producto.Nombre, producto.Sabor, producto.Descripcion, producto.Categoria,
            producto.StockActual, producto.StockMinimo, producto.Precio, producto.Codigo, producto.IsActivo));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoDto>> Actualizar(
        int id,
        ActualizarProductoRequestDto request,
        CancellationToken cancellationToken)
    {
        var producto = await dbContext.Productos.FirstOrDefaultAsync(item => item.Id == id && item.IsActivo, cancellationToken);
        if (producto is null) return NotFound();
        var codigo = string.IsNullOrWhiteSpace(request.Codigo) ? null : request.Codigo.Trim();
        if (codigo is not null && await dbContext.Productos.AnyAsync(item =>
                item.Id != id && item.Codigo == codigo, cancellationToken))
            return Conflict(new { message = "El código de producto ya está registrado." });

        producto.Nombre = request.Nombre.Trim();
        producto.Sabor = string.IsNullOrWhiteSpace(request.Sabor) ? null : request.Sabor.Trim();
        producto.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        producto.Categoria = request.Categoria.Trim();
        producto.StockActual = request.StockActual;
        producto.StockMinimo = request.StockMinimo;
        producto.Precio = request.Precio;
        producto.Codigo = codigo;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new { message = "El código de producto ya está registrado." });
        }

        return Ok(new ProductoDto(producto.Id, producto.Nombre, producto.Sabor, producto.Descripcion,
            producto.Categoria, producto.StockActual, producto.StockMinimo, producto.Precio, producto.Codigo,
            producto.IsActivo));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Desactivar(int id, CancellationToken cancellationToken)
    {
        var producto = await dbContext.Productos.FirstOrDefaultAsync(item => item.Id == id && item.IsActivo, cancellationToken);
        if (producto is null) return NotFound();
        producto.IsActivo = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:int}/stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AjustarStock(
        int id,
        AjustarStockRequestDto request,
        CancellationToken cancellationToken)
    {
        var affected = await dbContext.Productos.Where(item => item.Id == id && item.IsActivo)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.StockActual, request.StockActual), cancellationToken);
        return affected == 0 ? NotFound() : NoContent();
    }

    [HttpPost("ventas")]
    [Authorize(Roles = "Admin,Profesor")]
    public async Task<ActionResult<VentaProductoResponseDto>> Vender(
        VentaProductoRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.MetodoPago is not ("Efectivo" or "MercadoPago" or "Transferencia"))
            return BadRequest(new { message = "Método de pago no válido." });
        var caja = await ObtenerCajaAbiertaAsync(cancellationToken);
        if (caja is null) return Conflict(new { message = "Debe haber una caja abierta para registrar ventas." });

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var cantidades = request.Items.GroupBy(item => item.ProductoId)
            .Select(group => new { ProductoId = group.Key, Cantidad = group.Sum(item => item.Cantidad) })
            .ToArray();
        var productos = new List<(Producto Producto, int Cantidad)>();
        foreach (var item in cantidades)
        {
            var updated = await dbContext.Productos.Where(producto =>
                    producto.Id == item.ProductoId && producto.IsActivo && producto.StockActual >= item.Cantidad)
                .ExecuteUpdateAsync(setters => setters.SetProperty(
                    producto => producto.StockActual, producto => producto.StockActual - item.Cantidad), cancellationToken);
            if (updated == 0) return Conflict(new { message = $"Stock insuficiente o producto {item.ProductoId} inexistente." });
            var producto = await dbContext.Productos.AsNoTracking()
                .FirstAsync(producto => producto.Id == item.ProductoId, cancellationToken);
            productos.Add((producto, item.Cantidad));
        }

        var venta = new VentaProducto
        {
            Fecha = DateTime.UtcNow,
            UsuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            CajaTurnoId = caja.Id,
            MetodoPago = request.MetodoPago,
            MontoTotal = productos.Sum(item => item.Producto.Precio * item.Cantidad),
            Items = productos.Select(item => new VentaProductoItem
            {
                ProductoId = item.Producto.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.Producto.Precio
            }).ToList()
        };
        dbContext.VentasProducto.Add(venta);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Ok(new VentaProductoResponseDto(
            venta.Id, venta.MontoTotal, venta.MetodoPago,
            productos.Select(item => new ItemVentaResponseDto(
                item.Producto.Id, item.Producto.Nombre, item.Cantidad, item.Producto.Precio)).ToArray()));
    }

    private async Task<CajaTurno?> ObtenerCajaAbiertaAsync(CancellationToken cancellationToken)
    {
        return await dbContext.CajasTurno.OrderByDescending(caja => caja.FechaHoraApertura.HasValue)
            .ThenByDescending(caja => caja.FechaHoraApertura).ThenByDescending(caja => caja.Id)
            .FirstOrDefaultAsync(caja => caja.Estado == "Abierta", cancellationToken);
    }
}

[ApiController]
[Route("api/pases-diarios")]
[Authorize(Roles = "Admin,Profesor")]
public class PasesDiariosController(OlympusDbContext dbContext) : ControllerBase
{
    private const decimal MontoPase = 5000m;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaseDiarioResponseDto>> Obtener(
        int id,
        CancellationToken cancellationToken)
    {
        var pase = await dbContext.PasesDiarios.AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new PaseDiarioResponseDto(
                item.Id, item.NombreCompleto, item.Dni, item.Fecha, item.Monto, item.MetodoPago))
            .FirstOrDefaultAsync(cancellationToken);
        return pase is null ? NotFound() : Ok(pase);
    }

    [HttpPost]
    public async Task<ActionResult<PaseDiarioResponseDto>> Crear(
        CrearPaseDiarioRequestDto request,
        CancellationToken cancellationToken)
    {
        var dni = request.Dni.Trim();
        var hoy = DateTime.UtcNow.Date;
        if (await dbContext.PasesDiarios.AnyAsync(pase => pase.Dni == dni && pase.Fecha == hoy, cancellationToken))
            return Conflict(new { message = "Ya existe un pase diario para este DNI en el día de hoy." });

        var caja = await dbContext.CajasTurno.OrderByDescending(item => item.FechaHoraApertura.HasValue)
            .ThenByDescending(item => item.FechaHoraApertura).ThenByDescending(item => item.Id)
            .FirstOrDefaultAsync(item => item.Estado == "Abierta", cancellationToken);
        if (string.Equals(request.MetodoPago, "Efectivo", StringComparison.OrdinalIgnoreCase) && caja is null)
            return Conflict(new { message = "Debe haber una caja abierta para cobrar en efectivo." });

        var pase = new PaseDiario
        {
            NombreCompleto = request.NombreCompleto.Trim(),
            Dni = dni,
            Fecha = hoy,
            Monto = MontoPase,
            MetodoPago = request.MetodoPago,
            RegistradoPorUsuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            CajaTurnoId = caja?.Id
        };
        dbContext.PasesDiarios.Add(pase);
        await dbContext.SaveChangesAsync(cancellationToken);
        var response = new PaseDiarioResponseDto(
            pase.Id, pase.NombreCompleto, pase.Dni, pase.Fecha, pase.Monto, pase.MetodoPago);
        return CreatedAtAction(nameof(Obtener), new { id = pase.Id }, response);
    }
}

[ApiController]
[Route("api/socios")]
public class SociosController(OlympusDbContext dbContext) : ControllerBase
{
    [HttpGet("verificar-acceso/{dni}")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> VerificarAcceso(string dni, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dni) || dni.Length > 20) return Ok(false);
        var hoy = DateTime.UtcNow.Date;
        var ahora = DateTime.UtcNow;
        var membresiaActiva = await dbContext.Usuarios.AsNoTracking()
            .Where(usuario => usuario.Dni == dni &&
                (usuario.Rol == RolUsuario.User || usuario.Rol == RolUsuario.Socio))
            .AnyAsync(usuario => usuario.Membresias.Any(membresia =>
                membresia.Estado == EstadoMembresia.Vigente &&
                membresia.FechaInicio <= ahora &&
                membresia.FechaFin >= ahora), cancellationToken);
        var paseDiario = await dbContext.PasesDiarios.AsNoTracking()
            .AnyAsync(pase => pase.Dni == dni && pase.Fecha == hoy, cancellationToken);
        var puedeIngresar = membresiaActiva || paseDiario;
        return Ok(puedeIngresar);
    }
}

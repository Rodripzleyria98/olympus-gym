using System.Security.Cryptography;
using System.Text;
using MercadoPago.Client.Preference;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;
using Microsoft.EntityFrameworkCore;
using Olympus.Api.Data;
using Olympus.Api.Dtos;
using Olympus.Api.Models;

namespace Olympus.Api.Services;

public class PagoService(
    OlympusDbContext dbContext,
    IConfiguration configuration,
    IHostEnvironment environment)
{
    public bool IsConfiguredForPayments
    {
        get
        {
            var accessToken = configuration["MercadoPago:AccessToken"];
            var webhookSecret = configuration["MercadoPago:WebhookSecret"];
            var urls = new[]
            {
                configuration["MercadoPago:BackUrls:Success"],
                configuration["MercadoPago:BackUrls:Failure"],
                configuration["MercadoPago:BackUrls:Pending"],
                configuration["MercadoPago:NotificationUrl"]
            };

            return !string.IsNullOrWhiteSpace(accessToken) &&
                !string.IsNullOrWhiteSpace(webhookSecret) &&
                urls.All(url => Uri.TryCreate(url, UriKind.Absolute, out var parsedUrl) &&
                    (!environment.IsProduction() ||
                        parsedUrl.Scheme == Uri.UriSchemeHttps && !parsedUrl.IsLoopback)) &&
                (!environment.IsProduction() ||
                    !accessToken.StartsWith("TEST-", StringComparison.OrdinalIgnoreCase));
        }
    }

    public async Task<CrearPreferenciaPagoResponseDto?> CrearPreferenciaRenovacionAsync(
        Guid usuarioId,
        int planId,
        CancellationToken cancellationToken)
    {
        var usuario = await dbContext.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == usuarioId && item.Rol == RolUsuario.User, cancellationToken);
        var plan = await dbContext.PlanesMembresia.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == planId && item.IsActivo, cancellationToken);
        if (usuario is null || plan is null)
        {
            return null;
        }

        var ahora = DateTime.UtcNow;
        var membresia = new MembresiaUsuario
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            PlanMembresiaId = plan.Id,
            FechaInicio = ahora,
            FechaFin = ahora.AddMonths(CalcularPeriodoMeses(plan.DuracionDias)),
            Estado = EstadoMembresia.Pendiente
        };
        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            MembresiaUsuarioId = membresia.Id,
            Monto = plan.Precio,
            FechaCreacion = ahora,
            MetodoPago = "MercadoPago",
            PeriodoMeses = CalcularPeriodoMeses(plan.DuracionDias),
            EstadoPago = EstadoPago.Pendiente
        };

        dbContext.MembresiasUsuario.Add(membresia);
        dbContext.Pagos.Add(pago);
        await dbContext.SaveChangesAsync(cancellationToken);

        var request = new PreferenceRequest
        {
            Items =
            [
                new PreferenceItemRequest
                {
                    Title = plan.Nombre,
                    Quantity = 1,
                    CurrencyId = "ARS",
                    UnitPrice = plan.Precio
                }
            ],
            Payer = new PreferencePayerRequest { Email = usuario.Email },
            ExternalReference = pago.Id.ToString(),
            BackUrls = new PreferenceBackUrlsRequest
            {
                Success = configuration["MercadoPago:BackUrls:Success"],
                Failure = configuration["MercadoPago:BackUrls:Failure"],
                Pending = configuration["MercadoPago:BackUrls:Pending"]
            },
            AutoReturn = "approved",
            NotificationUrl = configuration["MercadoPago:NotificationUrl"]
        };

        try
        {
            var preference = await new PreferenceClient().CreateAsync(request, cancellationToken: cancellationToken);
            pago.PreferenciaId = preference.Id;
            await dbContext.SaveChangesAsync(cancellationToken);
            return new CrearPreferenciaPagoResponseDto(
                pago.Id,
                preference.Id,
                preference.InitPoint,
                preference.SandboxInitPoint);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            pago.EstadoPago = EstadoPago.Rechazado;
            membresia.Estado = EstadoMembresia.Cancelada;
            await dbContext.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<EstadoPagoResponseDto?> ObtenerEstadoAsync(
        Guid usuarioId,
        Guid pagoId,
        CancellationToken cancellationToken) =>
        await dbContext.Pagos.AsNoTracking()
            .Where(pago => pago.Id == pagoId && pago.UsuarioId == usuarioId)
            .Select(pago => new EstadoPagoResponseDto(
                pago.Id,
                pago.MembresiaUsuario.PlanMembresia.Nombre,
                pago.Monto,
                pago.EstadoPago.ToString()))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task ProcesarNotificacionAsync(long mercadoPagoPaymentId, CancellationToken cancellationToken)
    {
        var payment = await new PaymentClient().GetAsync(
            mercadoPagoPaymentId, cancellationToken: cancellationToken);
        if (!Guid.TryParse(payment.ExternalReference, out var pagoId) ||
            payment.TransactionAmount is null ||
            !string.Equals(payment.CurrencyId, "ARS", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var pago = await dbContext.Pagos
            .Include(item => item.MembresiaUsuario)
            .ThenInclude(item => item.PlanMembresia)
            .FirstOrDefaultAsync(item => item.Id == pagoId && item.MetodoPago == "MercadoPago", cancellationToken);
        if (pago is null || pago.Monto != payment.TransactionAmount.Value)
        {
            return;
        }

        pago.TransaccionExternaId = payment.Id?.ToString() ?? mercadoPagoPaymentId.ToString();
        var nuevoAprobado = false;
        if (string.Equals(payment.Status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            if (pago.EstadoPago != EstadoPago.Aprobado)
            {
                var ahora = DateTime.UtcNow;
                var membresia = pago.MembresiaUsuario;
                var membresiaActivaHasta = await dbContext.MembresiasUsuario
                    .Where(item => item.UsuarioId == pago.UsuarioId &&
                        item.Id != membresia.Id &&
                        item.Estado == EstadoMembresia.Vigente && item.FechaFin >= ahora)
                    .Select(item => (DateTime?)item.FechaFin)
                    .MaxAsync(cancellationToken);
                membresia.FechaInicio = membresiaActivaHasta is { } fin && fin > ahora ? fin : ahora;
                membresia.FechaFin = membresia.FechaInicio.AddMonths(pago.PeriodoMeses);
                membresia.Estado = EstadoMembresia.Vigente;
                pago.EstadoPago = EstadoPago.Aprobado;
                pago.FechaAcreditacion = payment.DateApproved?.ToUniversalTime() ?? ahora;
                nuevoAprobado = true;
            }
        }
        else if (string.Equals(payment.Status, "cancelled", StringComparison.OrdinalIgnoreCase))
        {
            pago.EstadoPago = EstadoPago.Cancelado;
            pago.MembresiaUsuario.Estado = EstadoMembresia.Cancelada;
        }
        else if (payment.Status?.ToLowerInvariant() is "rejected" or "refunded" or "charged_back")
        {
            pago.EstadoPago = EstadoPago.Rechazado;
            pago.MembresiaUsuario.Estado = EstadoMembresia.Cancelada;
        }
        else
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        if (nuevoAprobado)
        {
            // TODO: Invocar servicio ARCA para emisión de Factura Electrónica.
        }
    }

    public static bool ValidarFirmaWebhook(
        string dataId,
        string? requestId,
        string? signature,
        string secret)
    {
        if (string.IsNullOrWhiteSpace(requestId) || string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in signature.Split(','))
        {
            var parts = field.Split('=', 2);
            if (parts.Length == 2)
            {
                fields[parts[0].Trim()] = parts[1].Trim();
            }
        }

        if (!fields.TryGetValue("ts", out var timestamp) || !fields.TryGetValue("v1", out var receivedHash))
        {
            return false;
        }

        var manifest = $"id:{dataId.ToLowerInvariant()};request-id:{requestId};ts:{timestamp};";
        var expectedHash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(manifest));
        try
        {
            return CryptographicOperations.FixedTimeEquals(expectedHash, Convert.FromHexString(receivedHash));
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static int CalcularPeriodoMeses(int duracionDias) =>
        Math.Max(1, (int)Math.Round(duracionDias / 30.0, MidpointRounding.AwayFromZero));
}
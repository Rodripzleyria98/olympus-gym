namespace Olympus.Api.Services;

internal static class CajaTurnoHelper
{
    internal static (DateTime Fecha, string Turno) TurnoActual()
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ArgentinaTimeZone());
        var turno = local.Hour < 15 ? "Mañana" : local.Hour < 19 ? "Tarde" : "Noche";
        return (FechaComercialAUtc(DateOnly.FromDateTime(local)), turno);
    }

    internal static DateTime FechaComercialAUtc(DateOnly fecha) =>
        fecha.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    internal static DateTime FechaLocalAUtc(DateOnly fecha) =>
        TimeZoneInfo.ConvertTimeToUtc(
            fecha.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified),
            ArgentinaTimeZone());

    private static TimeZoneInfo ArgentinaTimeZone() => TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "Argentina Standard Time" : "America/Argentina/Buenos_Aires");
}

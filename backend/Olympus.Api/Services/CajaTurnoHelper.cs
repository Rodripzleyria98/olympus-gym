namespace Olympus.Api.Services;

internal static class CajaTurnoHelper
{
    internal static (DateTime Fecha, string Turno) TurnoActual()
    {
        var zoneId = OperatingSystem.IsWindows()
            ? "Argentina Standard Time"
            : "America/Argentina/Buenos_Aires";
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(zoneId));
        var turno = local.Hour < 6 ? "Noche" : local.Hour < 14 ? "Mañana" : local.Hour < 22 ? "Tarde" : "Noche";
        var fechaTurno = local.Hour < 6 ? local.Date.AddDays(-1) : local.Date;
        return (DateTime.SpecifyKind(fechaTurno, DateTimeKind.Utc), turno);
    }
}

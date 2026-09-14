namespace SB.PortalSolicitudes.Application.Common;

/// <summary>
/// Igual que <see cref="FiltroSolicitudes"/> pero solo con lo que el dashboard necesita:
/// el alcance por rol mas el mismo corte aditivo por asignacion que el listado (ADR-0027),
/// para que ambos coincidan siempre sobre que ve cada usuario.
/// </summary>
public sealed record CriterioDashboard(AlcanceSolicitudes Alcance, int? AsignadasAUsuarioId, bool SoloSinAsignar)
{
    public static CriterioDashboard DeAlcance(AlcanceSolicitudes alcance) => new(alcance, null, false);
}

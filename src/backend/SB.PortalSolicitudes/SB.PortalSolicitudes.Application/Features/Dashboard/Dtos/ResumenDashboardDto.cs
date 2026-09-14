using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Dashboard.Dtos;

/// <param name="TotalSinAsignar">
/// Sin responsable, dentro del alcance del usuario (ADR-0027) — indicador del pool disponible
/// para un Analista, ajeno al corte de <c>Asignacion</c> ya aplicado al resto de este DTO.
/// </param>
public sealed record ResumenDashboardDto(
    IReadOnlyList<ConteoEstadoDto> PorEstado,
    IReadOnlyList<ConteoPrioridadDto> PorPrioridad,
    int TotalVencidas,
    int TotalSolicitudes,
    IReadOnlyList<SolicitudResumenDto> Recientes,
    int TotalSinAsignar);

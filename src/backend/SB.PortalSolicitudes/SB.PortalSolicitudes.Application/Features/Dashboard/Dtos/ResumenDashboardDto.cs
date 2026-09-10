using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Dashboard.Dtos;

public sealed record ResumenDashboardDto(
    IReadOnlyList<ConteoEstadoDto> PorEstado,
    IReadOnlyList<ConteoPrioridadDto> PorPrioridad,
    int TotalVencidas,
    int TotalSolicitudes,
    IReadOnlyList<SolicitudResumenDto> Recientes);

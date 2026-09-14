using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Dashboard.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Dashboard.Queries.ObtenerResumenDashboard;

/// <summary>
/// <see cref="Asignacion"/> es el mismo corte aditivo que <c>GET /api/solicitudes</c> (ADR-0027):
/// se aplica encima del alcance por rol, nunca en su lugar.
/// </summary>
public sealed record ObtenerResumenDashboardQuery(FiltroAsignacion Asignacion = FiltroAsignacion.Todas)
    : IQuery<Resultado<ResumenDashboardDto>>;

using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Dashboard.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Dashboard.Queries.ObtenerResumenDashboard;

public sealed record ObtenerResumenDashboardQuery : IQuery<Resultado<ResumenDashboardDto>>;

using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerPrioridadesTodas;

/// <summary>Para la pantalla de administracion: incluye prioridades inactivas (a diferencia de <c>GET /api/prioridades</c>).</summary>
public sealed record ObtenerPrioridadesTodasQuery : IQuery<Resultado<IReadOnlyList<PrioridadAdminDto>>>;

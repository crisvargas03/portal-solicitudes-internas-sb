using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerAreasTodas;

/// <summary>Para la pantalla de administracion: incluye areas inactivas (a diferencia de <c>GET /api/areas</c>).</summary>
public sealed record ObtenerAreasTodasQuery : IQuery<Resultado<IReadOnlyList<AreaAdminDto>>>;

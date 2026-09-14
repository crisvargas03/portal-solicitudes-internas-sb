using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Dtos;

namespace SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Queries.ObtenerEntidadesGubernamentalesTodas;

/// <summary>Para la pantalla de administracion: incluye entidades inactivas (a diferencia de <c>GET /api/entidades-gubernamentales</c>).</summary>
public sealed record ObtenerEntidadesGubernamentalesTodasQuery : IQuery<Resultado<IReadOnlyList<EntidadGubernamentalAdminDto>>>;

using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerTiposSolicitudTodos;

/// <summary>Para la pantalla de administracion: incluye tipos inactivos (a diferencia de <c>GET /api/tipos-solicitud</c>).</summary>
public sealed record ObtenerTiposSolicitudTodosQuery : IQuery<Resultado<IReadOnlyList<TipoSolicitudAdminDto>>>;

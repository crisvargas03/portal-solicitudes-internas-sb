using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;

namespace SB.PortalSolicitudes.Application.Features.Auth.Queries.ObtenerUsuarioActual;

/// <summary>Respalda <c>GET /api/auth/me</c>: el usuario sale del token, no de la consulta.</summary>
public sealed record ObtenerUsuarioActualQuery : IQuery<Resultado<UsuarioResumenDto>>;

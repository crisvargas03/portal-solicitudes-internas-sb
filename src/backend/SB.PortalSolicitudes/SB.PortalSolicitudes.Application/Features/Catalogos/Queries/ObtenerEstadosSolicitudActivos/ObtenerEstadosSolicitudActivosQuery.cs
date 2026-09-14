using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerEstadosSolicitudActivos;

public sealed record ObtenerEstadosSolicitudActivosQuery : IQuery<Resultado<IReadOnlyList<EstadoSolicitudDto>>>;

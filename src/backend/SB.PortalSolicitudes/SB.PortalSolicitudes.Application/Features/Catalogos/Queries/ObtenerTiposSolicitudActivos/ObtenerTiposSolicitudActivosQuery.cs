using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerTiposSolicitudActivos;

public sealed record ObtenerTiposSolicitudActivosQuery : IQuery<Resultado<IReadOnlyList<CatalogoDto>>>;

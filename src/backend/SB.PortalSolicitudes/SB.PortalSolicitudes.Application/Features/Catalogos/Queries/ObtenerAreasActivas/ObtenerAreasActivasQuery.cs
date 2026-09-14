using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerAreasActivas;

public sealed record ObtenerAreasActivasQuery : IQuery<Resultado<IReadOnlyList<CatalogoDto>>>;

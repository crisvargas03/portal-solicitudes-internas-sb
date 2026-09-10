using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerPrioridadesActivas;

public sealed record ObtenerPrioridadesActivasQuery : IQuery<Resultado<IReadOnlyList<PrioridadDto>>>;

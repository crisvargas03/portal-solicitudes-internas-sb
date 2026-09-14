using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Queries.ObtenerTransicionesDisponibles;

public sealed record ObtenerTransicionesDisponiblesQuery(int SolicitudId)
    : IQuery<Resultado<IReadOnlyList<TransicionDisponibleDto>>>;

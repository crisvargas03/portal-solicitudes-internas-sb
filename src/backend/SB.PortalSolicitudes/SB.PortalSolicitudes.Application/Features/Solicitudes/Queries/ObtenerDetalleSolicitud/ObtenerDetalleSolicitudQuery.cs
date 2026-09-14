using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Queries.ObtenerDetalleSolicitud;

public sealed record ObtenerDetalleSolicitudQuery(int Id) : IQuery<Resultado<SolicitudDetalleDto>>;

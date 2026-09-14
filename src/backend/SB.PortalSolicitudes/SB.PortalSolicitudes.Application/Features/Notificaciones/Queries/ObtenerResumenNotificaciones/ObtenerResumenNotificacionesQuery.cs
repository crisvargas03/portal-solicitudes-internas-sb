using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Notificaciones.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Notificaciones.Queries.ObtenerResumenNotificaciones;

public sealed record ObtenerResumenNotificacionesQuery : IQuery<Resultado<ResumenNotificacionesDto>>;

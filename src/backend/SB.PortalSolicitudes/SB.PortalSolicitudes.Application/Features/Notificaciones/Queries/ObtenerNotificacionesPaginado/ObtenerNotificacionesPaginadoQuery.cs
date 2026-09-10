using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Notificaciones.Dtos;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Features.Notificaciones.Queries.ObtenerNotificacionesPaginado;

/// <summary>Sin <c>UsuarioDestinoId</c>: el handler siempre lo fuerza al usuario autenticado.</summary>
public sealed record ObtenerNotificacionesPaginadoQuery(
    EstadoNotificacion? Estado,
    CanalNotificacion? Canal,
    int? SolicitudId,
    int Pagina = ParametrosPaginacion.PAGINA_MINIMA,
    int TamanoPagina = ParametrosPaginacion.TAMANO_PAGINA_PREDETERMINADO)
    : IQuery<Resultado<ResultadoPaginado<NotificacionDto>>>;

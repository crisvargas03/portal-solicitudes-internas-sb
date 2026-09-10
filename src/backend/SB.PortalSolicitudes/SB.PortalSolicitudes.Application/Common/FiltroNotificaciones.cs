using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Common;

/// <summary>
/// Criterios opcionales para el listado paginado de <see cref="Domain.Entities.Notificacion"/>.
/// </summary>
public class FiltroNotificaciones : ParametrosPaginacion
{
    public int? UsuarioDestinoId { get; set; }

    public EstadoNotificacion? Estado { get; set; }

    public CanalNotificacion? Canal { get; set; }

    public int? SolicitudId { get; set; }
}

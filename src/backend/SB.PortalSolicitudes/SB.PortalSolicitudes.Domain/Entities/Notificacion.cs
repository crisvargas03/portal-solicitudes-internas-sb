using SB.PortalSolicitudes.Domain.Common;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Registro de auditoria de una notificacion emitida por creacion, asignacion, cambio de
/// estado o cierre. No es el mecanismo de entrega: el canal concreto sigue pendiente de
/// decision en ADR-0003.
/// </summary>
public class Notificacion : EntidadBase
{
    public const int MAX_LONGITUD_ASUNTO = 200;
    public const int MAX_LONGITUD_MENSAJE = 1000;

    public int SolicitudId { get; set; }

    public Solicitud? Solicitud { get; set; }

    public int UsuarioDestinoId { get; set; }

    public Usuario? UsuarioDestino { get; set; }

    public CanalNotificacion Canal { get; set; }

    public required string Asunto { get; set; }

    public required string Mensaje { get; set; }

    public EstadoNotificacion Estado { get; set; } = EstadoNotificacion.Pendiente;

    public DateTime Fecha { get; set; }
}

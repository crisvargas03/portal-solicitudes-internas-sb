namespace SB.PortalSolicitudes.Application.Abstractions.Notificaciones;

/// <summary>
/// Pedido de notificacion que un handler emite al ocurrir un evento de negocio (creacion,
/// asignacion, cambio de estado, cierre). No es el registro persistido: ver
/// <see cref="INotificationService"/>.
/// </summary>
public sealed record NotificacionSolicitada(int SolicitudId, int UsuarioDestinoId, string Asunto, string Mensaje);

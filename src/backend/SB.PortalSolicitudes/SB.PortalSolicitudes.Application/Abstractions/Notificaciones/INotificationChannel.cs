using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Abstractions.Notificaciones;

/// <summary>
/// Un medio de entrega de notificaciones (ver ADR-0003, Aceptada: alcance simulado, sin
/// envio real de correo ni de cola). <see cref="INotificationService"/> despacha a los
/// canales registrados; cada canal decide como dejar constancia del envio.
/// </summary>
public interface INotificationChannel
{
    CanalNotificacion Canal { get; }

    Task EnviarAsync(NotificacionSolicitada solicitud, CancellationToken cancellationToken = default);
}

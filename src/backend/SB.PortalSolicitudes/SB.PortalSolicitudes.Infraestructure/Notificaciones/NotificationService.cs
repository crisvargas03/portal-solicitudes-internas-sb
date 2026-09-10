using SB.PortalSolicitudes.Application.Abstractions.Notificaciones;

namespace SB.PortalSolicitudes.Infraestructure.Notificaciones;

/// <summary>Despacha a todos los canales registrados (ver ADR-0003). Hoy hay uno: <see cref="CanalNotificacionBaseDeDatos"/>.</summary>
public class NotificationService : INotificationService
{
    private readonly IEnumerable<INotificationChannel> _canales;

    public NotificationService(IEnumerable<INotificationChannel> canales)
    {
        _canales = canales;
    }

    public async Task NotificarAsync(NotificacionSolicitada solicitud, CancellationToken cancellationToken = default)
    {
        foreach (INotificationChannel canal in _canales)
        {
            await canal.EnviarAsync(solicitud, cancellationToken);
        }
    }
}

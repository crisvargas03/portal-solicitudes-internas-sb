using Microsoft.Extensions.Logging;
using SB.PortalSolicitudes.Application.Abstractions.Notificaciones;

namespace SB.PortalSolicitudes.Infraestructure.Notificaciones;

/// <summary>
/// Despacha a todos los canales registrados (ver ADR-0003). Hoy hay uno:
/// <see cref="CanalNotificacionBaseDeDatos"/>. Un fallo al notificar no debe revertir ni
/// hacer fallar la operacion que ya se confirmo (ADR-0003): se atrapa y registra por canal.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IEnumerable<INotificationChannel> _canales;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IEnumerable<INotificationChannel> canales, ILogger<NotificationService> logger)
    {
        _canales = canales;
        _logger = logger;
    }

    public async Task NotificarAsync(NotificacionSolicitada solicitud, CancellationToken cancellationToken = default)
    {
        foreach (INotificationChannel canal in _canales)
        {
            try
            {
                await canal.EnviarAsync(solicitud, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Fallo al notificar por el canal {Canal} para la solicitud {SolicitudId}",
                    canal.Canal, solicitud.SolicitudId);
            }
        }
    }
}

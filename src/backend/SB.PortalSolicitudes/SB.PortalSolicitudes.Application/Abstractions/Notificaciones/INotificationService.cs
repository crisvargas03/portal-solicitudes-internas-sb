namespace SB.PortalSolicitudes.Application.Abstractions.Notificaciones;

/// <summary>
/// Punto unico que los handlers invocan al crear, asignar, cambiar de estado o cerrar una
/// solicitud (ver ADR-0003). No conoce el canal concreto: eso lo decide la implementacion
/// en Infraestructure.
/// </summary>
public interface INotificationService
{
    Task NotificarAsync(NotificacionSolicitada solicitud, CancellationToken cancellationToken = default);
}

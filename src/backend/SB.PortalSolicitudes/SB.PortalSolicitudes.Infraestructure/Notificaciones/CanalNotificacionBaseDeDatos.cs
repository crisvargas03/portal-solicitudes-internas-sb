using Microsoft.Extensions.Logging;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Notificaciones;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Infraestructure.Notificaciones;

/// <summary>
/// Unico canal implementado (ver ADR-0003, Aceptada: alcance simulado). Persiste el
/// registro de auditoria en <see cref="Notificacion"/> y lo refleja en el log
/// estructurado; no hay envio real de correo ni de cola.
/// </summary>
public class CanalNotificacionBaseDeDatos : INotificationChannel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProveedorFechaHora _proveedorFechaHora;
    private readonly ILogger<CanalNotificacionBaseDeDatos> _logger;

    public CanalNotificacionBaseDeDatos(
        IUnitOfWork unitOfWork, IProveedorFechaHora proveedorFechaHora, ILogger<CanalNotificacionBaseDeDatos> logger)
    {
        _unitOfWork = unitOfWork;
        _proveedorFechaHora = proveedorFechaHora;
        _logger = logger;
    }

    public CanalNotificacion Canal => CanalNotificacion.BaseDeDatos;

    public async Task EnviarAsync(NotificacionSolicitada solicitud, CancellationToken cancellationToken = default)
    {
        Notificacion notificacion = new()
        {
            SolicitudId = solicitud.SolicitudId,
            UsuarioDestinoId = solicitud.UsuarioDestinoId,
            Canal = Canal,
            Asunto = solicitud.Asunto,
            Mensaje = solicitud.Mensaje,
            Estado = EstadoNotificacion.Enviada,
            Fecha = _proveedorFechaHora.Ahora
        };

        await _unitOfWork.Notificaciones.AgregarAsync(notificacion, cancellationToken);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        _logger.LogInformation(
            "Notificacion emitida a usuario {UsuarioDestinoId} para solicitud {SolicitudId}: {Asunto}",
            solicitud.UsuarioDestinoId, solicitud.SolicitudId, solicitud.Asunto);
    }
}

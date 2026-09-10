namespace SB.PortalSolicitudes.Application.Features.Notificaciones.Dtos;

public sealed record NotificacionDto(
    int Id, int SolicitudId, string CodigoSolicitud, string Canal, string Asunto, string Mensaje, string Estado, DateTime Fecha);

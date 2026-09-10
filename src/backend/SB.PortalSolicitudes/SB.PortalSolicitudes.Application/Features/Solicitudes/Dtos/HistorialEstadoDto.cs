using SB.PortalSolicitudes.Application.Common.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

public sealed record HistorialEstadoDto(
    int Id,
    EstadoSolicitudDto? EstadoAnterior,
    EstadoSolicitudDto EstadoNuevo,
    UsuarioResumenDto Usuario,
    string? Comentario,
    DateTime Fecha);

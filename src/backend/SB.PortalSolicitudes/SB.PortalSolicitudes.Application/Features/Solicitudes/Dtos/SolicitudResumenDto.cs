using SB.PortalSolicitudes.Application.Common.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

public sealed record SolicitudResumenDto(
    int Id,
    string Codigo,
    string Titulo,
    EstadoSolicitudDto Estado,
    PrioridadDto Prioridad,
    CatalogoDto Area,
    CatalogoDto TipoSolicitud,
    UsuarioResumenDto Solicitante,
    UsuarioResumenDto? Asignado,
    DateTime FechaCreacion,
    DateTime? FechaCompromiso,
    bool EstaVencida);

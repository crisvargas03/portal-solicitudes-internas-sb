using SB.PortalSolicitudes.Application.Common.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

/// <param name="ComentarioResolucion">
/// Derivado de la ultima transicion hacia RESUELTA (ver ADR-0001); no es un campo propio de la solicitud.
/// </param>
public sealed record SolicitudDetalleDto(
    int Id,
    string Codigo,
    string Titulo,
    string Descripcion,
    EstadoSolicitudDto Estado,
    PrioridadDto Prioridad,
    CatalogoDto Area,
    CatalogoDto TipoSolicitud,
    UsuarioResumenDto Solicitante,
    UsuarioResumenDto? Asignado,
    DateTime FechaCreacion,
    DateTime? FechaCompromiso,
    bool EstaVencida,
    string? ComentarioResolucion,
    IReadOnlyList<HistorialEstadoDto> Historial,
    IReadOnlyList<ComentarioDto> Comentarios,
    IReadOnlyList<AdjuntoDto> Adjuntos);

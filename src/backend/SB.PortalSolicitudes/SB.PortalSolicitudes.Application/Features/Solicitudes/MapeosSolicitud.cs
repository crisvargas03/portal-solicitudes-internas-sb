using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Common;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes;

/// <summary>
/// Mapeo manual de entidades a DTOs de la Api (no se usa AutoMapper: ver docs del plan).
/// Asume que <see cref="Solicitud"/> llega con sus navegaciones cargadas
/// (<c>ObtenerDetalleAsync</c>/<c>ObtenerPaginadoAsync</c>).
/// </summary>
public static class MapeosSolicitud
{
    public static SolicitudResumenDto ASolicitudResumenDto(Solicitud solicitud)
    {
        bool estaVencida = solicitud.FechaCompromiso is not null
            && solicitud.FechaCompromiso < DateTime.UtcNow
            && solicitud.Estado is not null
            && !solicitud.Estado.EsFinal;

        return new SolicitudResumenDto(
            solicitud.Id,
            solicitud.Codigo,
            solicitud.Titulo,
            AEstadoSolicitudDto(solicitud.Estado!),
            APrioridadDto(solicitud.Prioridad!),
            ACatalogoDto(solicitud.Area!),
            ACatalogoDto(solicitud.TipoSolicitud!),
            AUsuarioResumenDto(solicitud.UsuarioSolicitante!),
            solicitud.UsuarioAsignado is null ? null : AUsuarioResumenDto(solicitud.UsuarioAsignado),
            solicitud.FechaCreacion,
            solicitud.FechaCompromiso,
            estaVencida);
    }

    public static SolicitudDetalleDto ASolicitudDetalleDto(
        Solicitud solicitud, string? comentarioResolucion, bool incluirComentariosInternos)
    {
        SolicitudResumenDto resumen = ASolicitudResumenDto(solicitud);

        List<HistorialEstadoDto> historial = solicitud.Historial
            .OrderBy(historial => historial.Fecha)
            .Select(historial => new HistorialEstadoDto(
                historial.Id,
                historial.EstadoAnterior is null ? null : AEstadoSolicitudDto(historial.EstadoAnterior),
                AEstadoSolicitudDto(historial.EstadoNuevo!),
                AUsuarioResumenDto(historial.Usuario!),
                historial.Comentario,
                historial.Fecha))
            .ToList();

        List<ComentarioDto> comentarios = solicitud.Comentarios
            .Where(comentario => incluirComentariosInternos || !comentario.EsInterno)
            .OrderBy(comentario => comentario.Fecha)
            .Select(comentario => new ComentarioDto(
                comentario.Id, comentario.Texto, comentario.EsInterno, AUsuarioResumenDto(comentario.Usuario!), comentario.Fecha))
            .ToList();

        List<AdjuntoDto> adjuntos = solicitud.Adjuntos
            .OrderBy(adjunto => adjunto.Fecha)
            .Select(adjunto => new AdjuntoDto(
                adjunto.Id, adjunto.Descripcion, adjunto.Url, AUsuarioResumenDto(adjunto.Usuario!), adjunto.Fecha))
            .ToList();

        return new SolicitudDetalleDto(
            resumen.Id,
            resumen.Codigo,
            resumen.Titulo,
            solicitud.Descripcion,
            resumen.Estado,
            resumen.Prioridad,
            resumen.Area,
            resumen.TipoSolicitud,
            resumen.Solicitante,
            resumen.Asignado,
            resumen.FechaCreacion,
            resumen.FechaCompromiso,
            resumen.EstaVencida,
            comentarioResolucion,
            historial,
            comentarios,
            adjuntos);
    }

    public static EstadoSolicitudDto AEstadoSolicitudDto(EstadoSolicitud estado) =>
        new(estado.Id, estado.Codigo, estado.Nombre, estado.Orden, estado.EsFinal);

    public static PrioridadDto APrioridadDto(Prioridad prioridad) =>
        new(prioridad.Id, prioridad.Nombre, prioridad.Nivel);

    public static CatalogoDto ACatalogoDto(CatalogoBase catalogo) =>
        new(catalogo.Id, catalogo.Nombre);

    public static UsuarioResumenDto AUsuarioResumenDto(Usuario usuario) =>
        new(usuario.Id, usuario.Nombre, usuario.Email, usuario.Rol.ToString(), usuario.Activo);
}

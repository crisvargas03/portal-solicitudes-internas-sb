using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Notificaciones.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Notificaciones.Queries.ObtenerNotificacionesPaginado;

public class ObtenerNotificacionesPaginadoQueryHandler
    : IQueryHandler<ObtenerNotificacionesPaginadoQuery, Resultado<ResultadoPaginado<NotificacionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;

    public ObtenerNotificacionesPaginadoQueryHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
    }

    public async Task<Resultado<ResultadoPaginado<NotificacionDto>>> HandleAsync(
        ObtenerNotificacionesPaginadoQuery query, CancellationToken cancellationToken = default)
    {
        // Falla cerrado (mismo criterio que ADR-0029): un UsuarioDestinoId nulo el repositorio
        // lo interpreta como "sin filtro" y devolveria las notificaciones de todos los usuarios.
        if (_usuarioActual.Id is null)
        {
            return Resultado.Fallido<ResultadoPaginado<NotificacionDto>>(
                Error.NoAutorizado("Auth.NoAutenticado", "No hay una sesion activa."));
        }

        FiltroNotificaciones filtro = new()
        {
            UsuarioDestinoId = _usuarioActual.Id,
            Estado = query.Estado,
            Canal = query.Canal,
            SolicitudId = query.SolicitudId,
            Pagina = query.Pagina,
            TamanoPagina = query.TamanoPagina
        };

        ResultadoPaginado<Notificacion> pagina = await _unitOfWork.Notificaciones.ObtenerPaginadoAsync(filtro, cancellationToken);

        List<NotificacionDto> elementos = pagina.Elementos
            .Select(notificacion => new NotificacionDto(
                notificacion.Id,
                notificacion.SolicitudId,
                notificacion.Solicitud!.Codigo,
                notificacion.Canal.ToString(),
                notificacion.Asunto,
                notificacion.Mensaje,
                notificacion.Estado.ToString(),
                notificacion.Fecha))
            .ToList();

        return new ResultadoPaginado<NotificacionDto>(elementos, pagina.TotalElementos, pagina.Pagina, pagina.TamanoPagina);
    }
}

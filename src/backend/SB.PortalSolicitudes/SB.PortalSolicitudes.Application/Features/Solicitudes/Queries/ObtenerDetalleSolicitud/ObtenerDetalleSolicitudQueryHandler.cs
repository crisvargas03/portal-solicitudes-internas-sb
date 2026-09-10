using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Queries.ObtenerDetalleSolicitud;

public class ObtenerDetalleSolicitudQueryHandler : IQueryHandler<ObtenerDetalleSolicitudQuery, Resultado<SolicitudDetalleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;

    public ObtenerDetalleSolicitudQueryHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
    }

    public async Task<Resultado<SolicitudDetalleDto>> HandleAsync(
        ObtenerDetalleSolicitudQuery query, CancellationToken cancellationToken = default)
    {
        Solicitud? solicitud = await _unitOfWork.Solicitudes.ObtenerDetalleAsync(query.Id, cancellationToken);

        // Un Solicitante que pide la solicitud de otro recibe 404, no 403: no debe poder
        // distinguir "no existe" de "no es mia" (evita filtrar el rango de Ids en uso).
        if (solicitud is null || !PuedeVer(solicitud))
        {
            return Resultado.Fallido<SolicitudDetalleDto>(
                Error.NoEncontrado("Solicitud.NoEncontrada", "La solicitud no existe."));
        }

        string? comentarioResolucion = solicitud.Historial
            .Where(historial => historial.EstadoNuevo!.Codigo == CodigosEstadoSolicitud.RESUELTA)
            .OrderByDescending(historial => historial.Fecha)
            .FirstOrDefault()?.Comentario;

        bool incluirComentariosInternos = _usuarioActual.Rol is RolUsuario.Administrador or RolUsuario.Analista;

        return MapeosSolicitud.ASolicitudDetalleDto(solicitud, comentarioResolucion, incluirComentariosInternos);
    }

    private bool PuedeVer(Solicitud solicitud)
    {
        return _usuarioActual.Rol switch
        {
            RolUsuario.Solicitante => solicitud.UsuarioSolicitanteId == _usuarioActual.Id,
            _ => true
        };
    }
}

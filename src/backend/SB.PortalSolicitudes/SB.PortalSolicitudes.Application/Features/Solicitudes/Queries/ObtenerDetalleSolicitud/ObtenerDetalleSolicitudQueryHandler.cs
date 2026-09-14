using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common;
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
        Resultado<AlcanceSolicitudes> alcanceResultado = AlcanceSolicitudesFactory.Calcular(_usuarioActual);
        if (alcanceResultado.EsFallido)
        {
            return Resultado.Fallido<SolicitudDetalleDto>(alcanceResultado.Error);
        }

        Solicitud? solicitud = await _unitOfWork.Solicitudes.ObtenerDetalleAsync(query.Id, cancellationToken);

        // Fuera del alcance del rol se responde 404, no 403: no debe poder distinguir
        // "no existe" de "no me corresponde" (evita filtrar el rango de Ids en uso, ADR-0012).
        if (solicitud is null || !alcanceResultado.Valor.Incluye(solicitud))
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
}

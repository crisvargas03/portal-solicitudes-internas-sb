using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Notificaciones.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Notificaciones.Queries.ObtenerResumenNotificaciones;

public class ObtenerResumenNotificacionesQueryHandler
    : IQueryHandler<ObtenerResumenNotificacionesQuery, Resultado<ResumenNotificacionesDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;

    public ObtenerResumenNotificacionesQueryHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
    }

    public async Task<Resultado<ResumenNotificacionesDto>> HandleAsync(
        ObtenerResumenNotificacionesQuery query, CancellationToken cancellationToken = default)
    {
        if (_usuarioActual.Id is null)
        {
            return Resultado.Fallido<ResumenNotificacionesDto>(
                Error.NoAutorizado("Auth.NoAutenticado", "No hay una sesion activa."));
        }

        int noLeidas = await _unitOfWork.Notificaciones.ContarNoLeidasPorUsuarioAsync(
            _usuarioActual.Id.Value, cancellationToken);

        return new ResumenNotificacionesDto(noLeidas);
    }
}

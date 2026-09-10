using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Queries.ObtenerTransicionesDisponibles;

public class ObtenerTransicionesDisponiblesQueryHandler
    : IQueryHandler<ObtenerTransicionesDisponiblesQuery, Resultado<IReadOnlyList<TransicionDisponibleDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;

    public ObtenerTransicionesDisponiblesQueryHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
    }

    public async Task<Resultado<IReadOnlyList<TransicionDisponibleDto>>> HandleAsync(
        ObtenerTransicionesDisponiblesQuery query, CancellationToken cancellationToken = default)
    {
        Solicitud? solicitud = await _unitOfWork.Solicitudes.ObtenerParaCambioDeEstadoAsync(query.SolicitudId, cancellationToken);

        if (solicitud is null || EsSolicitudDeOtro(solicitud))
        {
            return Resultado.Fallido<IReadOnlyList<TransicionDisponibleDto>>(
                Error.NoEncontrado("Solicitud.NoEncontrada", "La solicitud no existe."));
        }

        IReadOnlyList<TransicionPermitida> transiciones =
            await _unitOfWork.TransicionesPermitidas.ObtenerDesdeEstadoAsync(solicitud.EstadoId, cancellationToken);

        List<TransicionDisponibleDto> disponibles = transiciones
            .Where(transicion => transicion.RolesPermitidos.Any(rolPermitido => rolPermitido.Rol == _usuarioActual.Rol))
            .Select(transicion => new TransicionDisponibleDto(
                transicion.EstadoDestinoId,
                transicion.EstadoDestino!.Codigo,
                transicion.EstadoDestino.Nombre,
                transicion.RequiereComentario))
            .ToList();

        return disponibles;
    }

    private bool EsSolicitudDeOtro(Solicitud solicitud) =>
        _usuarioActual.Rol == RolUsuario.Solicitante && solicitud.UsuarioSolicitanteId != _usuarioActual.Id;
}

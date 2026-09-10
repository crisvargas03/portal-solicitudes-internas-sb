using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearAdjunto;

public class CrearAdjuntoCommandHandler : ICommandHandler<CrearAdjuntoCommand, Resultado<AdjuntoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IProveedorFechaHora _proveedorFechaHora;

    public CrearAdjuntoCommandHandler(
        IUnitOfWork unitOfWork, IUsuarioActual usuarioActual, IProveedorFechaHora proveedorFechaHora)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _proveedorFechaHora = proveedorFechaHora;
    }

    public async Task<Resultado<AdjuntoDto>> HandleAsync(
        CrearAdjuntoCommand command, CancellationToken cancellationToken = default)
    {
        if (_usuarioActual.Id is null || _usuarioActual.Rol is null)
        {
            return Resultado.Fallido<AdjuntoDto>(Error.NoAutorizado("Auth.NoAutenticado", "No hay una sesion activa."));
        }

        Solicitud? solicitud = await _unitOfWork.Solicitudes.ObtenerParaCambioDeEstadoAsync(
            command.SolicitudId, cancellationToken);

        bool esSolicitante = _usuarioActual.Rol == RolUsuario.Solicitante;

        if (solicitud is null || (esSolicitante && solicitud.UsuarioSolicitanteId != _usuarioActual.Id))
        {
            return Resultado.Fallido<AdjuntoDto>(Error.NoEncontrado("Solicitud.NoEncontrada", "La solicitud no existe."));
        }

        Adjunto adjunto = new()
        {
            SolicitudId = solicitud.Id,
            UsuarioId = _usuarioActual.Id.Value,
            Descripcion = command.Descripcion,
            Url = command.Url,
            Fecha = _proveedorFechaHora.Ahora
        };

        await _unitOfWork.Adjuntos.AgregarAsync(adjunto, cancellationToken);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        Usuario autor = (await _unitOfWork.Usuarios.ObtenerPorIdAsync(_usuarioActual.Id.Value, cancellationToken))!;

        return new AdjuntoDto(
            adjunto.Id, adjunto.Descripcion, adjunto.Url, MapeosSolicitud.AUsuarioResumenDto(autor), adjunto.Fecha);
    }
}

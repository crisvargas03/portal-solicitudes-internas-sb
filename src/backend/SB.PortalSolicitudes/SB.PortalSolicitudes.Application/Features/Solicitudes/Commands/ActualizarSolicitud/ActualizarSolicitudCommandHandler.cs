using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.ActualizarSolicitud;

public class ActualizarSolicitudCommandHandler : ICommandHandler<ActualizarSolicitudCommand, Resultado<SolicitudResumenDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;

    public ActualizarSolicitudCommandHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
    }

    public async Task<Resultado<SolicitudResumenDto>> HandleAsync(
        ActualizarSolicitudCommand command, CancellationToken cancellationToken = default)
    {
        Solicitud? solicitud = await _unitOfWork.Solicitudes.ObtenerParaCambioDeEstadoAsync(command.Id, cancellationToken);

        if (solicitud is null)
        {
            return Resultado.Fallido<SolicitudResumenDto>(
                Error.NoEncontrado("Solicitud.NoEncontrada", "La solicitud no existe."));
        }

        bool esPropia = solicitud.UsuarioSolicitanteId == _usuarioActual.Id;
        bool esAdministrador = _usuarioActual.Rol == RolUsuario.Administrador;

        if (!esPropia && !esAdministrador)
        {
            return Resultado.Fallido<SolicitudResumenDto>(
                Error.Prohibido("Solicitud.NoAutorizada", "No puede editar esta solicitud."));
        }

        if (solicitud.Estado!.Codigo != CodigosEstadoSolicitud.REGISTRADA)
        {
            return Resultado.Fallido<SolicitudResumenDto>(
                Error.Conflicto(
                    "Solicitud.NoEditable", "Solo se puede editar una solicitud mientras esta en estado Registrada."));
        }

        solicitud.Titulo = command.Titulo ?? solicitud.Titulo;
        solicitud.Descripcion = command.Descripcion ?? solicitud.Descripcion;
        solicitud.TipoSolicitudId = command.TipoSolicitudId ?? solicitud.TipoSolicitudId;
        solicitud.PrioridadId = command.PrioridadId ?? solicitud.PrioridadId;
        solicitud.AreaId = command.AreaId ?? solicitud.AreaId;
        solicitud.FechaCompromiso = command.FechaCompromiso ?? solicitud.FechaCompromiso;

        _unitOfWork.Solicitudes.Actualizar(solicitud);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        Solicitud actualizada = (await _unitOfWork.Solicitudes.ObtenerDetalleAsync(solicitud.Id, cancellationToken))!;

        return MapeosSolicitud.ASolicitudResumenDto(actualizada);
    }
}

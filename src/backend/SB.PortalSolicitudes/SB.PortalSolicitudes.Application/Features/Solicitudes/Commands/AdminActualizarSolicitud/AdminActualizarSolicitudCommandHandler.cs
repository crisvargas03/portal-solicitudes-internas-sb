using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.AdminActualizarSolicitud;

public class AdminActualizarSolicitudCommandHandler
    : ICommandHandler<AdminActualizarSolicitudCommand, Resultado<SolicitudResumenDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IProveedorFechaHora _proveedorFechaHora;

    public AdminActualizarSolicitudCommandHandler(
        IUnitOfWork unitOfWork, IUsuarioActual usuarioActual, IProveedorFechaHora proveedorFechaHora)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _proveedorFechaHora = proveedorFechaHora;
    }

    public async Task<Resultado<SolicitudResumenDto>> HandleAsync(
        AdminActualizarSolicitudCommand command, CancellationToken cancellationToken = default)
    {
        if (_usuarioActual.Id is null)
        {
            return Resultado.Fallido<SolicitudResumenDto>(
                Error.NoAutorizado("Auth.NoAutenticado", "No hay una sesion activa."));
        }

        Solicitud? solicitud = await _unitOfWork.Solicitudes.ObtenerParaCambioDeEstadoAsync(command.Id, cancellationToken);

        if (solicitud is null)
        {
            return Resultado.Fallido<SolicitudResumenDto>(
                Error.NoEncontrado("Solicitud.NoEncontrada", "La solicitud no existe."));
        }

        Error? errorReferencias = await ValidadorReferenciasSolicitud.ValidarAsync(
            _unitOfWork, command.AreaId, command.TipoSolicitudId, command.PrioridadId, cancellationToken);

        if (errorReferencias is not null)
        {
            return Resultado.Fallido<SolicitudResumenDto>(errorReferencias);
        }

        solicitud.Titulo = command.Titulo;
        solicitud.Descripcion = command.Descripcion;
        solicitud.TipoSolicitudId = command.TipoSolicitudId;
        solicitud.PrioridadId = command.PrioridadId;
        solicitud.AreaId = command.AreaId;

        await _unitOfWork.IniciarTransaccionAsync(cancellationToken);

        try
        {
            _unitOfWork.Solicitudes.Actualizar(solicitud);

            Comentario comentario = new()
            {
                SolicitudId = solicitud.Id,
                UsuarioId = _usuarioActual.Id.Value,
                Texto = "Solicitud editada por Administrador.",
                EsInterno = true,
                Fecha = _proveedorFechaHora.Ahora
            };

            await _unitOfWork.Comentarios.AgregarAsync(comentario, cancellationToken);
        }
        catch
        {
            await _unitOfWork.RevertirTransaccionAsync(cancellationToken);
            throw;
        }

        await _unitOfWork.ConfirmarTransaccionAsync(cancellationToken);

        Solicitud actualizada = (await _unitOfWork.Solicitudes.ObtenerDetalleAsync(solicitud.Id, cancellationToken))!;

        return MapeosSolicitud.ASolicitudResumenDto(actualizada);
    }
}

using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Notificaciones;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CambiarEstado;

/// <summary>
/// Ejecuta un cambio de estado contra la maquina de estados declarada en
/// <see cref="TransicionPermitida"/> (ver ADR-0005): no hay un <c>switch</c> sobre codigos
/// de estado, la tabla es la regla. El comentario de resolucion (ADR-0001) es simplemente
/// el <c>Comentario</c> de la transicion cuyo destino es <c>RESUELTA</c>.
/// </summary>
public class CambiarEstadoCommandHandler : ICommandHandler<CambiarEstadoCommand, Resultado<SolicitudResumenDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IProveedorFechaHora _proveedorFechaHora;
    private readonly INotificationService _notificationService;

    public CambiarEstadoCommandHandler(
        IUnitOfWork unitOfWork,
        IUsuarioActual usuarioActual,
        IProveedorFechaHora proveedorFechaHora,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _proveedorFechaHora = proveedorFechaHora;
        _notificationService = notificationService;
    }

    public async Task<Resultado<SolicitudResumenDto>> HandleAsync(
        CambiarEstadoCommand command, CancellationToken cancellationToken = default)
    {
        if (_usuarioActual.Id is null || _usuarioActual.Rol is null)
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

        TransicionPermitida? transicion = await _unitOfWork.TransicionesPermitidas.ObtenerAsync(
            solicitud.EstadoId, command.EstadoDestinoId, cancellationToken);

        if (transicion is null)
        {
            return Resultado.Fallido<SolicitudResumenDto>(
                Error.Conflicto(
                    "Solicitud.TransicionNoPermitida",
                    $"No se puede pasar de '{solicitud.Estado!.Nombre}' al estado solicitado."));
        }

        bool rolAutorizado = transicion.RolesPermitidos.Any(rolPermitido => rolPermitido.Rol == _usuarioActual.Rol);

        if (!rolAutorizado)
        {
            return Resultado.Fallido<SolicitudResumenDto>(
                Error.Prohibido("Solicitud.RolNoAutorizado", "Su rol no puede ejecutar este cambio de estado."));
        }

        if (transicion.RequiereComentario && string.IsNullOrWhiteSpace(command.Comentario))
        {
            return Resultado.Fallido<SolicitudResumenDto>(
                Error.Validacion("Solicitud.ComentarioRequerido", "Este cambio de estado requiere un comentario."));
        }

        DateTime ahora = _proveedorFechaHora.Ahora;
        int estadoAnteriorId = solicitud.EstadoId;
        string nombreEstadoNuevo = transicion.EstadoDestino!.Nombre;

        await _unitOfWork.IniciarTransaccionAsync(cancellationToken);

        try
        {
            solicitud.EstadoId = command.EstadoDestinoId;
            _unitOfWork.Solicitudes.Actualizar(solicitud);

            HistorialEstado historial = new()
            {
                SolicitudId = solicitud.Id,
                EstadoAnteriorId = estadoAnteriorId,
                EstadoNuevoId = command.EstadoDestinoId,
                UsuarioId = _usuarioActual.Id.Value,
                Comentario = command.Comentario,
                Fecha = ahora
            };

            await _unitOfWork.HistorialEstados.AgregarAsync(historial, cancellationToken);
        }
        catch
        {
            // Solo revierte lo previo a Confirmar: esa llamada ya limpia su propia
            // transaccion en su "finally" (commit o rollback + dispose) incluso si falla,
            // asi que revertir de nuevo aqui encontraria la transaccion ya cerrada.
            await _unitOfWork.RevertirTransaccionAsync(cancellationToken);
            throw;
        }

        await _unitOfWork.ConfirmarTransaccionAsync(cancellationToken);

        await NotificarInteresadosAsync(solicitud, nombreEstadoNuevo, cancellationToken);

        Solicitud actualizada = (await _unitOfWork.Solicitudes.ObtenerDetalleAsync(solicitud.Id, cancellationToken))!;

        return MapeosSolicitud.ASolicitudResumenDto(actualizada);
    }

    private async Task NotificarInteresadosAsync(Solicitud solicitud, string nombreEstadoNuevo, CancellationToken cancellationToken)
    {
        string asunto = $"Solicitud {solicitud.Codigo}: cambio de estado";
        string mensaje = $"La solicitud {solicitud.Codigo} cambio a '{nombreEstadoNuevo}'.";

        await _notificationService.NotificarAsync(
            new NotificacionSolicitada(solicitud.Id, solicitud.UsuarioSolicitanteId, asunto, mensaje), cancellationToken);

        if (solicitud.UsuarioAsignadoId is not null && solicitud.UsuarioAsignadoId != solicitud.UsuarioSolicitanteId)
        {
            await _notificationService.NotificarAsync(
                new NotificacionSolicitada(solicitud.Id, solicitud.UsuarioAsignadoId.Value, asunto, mensaje), cancellationToken);
        }
    }
}

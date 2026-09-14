using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Notificaciones;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CambiarAsignacion;

/// <summary>
/// Administrador puede asignar o reasignar libremente. Analista solo puede reclamar para
/// si mismo una solicitud que todavia no tiene responsable (ver ADR-0012): reasignar una
/// ya asignada, o asignarsela a otra persona, son exclusivos de Administrador. El
/// <c>[Authorize(Roles=...)]</c> del controlador ya excluye a Solicitante; este matiz mas
/// fino entre Administrador y Analista solo puede resolverse aqui.
/// </summary>
public class CambiarAsignacionCommandHandler : ICommandHandler<CambiarAsignacionCommand, Resultado<SolicitudResumenDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly INotificationService _notificationService;

    public CambiarAsignacionCommandHandler(
        IUnitOfWork unitOfWork, IUsuarioActual usuarioActual, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _notificationService = notificationService;
    }

    public async Task<Resultado<SolicitudResumenDto>> HandleAsync(
        CambiarAsignacionCommand command, CancellationToken cancellationToken = default)
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

        Resultado? errorDeAutorizacion = ValidarAutorizacion(solicitud, command.UsuarioAsignadoId);

        if (errorDeAutorizacion is not null)
        {
            return Resultado.Fallido<SolicitudResumenDto>(errorDeAutorizacion.Error);
        }

        if (command.UsuarioAsignadoId is not null)
        {
            Usuario? destinatario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(
                command.UsuarioAsignadoId.Value, cancellationToken);

            bool esResponsableValido = destinatario is { Activo: true, Rol: RolUsuario.Administrador or RolUsuario.Analista };

            if (!esResponsableValido)
            {
                return Resultado.Fallido<SolicitudResumenDto>(
                    Error.Validacion(
                        "Solicitud.ResponsableInvalido",
                        "El responsable debe ser un usuario activo con rol Analista o Administrador."));
            }
        }

        solicitud.UsuarioAsignadoId = command.UsuarioAsignadoId;
        _unitOfWork.Solicitudes.Actualizar(solicitud);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        if (command.UsuarioAsignadoId is not null)
        {
            await _notificationService.NotificarAsync(
                new NotificacionSolicitada(
                    solicitud.Id,
                    command.UsuarioAsignadoId.Value,
                    $"Solicitud {solicitud.Codigo} asignada",
                    $"Se le asigno la solicitud {solicitud.Codigo}."),
                cancellationToken);
        }

        Solicitud actualizada = (await _unitOfWork.Solicitudes.ObtenerDetalleAsync(solicitud.Id, cancellationToken))!;

        return MapeosSolicitud.ASolicitudResumenDto(actualizada);
    }

    private Resultado? ValidarAutorizacion(Solicitud solicitud, int? usuarioAsignadoId)
    {
        if (_usuarioActual.Rol == RolUsuario.Administrador)
        {
            return null;
        }

        if (_usuarioActual.Rol != RolUsuario.Analista)
        {
            return Resultado.Fallido(Error.Prohibido("Solicitud.RolNoAutorizado", "Su rol no puede asignar solicitudes."));
        }

        bool intentaReclamarParaSiMismo = usuarioAsignadoId == _usuarioActual.Id;
        bool estabaSinAsignar = solicitud.UsuarioAsignadoId is null;

        if (!intentaReclamarParaSiMismo || !estabaSinAsignar)
        {
            return Resultado.Fallido(
                Error.Prohibido("Solicitud.RolNoAutorizado", "Solo puede reclamar solicitudes sin asignar, para si mismo."));
        }

        return null;
    }
}

using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.ActualizarSolicitud;

public class ActualizarSolicitudCommandHandler : ICommandHandler<ActualizarSolicitudCommand, Resultado<SolicitudResumenDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IProveedorFechaHora _proveedorFechaHora;

    public ActualizarSolicitudCommandHandler(
        IUnitOfWork unitOfWork, IUsuarioActual usuarioActual, IProveedorFechaHora proveedorFechaHora)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _proveedorFechaHora = proveedorFechaHora;
    }

    public async Task<Resultado<SolicitudResumenDto>> HandleAsync(
        ActualizarSolicitudCommand command, CancellationToken cancellationToken = default)
    {
        Resultado<AlcanceSolicitudes> alcanceResultado = AlcanceSolicitudesFactory.Calcular(_usuarioActual);
        if (alcanceResultado.EsFallido)
        {
            return Resultado.Fallido<SolicitudResumenDto>(alcanceResultado.Error);
        }

        Solicitud? solicitud = await _unitOfWork.Solicitudes.ObtenerParaCambioDeEstadoAsync(command.Id, cancellationToken);

        // Fuera del alcance: 404 (ADR-0012). Dentro del alcance pero sin permiso de edicion
        // (un Analista sobre su cola): 403, mas abajo.
        if (solicitud is null || !alcanceResultado.Valor.Incluye(solicitud))
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

        if (command.FechaCompromiso is not null && command.FechaCompromiso.Value.Date < _proveedorFechaHora.Ahora.Date)
        {
            return Resultado.Fallido<SolicitudResumenDto>(
                Error.Validacion("Solicitud.FechaCompromisoInvalida", "La fecha compromiso no puede ser una fecha pasada."));
        }

        int areaId = command.AreaId ?? solicitud.AreaId;
        int tipoSolicitudId = command.TipoSolicitudId ?? solicitud.TipoSolicitudId;
        int prioridadId = command.PrioridadId ?? solicitud.PrioridadId;

        Error? errorReferencias = await ValidadorReferenciasSolicitud.ValidarAsync(
            _unitOfWork, areaId, tipoSolicitudId, prioridadId, cancellationToken);

        if (errorReferencias is not null)
        {
            return Resultado.Fallido<SolicitudResumenDto>(errorReferencias);
        }

        solicitud.Titulo = command.Titulo ?? solicitud.Titulo;
        solicitud.Descripcion = command.Descripcion ?? solicitud.Descripcion;
        solicitud.TipoSolicitudId = tipoSolicitudId;
        solicitud.PrioridadId = prioridadId;
        solicitud.AreaId = areaId;
        solicitud.FechaCompromiso = command.FechaCompromiso ?? solicitud.FechaCompromiso;

        _unitOfWork.Solicitudes.Actualizar(solicitud);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        Solicitud actualizada = (await _unitOfWork.Solicitudes.ObtenerDetalleAsync(solicitud.Id, cancellationToken))!;

        return MapeosSolicitud.ASolicitudResumenDto(actualizada);
    }
}

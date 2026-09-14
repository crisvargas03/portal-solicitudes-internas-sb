using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Notificaciones;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearSolicitud;

public class CrearSolicitudCommandHandler : ICommandHandler<CrearSolicitudCommand, Resultado<SolicitudResumenDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IProveedorFechaHora _proveedorFechaHora;
    private readonly IGeneradorCodigoSolicitud _generadorCodigo;
    private readonly INotificationService _notificationService;

    public CrearSolicitudCommandHandler(
        IUnitOfWork unitOfWork,
        IUsuarioActual usuarioActual,
        IProveedorFechaHora proveedorFechaHora,
        IGeneradorCodigoSolicitud generadorCodigo,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _proveedorFechaHora = proveedorFechaHora;
        _generadorCodigo = generadorCodigo;
        _notificationService = notificationService;
    }

    public async Task<Resultado<SolicitudResumenDto>> HandleAsync(
        CrearSolicitudCommand command, CancellationToken cancellationToken = default)
    {
        if (_usuarioActual.Id is null)
        {
            return Resultado.Fallido<SolicitudResumenDto>(
                Error.NoAutorizado("Auth.NoAutenticado", "No hay una sesion activa."));
        }

        EstadoSolicitud? estadoInicial = await _unitOfWork.EstadosSolicitud.ObtenerPorCodigoAsync(
            CodigosEstadoSolicitud.REGISTRADA, cancellationToken);

        if (estadoInicial is null)
        {
            return Resultado.Fallido<SolicitudResumenDto>(
                Error.Falla("Solicitud.EstadoInicialNoConfigurado", "No se encontro el estado inicial del flujo."));
        }

        DateTime ahora = _proveedorFechaHora.Ahora;

        if (command.FechaCompromiso is not null && command.FechaCompromiso.Value.Date < ahora.Date)
        {
            return Resultado.Fallido<SolicitudResumenDto>(
                Error.Validacion("Solicitud.FechaCompromisoInvalida", "La fecha compromiso no puede ser una fecha pasada."));
        }

        Error? errorReferencias = await ValidadorReferenciasSolicitud.ValidarAsync(
            _unitOfWork, command.AreaId, command.TipoSolicitudId, command.PrioridadId, cancellationToken);

        if (errorReferencias is not null)
        {
            return Resultado.Fallido<SolicitudResumenDto>(errorReferencias);
        }

        int usuarioSolicitanteId = _usuarioActual.Id.Value;

        await _unitOfWork.IniciarTransaccionAsync(cancellationToken);

        int solicitudId;
        string codigo;

        try
        {
            codigo = await _generadorCodigo.GenerarAsync(ahora.Year, cancellationToken);

            Solicitud solicitud = new()
            {
                Codigo = codigo,
                Titulo = command.Titulo,
                Descripcion = command.Descripcion,
                FechaCreacion = ahora,
                FechaCompromiso = command.FechaCompromiso,
                PrioridadId = command.PrioridadId,
                EstadoId = estadoInicial.Id,
                AreaId = command.AreaId,
                TipoSolicitudId = command.TipoSolicitudId,
                UsuarioSolicitanteId = usuarioSolicitanteId
            };

            await _unitOfWork.Solicitudes.AgregarAsync(solicitud, cancellationToken);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);

            HistorialEstado historialInicial = new()
            {
                SolicitudId = solicitud.Id,
                EstadoAnteriorId = null,
                EstadoNuevoId = estadoInicial.Id,
                UsuarioId = usuarioSolicitanteId,
                Comentario = null,
                Fecha = ahora
            };

            await _unitOfWork.HistorialEstados.AgregarAsync(historialInicial, cancellationToken);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);

            solicitudId = solicitud.Id;
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

        await _notificationService.NotificarAsync(
            new NotificacionSolicitada(
                solicitudId,
                usuarioSolicitanteId,
                $"Solicitud {codigo} registrada",
                $"Se registro la solicitud {codigo}."),
            cancellationToken);

        Solicitud creada = (await _unitOfWork.Solicitudes.ObtenerDetalleAsync(solicitudId, cancellationToken))!;

        return MapeosSolicitud.ASolicitudResumenDto(creada);
    }
}

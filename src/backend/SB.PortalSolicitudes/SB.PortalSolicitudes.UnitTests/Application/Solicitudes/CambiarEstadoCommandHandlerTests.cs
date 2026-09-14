using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Notificaciones;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CambiarEstado;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Solicitudes;

/// <summary>
/// Maquina de estados (ADR-0005) con sus reglas de negocio: comentario obligatorio
/// (ADR-0001), roles por transicion (reapertura solo Administrador/Analista) y alcance
/// por rol (ADR-0012, amendada). La tabla de transiciones se simula: el handler solo
/// debe obedecer lo que el repositorio devuelve.
/// </summary>
public class CambiarEstadoCommandHandlerTests
{
    private const string COMENTARIO = "Comentario de la transicion";

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();
    private readonly ITransicionPermitidaRepository _transiciones = Substitute.For<ITransicionPermitidaRepository>();
    private readonly IHistorialEstadoRepository _historial = Substitute.For<IHistorialEstadoRepository>();
    private readonly IProveedorFechaHora _proveedorFechaHora = Substitute.For<IProveedorFechaHora>();
    private readonly INotificationService _notificationService = Substitute.For<INotificationService>();

    public CambiarEstadoCommandHandlerTests()
    {
        _unitOfWork.Solicitudes.Returns(_solicitudes);
        _unitOfWork.TransicionesPermitidas.Returns(_transiciones);
        _unitOfWork.HistorialEstados.Returns(_historial);
        _proveedorFechaHora.Ahora.Returns(DatosPrueba.AHORA);
    }

    [Fact]
    public async Task HandleAsync_TransicionPermitidaConComentario_CambiaEstadoYRegistraHistorial()
    {
        Solicitud solicitud = PrepararSolicitud(DatosPrueba.EnProgreso(), usuarioAsignadoId: DatosPrueba.ID_ANALISTA_UNO);
        PrepararTransicion(DatosPrueba.EnProgreso(), DatosPrueba.Resuelta(), requiereComentario: true, RolUsuario.Analista);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(
            DatosPrueba.AnalistaUno(), DatosPrueba.ID_ESTADO_RESUELTA, COMENTARIO);

        Assert.True(resultado.EsExitoso);
        Assert.Equal(DatosPrueba.ID_ESTADO_RESUELTA, solicitud.EstadoId);

        await _historial.Received(1).AgregarAsync(
            Arg.Is<HistorialEstado>(historial =>
                historial.SolicitudId == DatosPrueba.ID_SOLICITUD
                && historial.EstadoAnteriorId == DatosPrueba.ID_ESTADO_EN_PROGRESO
                && historial.EstadoNuevoId == DatosPrueba.ID_ESTADO_RESUELTA
                && historial.UsuarioId == DatosPrueba.ID_ANALISTA_UNO
                && historial.Comentario == COMENTARIO
                && historial.Fecha == DatosPrueba.AHORA),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).ConfirmarTransaccionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_CambioExitoso_NotificaAlSolicitanteYAlResponsable()
    {
        PrepararSolicitud(DatosPrueba.Registrada(), usuarioAsignadoId: DatosPrueba.ID_ANALISTA_UNO);
        PrepararTransicion(DatosPrueba.Registrada(), DatosPrueba.EnAnalisis(), requiereComentario: false, RolUsuario.Analista);

        await Ejecutar(DatosPrueba.AnalistaUno(), DatosPrueba.ID_ESTADO_EN_ANALISIS, comentario: null);

        await _notificationService.Received(1).NotificarAsync(
            Arg.Is<NotificacionSolicitada>(notificacion => notificacion.UsuarioDestinoId == DatosPrueba.ID_SOLICITANTE_UNO),
            Arg.Any<CancellationToken>());
        await _notificationService.Received(1).NotificarAsync(
            Arg.Is<NotificacionSolicitada>(notificacion => notificacion.UsuarioDestinoId == DatosPrueba.ID_ANALISTA_UNO),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_TransicionQueRequiereComentarioSinComentario_DevuelveValidacionSinModificar()
    {
        Solicitud solicitud = PrepararSolicitud(DatosPrueba.EnProgreso(), usuarioAsignadoId: DatosPrueba.ID_ANALISTA_UNO);
        PrepararTransicion(DatosPrueba.EnProgreso(), DatosPrueba.Resuelta(), requiereComentario: true, RolUsuario.Analista);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(
            DatosPrueba.AnalistaUno(), DatosPrueba.ID_ESTADO_RESUELTA, comentario: "   ");

        Assert.Equal(TipoError.Validacion, resultado.Error.Tipo);
        Assert.Equal("Solicitud.ComentarioRequerido", resultado.Error.Codigo);
        Assert.Equal(DatosPrueba.ID_ESTADO_EN_PROGRESO, solicitud.EstadoId);
        await AfirmarQueNoSeModificoNada();
    }

    [Fact]
    public async Task HandleAsync_TransicionNoDeclarada_DevuelveConflicto()
    {
        // No existe fila REGISTRADA -> CERRADA: no se puede cerrar sin pasar por RESUELTA (ADR-0002).
        PrepararSolicitud(DatosPrueba.Registrada(), usuarioAsignadoId: DatosPrueba.ID_ANALISTA_UNO);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(
            DatosPrueba.Administrador(), DatosPrueba.ID_ESTADO_CERRADA, COMENTARIO);

        Assert.Equal(TipoError.Conflicto, resultado.Error.Tipo);
        Assert.Equal("Solicitud.TransicionNoPermitida", resultado.Error.Codigo);
        await AfirmarQueNoSeModificoNada();
    }

    [Fact]
    public async Task HandleAsync_SolicitanteReabreSuSolicitudCerrada_DevuelveProhibido()
    {
        PrepararSolicitud(DatosPrueba.Cerrada(), usuarioSolicitanteId: DatosPrueba.ID_SOLICITANTE_UNO);
        PrepararTransicion(
            DatosPrueba.Cerrada(), DatosPrueba.EnAnalisis(), requiereComentario: true,
            RolUsuario.Administrador, RolUsuario.Analista);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(
            DatosPrueba.SolicitanteUno(), DatosPrueba.ID_ESTADO_EN_ANALISIS, COMENTARIO);

        Assert.Equal(TipoError.Prohibido, resultado.Error.Tipo);
        await AfirmarQueNoSeModificoNada();
    }

    [Fact]
    public async Task HandleAsync_AnalistaReabreSolicitudCerradaAsignadaASiMismo_Permitido()
    {
        PrepararSolicitud(DatosPrueba.Cerrada(), usuarioAsignadoId: DatosPrueba.ID_ANALISTA_UNO);
        PrepararTransicion(
            DatosPrueba.Cerrada(), DatosPrueba.EnAnalisis(), requiereComentario: true,
            RolUsuario.Administrador, RolUsuario.Analista);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(
            DatosPrueba.AnalistaUno(), DatosPrueba.ID_ESTADO_EN_ANALISIS, COMENTARIO);

        Assert.True(resultado.EsExitoso);
    }

    [Fact]
    public async Task HandleAsync_SolicitanteCierraSolicitudDeOtroSolicitante_DevuelveNoEncontradaSinModificar()
    {
        // Regresion: el rol Solicitante SI esta autorizado para RESUELTA -> CERRADA, asi que sin
        // el chequeo de alcance podia cerrar la solicitud de otra persona (ADR-0012, amendada).
        Solicitud solicitud = PrepararSolicitud(DatosPrueba.Resuelta(), usuarioSolicitanteId: DatosPrueba.ID_SOLICITANTE_DOS);
        PrepararTransicion(
            DatosPrueba.Resuelta(), DatosPrueba.Cerrada(), requiereComentario: false,
            RolUsuario.Administrador, RolUsuario.Analista, RolUsuario.Solicitante);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(
            DatosPrueba.SolicitanteUno(), DatosPrueba.ID_ESTADO_CERRADA, comentario: null);

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
        Assert.Equal(DatosPrueba.ID_ESTADO_RESUELTA, solicitud.EstadoId);
        await AfirmarQueNoSeModificoNada();
    }

    [Fact]
    public async Task HandleAsync_AnalistaSobreSolicitudAsignadaAOtroAnalista_DevuelveNoEncontrada()
    {
        PrepararSolicitud(DatosPrueba.EnProgreso(), usuarioAsignadoId: DatosPrueba.ID_ANALISTA_DOS);
        PrepararTransicion(DatosPrueba.EnProgreso(), DatosPrueba.Resuelta(), requiereComentario: true, RolUsuario.Analista);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(
            DatosPrueba.AnalistaUno(), DatosPrueba.ID_ESTADO_RESUELTA, COMENTARIO);

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
        await AfirmarQueNoSeModificoNada();
    }

    [Fact]
    public async Task HandleAsync_SolicitudInexistente_DevuelveNoEncontrada()
    {
        Resultado<SolicitudResumenDto> resultado = await Ejecutar(
            DatosPrueba.Administrador(), DatosPrueba.ID_ESTADO_EN_ANALISIS, comentario: null);

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
    }

    [Fact]
    public async Task HandleAsync_SinSesion_DevuelveNoAutorizado()
    {
        Resultado<SolicitudResumenDto> resultado = await Ejecutar(
            DatosPrueba.UsuarioActual(null, null), DatosPrueba.ID_ESTADO_EN_ANALISIS, comentario: null);

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await _solicitudes.DidNotReceiveWithAnyArgs().ObtenerParaCambioDeEstadoAsync(default);
    }

    [Fact]
    public async Task HandleAsync_RolIndeterminado_FallaCerradoSinConsultarLaSolicitud()
    {
        Resultado<SolicitudResumenDto> resultado = await Ejecutar(
            DatosPrueba.ConRolDesconocido(), DatosPrueba.ID_ESTADO_EN_ANALISIS, comentario: null);

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await _solicitudes.DidNotReceiveWithAnyArgs().ObtenerParaCambioDeEstadoAsync(default);
    }

    [Fact]
    public async Task HandleAsync_FallaAlRegistrarHistorial_RevierteLaTransaccionYNoNotifica()
    {
        PrepararSolicitud(DatosPrueba.Registrada(), usuarioAsignadoId: DatosPrueba.ID_ANALISTA_UNO);
        PrepararTransicion(DatosPrueba.Registrada(), DatosPrueba.EnAnalisis(), requiereComentario: false, RolUsuario.Analista);
        _historial.AgregarAsync(Arg.Any<HistorialEstado>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("Fallo de base de datos"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Ejecutar(DatosPrueba.AnalistaUno(), DatosPrueba.ID_ESTADO_EN_ANALISIS, comentario: null));

        await _unitOfWork.Received(1).RevertirTransaccionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceiveWithAnyArgs().ConfirmarTransaccionAsync(default);
        await _notificationService.DidNotReceiveWithAnyArgs().NotificarAsync(default!, default);
    }

    [Fact]
    public async Task HandleAsync_ResponsableEsTambienElSolicitante_NotificaUnaSolaVez()
    {
        PrepararSolicitud(
            DatosPrueba.Registrada(), usuarioSolicitanteId: DatosPrueba.ID_ADMINISTRADOR, usuarioAsignadoId: DatosPrueba.ID_ADMINISTRADOR);
        PrepararTransicion(DatosPrueba.Registrada(), DatosPrueba.EnAnalisis(), requiereComentario: false, RolUsuario.Administrador);

        await Ejecutar(DatosPrueba.Administrador(), DatosPrueba.ID_ESTADO_EN_ANALISIS, comentario: null);

        await _notificationService.Received(1).NotificarAsync(Arg.Any<NotificacionSolicitada>(), Arg.Any<CancellationToken>());
    }

    private Solicitud PrepararSolicitud(
        EstadoSolicitud estado, int usuarioSolicitanteId = DatosPrueba.ID_SOLICITANTE_UNO, int? usuarioAsignadoId = null)
    {
        Solicitud solicitud = DatosPrueba.Solicitud(estado, usuarioSolicitanteId, usuarioAsignadoId);

        _solicitudes.ObtenerParaCambioDeEstadoAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>()).Returns(solicitud);
        _solicitudes.ObtenerDetalleAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>()).Returns(solicitud);

        return solicitud;
    }

    private void PrepararTransicion(
        EstadoSolicitud origen, EstadoSolicitud destino, bool requiereComentario, params RolUsuario[] roles)
    {
        _transiciones.ObtenerAsync(origen.Id, destino.Id, Arg.Any<CancellationToken>())
            .Returns(DatosPrueba.Transicion(origen, destino, requiereComentario, roles));
    }

    private Task<Resultado<SolicitudResumenDto>> Ejecutar(IUsuarioActual usuarioActual, int estadoDestinoId, string? comentario)
    {
        CambiarEstadoCommandHandler handler = new(_unitOfWork, usuarioActual, _proveedorFechaHora, _notificationService);

        return handler.HandleAsync(new CambiarEstadoCommand(DatosPrueba.ID_SOLICITUD, estadoDestinoId, comentario));
    }

    private async Task AfirmarQueNoSeModificoNada()
    {
        await _unitOfWork.DidNotReceiveWithAnyArgs().IniciarTransaccionAsync(default);
        await _historial.DidNotReceiveWithAnyArgs().AgregarAsync(default!, default);
        await _notificationService.DidNotReceiveWithAnyArgs().NotificarAsync(default!, default);
    }
}

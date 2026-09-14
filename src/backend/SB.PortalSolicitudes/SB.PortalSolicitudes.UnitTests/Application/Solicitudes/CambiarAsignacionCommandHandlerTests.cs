using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Notificaciones;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CambiarAsignacion;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Solicitudes;

/// <summary>
/// Reglas de asignacion de ADR-0012: Administrador asigna libremente; Analista solo reclama
/// para si mismo una solicitud sin responsable; el responsable debe ser un Analista o
/// Administrador activo; asignar notifica al nuevo responsable.
/// </summary>
public class CambiarAsignacionCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly INotificationService _notificationService = Substitute.For<INotificationService>();

    public CambiarAsignacionCommandHandlerTests()
    {
        _unitOfWork.Solicitudes.Returns(_solicitudes);
        _unitOfWork.Usuarios.Returns(_usuarios);

        PrepararUsuario(DatosPrueba.Usuario(DatosPrueba.ID_ADMINISTRADOR, RolUsuario.Administrador));
        PrepararUsuario(DatosPrueba.Usuario(DatosPrueba.ID_ANALISTA_UNO, RolUsuario.Analista));
        PrepararUsuario(DatosPrueba.Usuario(DatosPrueba.ID_ANALISTA_DOS, RolUsuario.Analista));
        PrepararUsuario(DatosPrueba.Usuario(DatosPrueba.ID_SOLICITANTE_UNO, RolUsuario.Solicitante));
    }

    [Fact]
    public async Task HandleAsync_AnalistaReclamaSolicitudSinAsignar_AsignaYNotifica()
    {
        Solicitud solicitud = PrepararSolicitud(usuarioAsignadoId: null);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.AnalistaUno(), DatosPrueba.ID_ANALISTA_UNO);

        Assert.True(resultado.EsExitoso);
        Assert.Equal(DatosPrueba.ID_ANALISTA_UNO, solicitud.UsuarioAsignadoId);
        await _unitOfWork.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
        await _notificationService.Received(1).NotificarAsync(
            Arg.Is<NotificacionSolicitada>(notificacion => notificacion.UsuarioDestinoId == DatosPrueba.ID_ANALISTA_UNO),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_AnalistaReasignaSolicitudYaAsignada_DevuelveProhibido()
    {
        Solicitud solicitud = PrepararSolicitud(usuarioAsignadoId: DatosPrueba.ID_ANALISTA_DOS);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.AnalistaUno(), DatosPrueba.ID_ANALISTA_UNO);

        Assert.Equal(TipoError.Prohibido, resultado.Error.Tipo);
        Assert.Equal(DatosPrueba.ID_ANALISTA_DOS, solicitud.UsuarioAsignadoId);
        await AfirmarQueNoSeGuardoNiNotifico();
    }

    [Fact]
    public async Task HandleAsync_AnalistaAsignaSolicitudSinAsignarAOtraPersona_DevuelveProhibido()
    {
        PrepararSolicitud(usuarioAsignadoId: null);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.AnalistaUno(), DatosPrueba.ID_ANALISTA_DOS);

        Assert.Equal(TipoError.Prohibido, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardoNiNotifico();
    }

    [Fact]
    public async Task HandleAsync_SolicitanteIntentaAsignar_DevuelveProhibido()
    {
        PrepararSolicitud(usuarioAsignadoId: null);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.SolicitanteUno(), DatosPrueba.ID_ANALISTA_UNO);

        Assert.Equal(TipoError.Prohibido, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardoNiNotifico();
    }

    [Fact]
    public async Task HandleAsync_AdministradorReasigna_AsignaAlNuevoResponsable()
    {
        Solicitud solicitud = PrepararSolicitud(usuarioAsignadoId: DatosPrueba.ID_ANALISTA_UNO);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.Administrador(), DatosPrueba.ID_ANALISTA_DOS);

        Assert.True(resultado.EsExitoso);
        Assert.Equal(DatosPrueba.ID_ANALISTA_DOS, solicitud.UsuarioAsignadoId);
    }

    [Fact]
    public async Task HandleAsync_AdministradorAsignaAUnSolicitante_DevuelveValidacion()
    {
        PrepararSolicitud(usuarioAsignadoId: null);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.Administrador(), DatosPrueba.ID_SOLICITANTE_UNO);

        Assert.Equal(TipoError.Validacion, resultado.Error.Tipo);
        Assert.Equal("Solicitud.ResponsableInvalido", resultado.Error.Codigo);
        await AfirmarQueNoSeGuardoNiNotifico();
    }

    [Fact]
    public async Task HandleAsync_AdministradorAsignaAUnAnalistaInactivo_DevuelveValidacion()
    {
        PrepararUsuario(DatosPrueba.Usuario(DatosPrueba.ID_ANALISTA_DOS, RolUsuario.Analista, activo: false));
        PrepararSolicitud(usuarioAsignadoId: null);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.Administrador(), DatosPrueba.ID_ANALISTA_DOS);

        Assert.Equal(TipoError.Validacion, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardoNiNotifico();
    }

    [Fact]
    public async Task HandleAsync_AdministradorDesasigna_GuardaSinNotificar()
    {
        Solicitud solicitud = PrepararSolicitud(usuarioAsignadoId: DatosPrueba.ID_ANALISTA_UNO);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.Administrador(), usuarioAsignadoId: null);

        Assert.True(resultado.EsExitoso);
        Assert.Null(solicitud.UsuarioAsignadoId);
        await _notificationService.DidNotReceiveWithAnyArgs().NotificarAsync(default!, default);
    }

    [Fact]
    public async Task HandleAsync_SolicitudInexistente_DevuelveNoEncontrada()
    {
        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.Administrador(), DatosPrueba.ID_ANALISTA_UNO);

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardoNiNotifico();
    }

    [Fact]
    public async Task HandleAsync_SinSesion_DevuelveNoAutorizadoSinConsultar()
    {
        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.UsuarioActual(null, null), DatosPrueba.ID_ANALISTA_UNO);

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await _solicitudes.DidNotReceiveWithAnyArgs().ObtenerParaCambioDeEstadoAsync(default);
    }

    private void PrepararUsuario(Usuario usuario)
    {
        _usuarios.ObtenerPorIdAsync(usuario.Id, Arg.Any<CancellationToken>()).Returns(usuario);
    }

    private Solicitud PrepararSolicitud(int? usuarioAsignadoId)
    {
        Solicitud solicitud = DatosPrueba.Solicitud(DatosPrueba.EnAnalisis(), usuarioAsignadoId: usuarioAsignadoId);

        _solicitudes.ObtenerParaCambioDeEstadoAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>()).Returns(solicitud);
        _solicitudes.ObtenerDetalleAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>()).Returns(solicitud);

        return solicitud;
    }

    private Task<Resultado<SolicitudResumenDto>> Ejecutar(IUsuarioActual usuarioActual, int? usuarioAsignadoId)
    {
        CambiarAsignacionCommandHandler handler = new(_unitOfWork, usuarioActual, _notificationService);

        return handler.HandleAsync(new CambiarAsignacionCommand(DatosPrueba.ID_SOLICITUD, usuarioAsignadoId));
    }

    private async Task AfirmarQueNoSeGuardoNiNotifico()
    {
        await _unitOfWork.DidNotReceiveWithAnyArgs().GuardarCambiosAsync(default);
        await _notificationService.DidNotReceiveWithAnyArgs().NotificarAsync(default!, default);
    }
}

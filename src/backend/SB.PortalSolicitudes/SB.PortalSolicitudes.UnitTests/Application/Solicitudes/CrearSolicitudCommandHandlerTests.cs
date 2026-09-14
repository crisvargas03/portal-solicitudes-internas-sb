using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Notificaciones;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearSolicitud;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Solicitudes;

/// <summary>
/// Alta de solicitud: nace en REGISTRADA a nombre del usuario del token (nunca del cuerpo),
/// con el codigo del generador (ADR-0011), su primer paso de historial y la notificacion
/// de creacion.
/// </summary>
public class CrearSolicitudCommandHandlerTests
{
    private const string CODIGO_GENERADO = "SOL-2026-0042";

    private static readonly CrearSolicitudCommand COMANDO = new(
        "Falla de impresora", "La impresora del piso 3 no responde.",
        DatosPrueba.ID_CATALOGO, DatosPrueba.ID_CATALOGO, DatosPrueba.ID_CATALOGO, FechaCompromiso: null);

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();
    private readonly IEstadoSolicitudRepository _estados = Substitute.For<IEstadoSolicitudRepository>();
    private readonly IHistorialEstadoRepository _historial = Substitute.For<IHistorialEstadoRepository>();
    private readonly IProveedorFechaHora _proveedorFechaHora = Substitute.For<IProveedorFechaHora>();
    private readonly IGeneradorCodigoSolicitud _generadorCodigo = Substitute.For<IGeneradorCodigoSolicitud>();
    private readonly INotificationService _notificationService = Substitute.For<INotificationService>();

    private Solicitud? _solicitudAgregada;

    public CrearSolicitudCommandHandlerTests()
    {
        _unitOfWork.Solicitudes.Returns(_solicitudes);
        _unitOfWork.EstadosSolicitud.Returns(_estados);
        _unitOfWork.HistorialEstados.Returns(_historial);
        _proveedorFechaHora.Ahora.Returns(DatosPrueba.AHORA);
        DatosPrueba.PrepararCatalogosActivos(_unitOfWork);
        _generadorCodigo.GenerarAsync(DatosPrueba.AHORA.Year, Arg.Any<CancellationToken>()).Returns(CODIGO_GENERADO);

        _estados.ObtenerPorCodigoAsync(CodigosEstadoSolicitud.REGISTRADA, Arg.Any<CancellationToken>())
            .Returns(DatosPrueba.Registrada());

        // Simula la identidad que asigna la base de datos al guardar.
        _solicitudes
            .When(repositorio => repositorio.AgregarAsync(Arg.Any<Solicitud>(), Arg.Any<CancellationToken>()))
            .Do(llamada =>
            {
                _solicitudAgregada = llamada.Arg<Solicitud>();
                _solicitudAgregada.Id = DatosPrueba.ID_SOLICITUD;
            });

        _solicitudes.ObtenerDetalleAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>())
            .Returns(_ => DatosPrueba.Solicitud(DatosPrueba.Registrada()));
    }

    [Fact]
    public async Task HandleAsync_ComandoValido_CreaEnRegistradaConCodigoGeneradoANombreDelUsuarioActual()
    {
        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.SolicitanteUno());

        Assert.True(resultado.EsExitoso);
        Assert.NotNull(_solicitudAgregada);
        Assert.Equal(CODIGO_GENERADO, _solicitudAgregada.Codigo);
        Assert.Equal(DatosPrueba.ID_ESTADO_REGISTRADA, _solicitudAgregada.EstadoId);
        Assert.Equal(DatosPrueba.ID_SOLICITANTE_UNO, _solicitudAgregada.UsuarioSolicitanteId);
        Assert.Null(_solicitudAgregada.UsuarioAsignadoId);
        Assert.Equal(DatosPrueba.AHORA, _solicitudAgregada.FechaCreacion);
        await _unitOfWork.Received(1).ConfirmarTransaccionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ComandoValido_RegistraElPrimerPasoDelHistorialSinEstadoAnterior()
    {
        await Ejecutar(DatosPrueba.SolicitanteUno());

        await _historial.Received(1).AgregarAsync(
            Arg.Is<HistorialEstado>(historial =>
                historial.SolicitudId == DatosPrueba.ID_SOLICITUD
                && historial.EstadoAnteriorId == null
                && historial.EstadoNuevoId == DatosPrueba.ID_ESTADO_REGISTRADA
                && historial.UsuarioId == DatosPrueba.ID_SOLICITANTE_UNO),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ComandoValido_NotificaLaCreacionAlSolicitanteDespuesDeConfirmar()
    {
        await Ejecutar(DatosPrueba.SolicitanteUno());

        Received.InOrder(() =>
        {
            _unitOfWork.ConfirmarTransaccionAsync(Arg.Any<CancellationToken>());
            _notificationService.NotificarAsync(
                Arg.Is<NotificacionSolicitada>(notificacion =>
                    notificacion.UsuarioDestinoId == DatosPrueba.ID_SOLICITANTE_UNO
                    && notificacion.Asunto.Contains(CODIGO_GENERADO)),
                Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task HandleAsync_FallaAlGuardar_RevierteLaTransaccionYNoNotifica()
    {
        _unitOfWork.GuardarCambiosAsync(Arg.Any<CancellationToken>())
            .Returns<int>(_ => throw new InvalidOperationException("Fallo de base de datos"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Ejecutar(DatosPrueba.SolicitanteUno()));

        await _unitOfWork.Received(1).RevertirTransaccionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceiveWithAnyArgs().ConfirmarTransaccionAsync(default);
        await _notificationService.DidNotReceiveWithAnyArgs().NotificarAsync(default!, default);
    }

    [Fact]
    public async Task HandleAsync_EstadoInicialNoConfigurado_DevuelveFallaSinAbrirTransaccion()
    {
        _estados.ObtenerPorCodigoAsync(CodigosEstadoSolicitud.REGISTRADA, Arg.Any<CancellationToken>())
            .Returns((EstadoSolicitud?)null);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.SolicitanteUno());

        Assert.Equal(TipoError.Falla, resultado.Error.Tipo);
        await _unitOfWork.DidNotReceiveWithAnyArgs().IniciarTransaccionAsync(default);
    }

    [Fact]
    public async Task HandleAsync_SinSesion_DevuelveNoAutorizado()
    {
        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.UsuarioActual(null, null));

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await _solicitudes.DidNotReceiveWithAnyArgs().AgregarAsync(default!, default);
    }

    [Fact]
    public async Task HandleAsync_AreaInexistente_DevuelveValidacionSinAbrirTransaccion()
    {
        _unitOfWork.Areas.ObtenerPorIdAsync(DatosPrueba.ID_CATALOGO, Arg.Any<CancellationToken>())
            .Returns((Area?)null);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.SolicitanteUno());

        Assert.Equal(TipoError.Validacion, resultado.Error.Tipo);
        Assert.Equal("Solicitud.AreaInvalida", resultado.Error.Codigo);
        await _unitOfWork.DidNotReceiveWithAnyArgs().IniciarTransaccionAsync(default);
    }

    [Fact]
    public async Task HandleAsync_TipoSolicitudInactivo_DevuelveValidacion()
    {
        _unitOfWork.TiposSolicitud.ObtenerPorIdAsync(DatosPrueba.ID_CATALOGO, Arg.Any<CancellationToken>())
            .Returns(new TipoSolicitud { Id = DatosPrueba.ID_CATALOGO, Nombre = "Tipo", Activo = false });

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.SolicitanteUno());

        Assert.Equal(TipoError.Validacion, resultado.Error.Tipo);
        Assert.Equal("Solicitud.TipoSolicitudInvalido", resultado.Error.Codigo);
    }

    [Fact]
    public async Task HandleAsync_PrioridadInexistente_DevuelveValidacion()
    {
        _unitOfWork.Prioridades.ObtenerPorIdAsync(DatosPrueba.ID_CATALOGO, Arg.Any<CancellationToken>())
            .Returns((Prioridad?)null);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.SolicitanteUno());

        Assert.Equal(TipoError.Validacion, resultado.Error.Tipo);
        Assert.Equal("Solicitud.PrioridadInvalida", resultado.Error.Codigo);
    }

    [Fact]
    public async Task HandleAsync_FechaCompromisoEnElPasado_DevuelveValidacionSinAbrirTransaccion()
    {
        CrearSolicitudCommand comando = COMANDO with { FechaCompromiso = DatosPrueba.AHORA.Date.AddDays(-1) };

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.SolicitanteUno(), comando);

        Assert.Equal(TipoError.Validacion, resultado.Error.Tipo);
        Assert.Equal("Solicitud.FechaCompromisoInvalida", resultado.Error.Codigo);
        await _unitOfWork.DidNotReceiveWithAnyArgs().IniciarTransaccionAsync(default);
    }

    private Task<Resultado<SolicitudResumenDto>> Ejecutar(IUsuarioActual usuarioActual, CrearSolicitudCommand? comando = null)
    {
        CrearSolicitudCommandHandler handler = new(
            _unitOfWork, usuarioActual, _proveedorFechaHora, _generadorCodigo, _notificationService);

        return handler.HandleAsync(comando ?? COMANDO);
    }
}

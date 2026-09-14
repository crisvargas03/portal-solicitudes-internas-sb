using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Dashboard.Dtos;
using SB.PortalSolicitudes.Application.Features.Dashboard.Queries.ObtenerResumenDashboard;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Dashboard;

/// <summary>
/// Metricas del tablero (seccion 5 del requerimiento): conteo por estado y por prioridad,
/// vencidas segun la hora actual y recientes, todo dentro del mismo alcance por rol que el
/// listado (ADR-0012, ADR-0027).
/// </summary>
public class ObtenerResumenDashboardQueryHandlerTests
{
    private const int ID_PRIORIDAD_BAJA = 1;
    private const int ID_PRIORIDAD_ALTA = 3;
    private const int NIVEL_BAJA = 1;
    private const int NIVEL_ALTA = 3;
    private const int CANTIDAD_REGISTRADAS = 4;
    private const int CANTIDAD_EN_PROGRESO = 2;
    private const int CANTIDAD_ALTA = 6;
    private const int TOTAL_VENCIDAS = 1;
    private const int TOTAL_SIN_ASIGNAR = 3;
    private const int CANTIDAD_RECIENTES_ESPERADA = 5;

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();
    private readonly IEstadoSolicitudRepository _estados = Substitute.For<IEstadoSolicitudRepository>();
    private readonly IPrioridadRepository _prioridades = Substitute.For<IPrioridadRepository>();
    private readonly IProveedorFechaHora _proveedorFechaHora = Substitute.For<IProveedorFechaHora>();

    private CriterioDashboard? _criterioRecibido;

    public ObtenerResumenDashboardQueryHandlerTests()
    {
        _unitOfWork.Solicitudes.Returns(_solicitudes);
        _unitOfWork.EstadosSolicitud.Returns(_estados);
        _unitOfWork.Prioridades.Returns(_prioridades);
        _proveedorFechaHora.Ahora.Returns(DatosPrueba.AHORA);

        _estados.ObtenerActivosOrdenadosAsync(Arg.Any<CancellationToken>())
            .Returns([DatosPrueba.Registrada(), DatosPrueba.EnProgreso(), DatosPrueba.Cerrada()]);
        _prioridades.ObtenerActivasOrdenadasPorNivelAsync(Arg.Any<CancellationToken>())
            .Returns(
            [
                new Prioridad { Id = ID_PRIORIDAD_BAJA, Nombre = "Baja", Nivel = NIVEL_BAJA },
                new Prioridad { Id = ID_PRIORIDAD_ALTA, Nombre = "Alta", Nivel = NIVEL_ALTA }
            ]);

        _solicitudes.ContarPorCodigoDeEstadoAsync(Arg.Do<CriterioDashboard>(criterio => _criterioRecibido = criterio), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int>
            {
                [CodigosEstadoSolicitud.REGISTRADA] = CANTIDAD_REGISTRADAS,
                [CodigosEstadoSolicitud.EN_PROGRESO] = CANTIDAD_EN_PROGRESO
            });
        _solicitudes.ContarPorPrioridadAsync(Arg.Any<CriterioDashboard>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<int, int> { [ID_PRIORIDAD_ALTA] = CANTIDAD_ALTA });
        _solicitudes.ContarVencidasAsync(DatosPrueba.AHORA, Arg.Any<CriterioDashboard>(), Arg.Any<CancellationToken>())
            .Returns(TOTAL_VENCIDAS);
        _solicitudes.ContarSinAsignarAsync(Arg.Any<AlcanceSolicitudes>(), Arg.Any<CancellationToken>())
            .Returns(TOTAL_SIN_ASIGNAR);
        _solicitudes.ObtenerRecientesAsync(Arg.Any<int>(), Arg.Any<CriterioDashboard>(), Arg.Any<CancellationToken>())
            .Returns([DatosPrueba.Solicitud(DatosPrueba.Registrada())]);
    }

    [Fact]
    public async Task HandleAsync_ConteoPorEstado_IncluyeTodosLosEstadosActivosEnOrdenConCeroSiNoHay()
    {
        Resultado<ResumenDashboardDto> resultado = await Ejecutar(DatosPrueba.Administrador());

        Assert.Equal(
            [
                (CodigosEstadoSolicitud.REGISTRADA, CANTIDAD_REGISTRADAS),
                (CodigosEstadoSolicitud.EN_PROGRESO, CANTIDAD_EN_PROGRESO),
                (CodigosEstadoSolicitud.CERRADA, 0)
            ],
            resultado.Valor.PorEstado.Select(conteo => (conteo.CodigoEstado, conteo.Cantidad)));
    }

    [Fact]
    public async Task HandleAsync_ConteoPorPrioridad_IncluyeTodasLasPrioridadesActivasConCeroSiNoHay()
    {
        Resultado<ResumenDashboardDto> resultado = await Ejecutar(DatosPrueba.Administrador());

        Assert.Equal(
            [(ID_PRIORIDAD_BAJA, 0), (ID_PRIORIDAD_ALTA, CANTIDAD_ALTA)],
            resultado.Valor.PorPrioridad.Select(conteo => (conteo.PrioridadId, conteo.Cantidad)));
    }

    [Fact]
    public async Task HandleAsync_Totales_SumanLosConteosYUsanLaHoraActualParaVencidas()
    {
        Resultado<ResumenDashboardDto> resultado = await Ejecutar(DatosPrueba.Administrador());

        Assert.Equal(CANTIDAD_REGISTRADAS + CANTIDAD_EN_PROGRESO, resultado.Valor.TotalSolicitudes);
        Assert.Equal(TOTAL_VENCIDAS, resultado.Valor.TotalVencidas);
        Assert.Equal(TOTAL_SIN_ASIGNAR, resultado.Valor.TotalSinAsignar);
        Assert.Single(resultado.Valor.Recientes);
        await _solicitudes.Received(1).ObtenerRecientesAsync(
            CANTIDAD_RECIENTES_ESPERADA, Arg.Any<CriterioDashboard>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Solicitante_AplicaSuAlcanceATodasLasMetricas()
    {
        await Ejecutar(DatosPrueba.SolicitanteUno());

        Assert.Equal(new AlcanceSolicitudes(DatosPrueba.ID_SOLICITANTE_UNO, null, false), _criterioRecibido!.Alcance);
    }

    [Fact]
    public async Task HandleAsync_AnalistaConAsignacionAsignadas_CortaALasSuyasDentroDeSuAlcance()
    {
        await Ejecutar(DatosPrueba.AnalistaUno(), FiltroAsignacion.Asignadas);

        Assert.Equal(new AlcanceSolicitudes(null, DatosPrueba.ID_ANALISTA_UNO, true), _criterioRecibido!.Alcance);
        Assert.Equal(DatosPrueba.ID_ANALISTA_UNO, _criterioRecibido.AsignadasAUsuarioId);
        Assert.False(_criterioRecibido.SoloSinAsignar);
    }

    [Fact]
    public async Task HandleAsync_AsignacionDisponibles_CortaALasSinResponsable()
    {
        await Ejecutar(DatosPrueba.AnalistaUno(), FiltroAsignacion.Disponibles);

        Assert.True(_criterioRecibido!.SoloSinAsignar);
        Assert.Null(_criterioRecibido.AsignadasAUsuarioId);
    }

    [Fact]
    public async Task HandleAsync_RolIndeterminado_FallaCerradoSinConsultar()
    {
        Resultado<ResumenDashboardDto> resultado =
            await Ejecutar(DatosPrueba.UsuarioActual(DatosPrueba.ID_ANALISTA_UNO, rol: null));

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await _solicitudes.DidNotReceiveWithAnyArgs().ContarPorCodigoDeEstadoAsync(default!, default);
    }

    private Task<Resultado<ResumenDashboardDto>> Ejecutar(
        IUsuarioActual usuarioActual, FiltroAsignacion asignacion = FiltroAsignacion.Todas)
    {
        ObtenerResumenDashboardQueryHandler handler = new(_unitOfWork, usuarioActual, _proveedorFechaHora);

        return handler.HandleAsync(new ObtenerResumenDashboardQuery(asignacion));
    }
}

using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Queries.ObtenerSolicitudesPaginado;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Solicitudes;

/// <summary>
/// Traduccion de la consulta HTTP al <see cref="FiltroSolicitudes"/> del repositorio. Lo
/// critico es que el alcance por rol (ADR-0012) nunca lo decide el cliente: un Solicitante
/// que pide solicitudes de otro sigue recibiendo solo las suyas. El SQL resultante no se
/// prueba aqui (ver ADR-0035).
/// </summary>
public class ObtenerSolicitudesPaginadoQueryHandlerTests
{
    private const int PAGINA = 2;
    private const int TAMANO_PAGINA = 10;
    private const int TOTAL_ELEMENTOS = 11;
    private const string TEXTO_BUSQUEDA = "impresora";

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();
    private readonly IProveedorFechaHora _proveedorFechaHora = Substitute.For<IProveedorFechaHora>();

    private FiltroSolicitudes? _filtroRecibido;

    public ObtenerSolicitudesPaginadoQueryHandlerTests()
    {
        _unitOfWork.Solicitudes.Returns(_solicitudes);
        _proveedorFechaHora.Ahora.Returns(DatosPrueba.AHORA);

        _solicitudes.ObtenerPaginadoAsync(Arg.Do<FiltroSolicitudes>(filtro => _filtroRecibido = filtro), Arg.Any<CancellationToken>())
            .Returns(new ResultadoPaginado<Solicitud>([], 0, ParametrosPaginacion.PAGINA_MINIMA, TAMANO_PAGINA));
    }

    [Fact]
    public async Task HandleAsync_SolicitanteFiltraPorOtroSolicitante_ElAlcanceLoIgnoraYFuerzaSusPropias()
    {
        await Ejecutar(DatosPrueba.SolicitanteUno(), Consulta() with { UsuarioSolicitanteId = DatosPrueba.ID_SOLICITANTE_DOS });

        Assert.NotNull(_filtroRecibido);
        Assert.Equal(DatosPrueba.ID_SOLICITANTE_UNO, _filtroRecibido.UsuarioSolicitanteId);
        Assert.False(_filtroRecibido.IncluirSinAsignar);
    }

    [Fact]
    public async Task HandleAsync_AnalistaFiltraPorOtroResponsable_ElAlcanceFuerzaLasSuyasMasLasSinAsignar()
    {
        await Ejecutar(DatosPrueba.AnalistaUno(), Consulta() with { UsuarioAsignadoId = DatosPrueba.ID_ANALISTA_DOS });

        Assert.NotNull(_filtroRecibido);
        Assert.Equal(DatosPrueba.ID_ANALISTA_UNO, _filtroRecibido.UsuarioAsignadoId);
        Assert.True(_filtroRecibido.IncluirSinAsignar);
    }

    [Fact]
    public async Task HandleAsync_Administrador_UsaLosFiltrosDeLaConsultaTalCual()
    {
        ObtenerSolicitudesPaginadoQuery consulta = Consulta() with
        {
            UsuarioSolicitanteId = DatosPrueba.ID_SOLICITANTE_DOS,
            UsuarioAsignadoId = DatosPrueba.ID_ANALISTA_DOS,
            EstadoId = DatosPrueba.ID_ESTADO_EN_PROGRESO,
            PrioridadId = DatosPrueba.ID_CATALOGO,
            AreaId = DatosPrueba.ID_CATALOGO,
            TipoSolicitudId = DatosPrueba.ID_CATALOGO,
            FechaCreacionDesde = DatosPrueba.AHORA.AddDays(-7),
            FechaCreacionHasta = DatosPrueba.AHORA,
            TextoBusqueda = TEXTO_BUSQUEDA,
            SoloVencidas = true,
            Orden = OrdenSolicitudes.Urgencia,
            Direccion = DireccionOrden.Asc,
            Pagina = PAGINA,
            TamanoPagina = TAMANO_PAGINA
        };

        await Ejecutar(DatosPrueba.Administrador(), consulta);

        Assert.NotNull(_filtroRecibido);
        Assert.Equal(DatosPrueba.ID_SOLICITANTE_DOS, _filtroRecibido.UsuarioSolicitanteId);
        Assert.Equal(DatosPrueba.ID_ANALISTA_DOS, _filtroRecibido.UsuarioAsignadoId);
        Assert.Equal(DatosPrueba.ID_ESTADO_EN_PROGRESO, _filtroRecibido.EstadoId);
        Assert.Equal(DatosPrueba.ID_CATALOGO, _filtroRecibido.PrioridadId);
        Assert.Equal(DatosPrueba.ID_CATALOGO, _filtroRecibido.AreaId);
        Assert.Equal(DatosPrueba.ID_CATALOGO, _filtroRecibido.TipoSolicitudId);
        Assert.Equal(consulta.FechaCreacionDesde, _filtroRecibido.FechaCreacionDesde);
        Assert.Equal(consulta.FechaCreacionHasta, _filtroRecibido.FechaCreacionHasta);
        Assert.Equal(TEXTO_BUSQUEDA, _filtroRecibido.TextoBusqueda);
        Assert.True(_filtroRecibido.SoloVencidas);
        Assert.Equal(OrdenSolicitudes.Urgencia, _filtroRecibido.Orden);
        Assert.Equal(DireccionOrden.Asc, _filtroRecibido.Direccion);
        Assert.Equal(PAGINA, _filtroRecibido.Pagina);
        Assert.Equal(TAMANO_PAGINA, _filtroRecibido.TamanoPagina);
    }

    [Fact]
    public async Task HandleAsync_Siempre_UsaLaHoraDelProveedorComoReferenciaDeVencimiento()
    {
        await Ejecutar(DatosPrueba.Administrador(), Consulta());

        Assert.Equal(DatosPrueba.AHORA, _filtroRecibido!.FechaReferencia);
    }

    [Fact]
    public async Task HandleAsync_AsignacionAsignadas_CortaALasDelUsuarioActual()
    {
        await Ejecutar(DatosPrueba.AnalistaUno(), Consulta() with { Asignacion = FiltroAsignacion.Asignadas });

        Assert.Equal(DatosPrueba.ID_ANALISTA_UNO, _filtroRecibido!.AsignadasAUsuarioId);
        Assert.False(_filtroRecibido.SoloSinAsignar);
    }

    [Fact]
    public async Task HandleAsync_AsignacionDisponibles_CortaALasSinResponsable()
    {
        await Ejecutar(DatosPrueba.AnalistaUno(), Consulta() with { Asignacion = FiltroAsignacion.Disponibles });

        Assert.True(_filtroRecibido!.SoloSinAsignar);
        Assert.Null(_filtroRecibido.AsignadasAUsuarioId);
    }

    [Fact]
    public async Task HandleAsync_PaginaDelRepositorio_SeMapeaConSuMetadataYVencimiento()
    {
        Solicitud vencida = DatosPrueba.Solicitud(DatosPrueba.EnProgreso());
        vencida.FechaCompromiso = DatosPrueba.AHORA.AddDays(-1);

        _solicitudes.ObtenerPaginadoAsync(Arg.Any<FiltroSolicitudes>(), Arg.Any<CancellationToken>())
            .Returns(new ResultadoPaginado<Solicitud>([vencida], TOTAL_ELEMENTOS, PAGINA, TAMANO_PAGINA));

        Resultado<ResultadoPaginado<SolicitudResumenDto>> resultado = await Ejecutar(DatosPrueba.Administrador(), Consulta());

        SolicitudResumenDto elemento = Assert.Single(resultado.Valor.Elementos);
        Assert.True(elemento.EstaVencida);
        Assert.Equal(TOTAL_ELEMENTOS, resultado.Valor.TotalElementos);
        Assert.Equal(PAGINA, resultado.Valor.Pagina);
        Assert.Equal(TAMANO_PAGINA, resultado.Valor.TamanoPagina);
    }

    [Fact]
    public async Task HandleAsync_RolIndeterminado_FallaCerradoSinConsultar()
    {
        Resultado<ResultadoPaginado<SolicitudResumenDto>> resultado =
            await Ejecutar(DatosPrueba.UsuarioActual(DatosPrueba.ID_SOLICITANTE_UNO, rol: null), Consulta());

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await _solicitudes.DidNotReceiveWithAnyArgs().ObtenerPaginadoAsync(default!, default);
    }

    private static ObtenerSolicitudesPaginadoQuery Consulta() =>
        new(null, null, null, null, null, null, null, null, null, SoloVencidas: false);

    private Task<Resultado<ResultadoPaginado<SolicitudResumenDto>>> Ejecutar(
        IUsuarioActual usuarioActual, ObtenerSolicitudesPaginadoQuery consulta)
    {
        ObtenerSolicitudesPaginadoQueryHandler handler = new(_unitOfWork, usuarioActual, _proveedorFechaHora);

        return handler.HandleAsync(consulta);
    }
}

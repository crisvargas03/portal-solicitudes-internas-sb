using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Notificaciones.Dtos;
using SB.PortalSolicitudes.Application.Features.Notificaciones.Queries.ObtenerNotificacionesPaginado;
using SB.PortalSolicitudes.Application.Features.Notificaciones.Queries.ObtenerResumenNotificaciones;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Notificaciones;

/// <summary>
/// Bandeja de notificaciones (ADR-0003): cada usuario ve solo las suyas, sin importar lo que
/// pida el cliente, y sin un Id valido la consulta falla cerrada (mismo criterio que ADR-0029).
/// </summary>
public class NotificacionesHandlersTests
{
    private const int ID_NOTIFICACION = 30;
    private const int PAGINA = 2;
    private const int TAMANO_PAGINA = 10;
    private const int TOTAL_ELEMENTOS = 11;
    private const int NO_LEIDAS = 3;
    private const string ASUNTO = "Solicitud SOL-2026-0001 asignada";
    private const string MENSAJE = "Se le asigno la solicitud SOL-2026-0001.";

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly INotificacionRepository _notificaciones = Substitute.For<INotificacionRepository>();

    private FiltroNotificaciones? _filtroRecibido;

    public NotificacionesHandlersTests()
    {
        _unitOfWork.Notificaciones.Returns(_notificaciones);

        Notificacion notificacion = new()
        {
            Id = ID_NOTIFICACION,
            SolicitudId = DatosPrueba.ID_SOLICITUD,
            Solicitud = DatosPrueba.Solicitud(DatosPrueba.EnAnalisis()),
            UsuarioDestinoId = DatosPrueba.ID_ANALISTA_UNO,
            Canal = CanalNotificacion.BaseDeDatos,
            Asunto = ASUNTO,
            Mensaje = MENSAJE,
            Estado = EstadoNotificacion.Enviada,
            Fecha = DatosPrueba.AHORA
        };

        _notificaciones.ObtenerPaginadoAsync(Arg.Do<FiltroNotificaciones>(filtro => _filtroRecibido = filtro), Arg.Any<CancellationToken>())
            .Returns(new ResultadoPaginado<Notificacion>([notificacion], TOTAL_ELEMENTOS, PAGINA, TAMANO_PAGINA));
    }

    [Fact]
    public async Task ObtenerNotificaciones_Siempre_FiltraPorElUsuarioDelTokenYPasaLosDemasFiltros()
    {
        await ObtenerNotificaciones(
            DatosPrueba.AnalistaUno(),
            new ObtenerNotificacionesPaginadoQuery(
                EstadoNotificacion.Enviada, CanalNotificacion.BaseDeDatos, DatosPrueba.ID_SOLICITUD, PAGINA, TAMANO_PAGINA));

        Assert.NotNull(_filtroRecibido);
        Assert.Equal(DatosPrueba.ID_ANALISTA_UNO, _filtroRecibido.UsuarioDestinoId);
        Assert.Equal(EstadoNotificacion.Enviada, _filtroRecibido.Estado);
        Assert.Equal(CanalNotificacion.BaseDeDatos, _filtroRecibido.Canal);
        Assert.Equal(DatosPrueba.ID_SOLICITUD, _filtroRecibido.SolicitudId);
        Assert.Equal(PAGINA, _filtroRecibido.Pagina);
        Assert.Equal(TAMANO_PAGINA, _filtroRecibido.TamanoPagina);
    }

    [Fact]
    public async Task ObtenerNotificaciones_PaginaDelRepositorio_SeMapeaConElCodigoDeLaSolicitud()
    {
        Resultado<ResultadoPaginado<NotificacionDto>> resultado =
            await ObtenerNotificaciones(DatosPrueba.AnalistaUno(), new ObtenerNotificacionesPaginadoQuery(null, null, null));

        NotificacionDto dto = Assert.Single(resultado.Valor.Elementos);
        Assert.Equal(
            new NotificacionDto(
                ID_NOTIFICACION, DatosPrueba.ID_SOLICITUD, "SOL-2026-0001", nameof(CanalNotificacion.BaseDeDatos),
                ASUNTO, MENSAJE, nameof(EstadoNotificacion.Enviada), DatosPrueba.AHORA),
            dto);
        Assert.Equal(TOTAL_ELEMENTOS, resultado.Valor.TotalElementos);
    }

    [Fact]
    public async Task ObtenerNotificaciones_SinIdentificador_FallaCerradoEnVezDeDevolverLasDeTodos()
    {
        // Regresion: con Id nulo el filtro UsuarioDestinoId quedaba en null, que el repositorio
        // interpreta como "sin filtro": devolvia las notificaciones de todos los usuarios.
        Resultado<ResultadoPaginado<NotificacionDto>> resultado = await ObtenerNotificaciones(
            DatosPrueba.UsuarioActual(null, RolUsuario.Analista), new ObtenerNotificacionesPaginadoQuery(null, null, null));

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await _notificaciones.DidNotReceiveWithAnyArgs().ObtenerPaginadoAsync(default!, default);
    }

    [Fact]
    public async Task ObtenerResumen_UsuarioAutenticado_CuentaSusNoLeidas()
    {
        _notificaciones.ContarNoLeidasPorUsuarioAsync(DatosPrueba.ID_SOLICITANTE_UNO, Arg.Any<CancellationToken>()).Returns(NO_LEIDAS);

        Resultado<ResumenNotificacionesDto> resultado = await ObtenerResumen(DatosPrueba.SolicitanteUno());

        Assert.Equal(new ResumenNotificacionesDto(NO_LEIDAS), resultado.Valor);
    }

    [Fact]
    public async Task ObtenerResumen_SinIdentificador_DevuelveNoAutorizadoSinContar()
    {
        Resultado<ResumenNotificacionesDto> resultado = await ObtenerResumen(DatosPrueba.UsuarioActual(null, null));

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await _notificaciones.DidNotReceiveWithAnyArgs().ContarNoLeidasPorUsuarioAsync(default);
    }

    private Task<Resultado<ResultadoPaginado<NotificacionDto>>> ObtenerNotificaciones(
        IUsuarioActual usuarioActual, ObtenerNotificacionesPaginadoQuery consulta) =>
        new ObtenerNotificacionesPaginadoQueryHandler(_unitOfWork, usuarioActual).HandleAsync(consulta);

    private Task<Resultado<ResumenNotificacionesDto>> ObtenerResumen(IUsuarioActual usuarioActual) =>
        new ObtenerResumenNotificacionesQueryHandler(_unitOfWork, usuarioActual).HandleAsync(new ObtenerResumenNotificacionesQuery());
}

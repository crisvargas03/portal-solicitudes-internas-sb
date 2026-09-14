using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Queries.ObtenerDetalleSolicitud;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Solicitudes;

/// <summary>
/// Que datos de una solicitud salen del servidor segun quien pregunta: alcance por rol con
/// 404 (ADR-0012, amendada), comentarios internos ocultos al Solicitante (ADR-0023) y el
/// comentario de resolucion derivado del historial (ADR-0001).
/// </summary>
public class ObtenerDetalleSolicitudQueryHandlerTests
{
    private const string TEXTO_INTERNO = "Nota interna del equipo";
    private const string TEXTO_PUBLICO = "Respuesta visible para el solicitante";
    private const string RESOLUCION_ANTERIOR = "Primera resolucion";
    private const string RESOLUCION_VIGENTE = "Resolucion definitiva";
    private const int CANTIDAD_COMENTARIOS_SEMBRADOS = 2;

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();

    public ObtenerDetalleSolicitudQueryHandlerTests()
    {
        _unitOfWork.Solicitudes.Returns(_solicitudes);
    }

    [Fact]
    public async Task HandleAsync_Solicitante_NoRecibeComentariosInternos()
    {
        PrepararSolicitudConComentarios(usuarioAsignadoId: DatosPrueba.ID_ANALISTA_UNO);

        Resultado<SolicitudDetalleDto> resultado = await Ejecutar(DatosPrueba.SolicitanteUno());

        ComentarioDto comentario = Assert.Single(resultado.Valor.Comentarios);
        Assert.Equal(TEXTO_PUBLICO, comentario.Texto);
    }

    [Theory]
    [InlineData(RolUsuario.Analista)]
    [InlineData(RolUsuario.Administrador)]
    public async Task HandleAsync_PersonalDeGestion_RecibeComentariosInternos(RolUsuario rol)
    {
        PrepararSolicitudConComentarios(usuarioAsignadoId: DatosPrueba.ID_ANALISTA_UNO);
        int usuarioId = rol == RolUsuario.Analista ? DatosPrueba.ID_ANALISTA_UNO : DatosPrueba.ID_ADMINISTRADOR;

        Resultado<SolicitudDetalleDto> resultado = await Ejecutar(DatosPrueba.UsuarioActual(usuarioId, rol));

        Assert.Equal(CANTIDAD_COMENTARIOS_SEMBRADOS, resultado.Valor.Comentarios.Count);
        Assert.Contains(resultado.Valor.Comentarios, comentario => comentario.EsInterno);
    }

    [Fact]
    public async Task HandleAsync_SolicitanteSobreSolicitudAjena_DevuelveNoEncontrada()
    {
        PrepararSolicitudConComentarios(usuarioAsignadoId: null);

        Resultado<SolicitudDetalleDto> resultado = await Ejecutar(DatosPrueba.SolicitanteDos());

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
    }

    [Fact]
    public async Task HandleAsync_AnalistaSobreSolicitudAsignadaAOtroAnalista_DevuelveNoEncontrada()
    {
        // Regresion: antes el detalle solo recortaba al Solicitante, y un Analista leia por Id
        // (comentarios internos incluidos) solicitudes que su propio listado no le mostraba.
        PrepararSolicitudConComentarios(usuarioAsignadoId: DatosPrueba.ID_ANALISTA_DOS);

        Resultado<SolicitudDetalleDto> resultado = await Ejecutar(DatosPrueba.AnalistaUno());

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
    }

    [Fact]
    public async Task HandleAsync_AnalistaSobreSolicitudSinAsignar_PuedeVerla()
    {
        PrepararSolicitudConComentarios(usuarioAsignadoId: null);

        Resultado<SolicitudDetalleDto> resultado = await Ejecutar(DatosPrueba.AnalistaUno());

        Assert.True(resultado.EsExitoso);
    }

    [Fact]
    public async Task HandleAsync_SolicitudReabiertaYResueltaDeNuevo_UsaElComentarioDeLaUltimaResolucion()
    {
        Solicitud solicitud = PrepararSolicitudConComentarios(usuarioAsignadoId: DatosPrueba.ID_ANALISTA_UNO);
        AgregarPasoDeHistorial(solicitud, DatosPrueba.Resuelta(), RESOLUCION_ANTERIOR, DatosPrueba.AHORA.AddDays(-2));
        AgregarPasoDeHistorial(solicitud, DatosPrueba.EnProgreso(), "Reabierta", DatosPrueba.AHORA.AddDays(-1));
        AgregarPasoDeHistorial(solicitud, DatosPrueba.Resuelta(), RESOLUCION_VIGENTE, DatosPrueba.AHORA);

        Resultado<SolicitudDetalleDto> resultado = await Ejecutar(DatosPrueba.SolicitanteUno());

        Assert.Equal(RESOLUCION_VIGENTE, resultado.Valor.ComentarioResolucion);
    }

    [Fact]
    public async Task HandleAsync_SolicitudInexistente_DevuelveNoEncontrada()
    {
        Resultado<SolicitudDetalleDto> resultado = await Ejecutar(DatosPrueba.Administrador());

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
    }

    [Fact]
    public async Task HandleAsync_RolIndeterminado_FallaCerradoSinConsultar()
    {
        Resultado<SolicitudDetalleDto> resultado = await Ejecutar(DatosPrueba.ConRolDesconocido());

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await _solicitudes.DidNotReceiveWithAnyArgs().ObtenerDetalleAsync(default);
    }

    private Solicitud PrepararSolicitudConComentarios(int? usuarioAsignadoId)
    {
        Solicitud solicitud = DatosPrueba.Solicitud(
            DatosPrueba.EnProgreso(), DatosPrueba.ID_SOLICITANTE_UNO, usuarioAsignadoId);

        Usuario autor = DatosPrueba.Usuario(DatosPrueba.ID_ANALISTA_UNO, RolUsuario.Analista);

        solicitud.Comentarios.Add(new Comentario { Texto = TEXTO_INTERNO, EsInterno = true, Usuario = autor, Fecha = DatosPrueba.AHORA });
        solicitud.Comentarios.Add(new Comentario { Texto = TEXTO_PUBLICO, EsInterno = false, Usuario = autor, Fecha = DatosPrueba.AHORA });

        _solicitudes.ObtenerDetalleAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>()).Returns(solicitud);

        return solicitud;
    }

    private static void AgregarPasoDeHistorial(Solicitud solicitud, EstadoSolicitud estadoNuevo, string comentario, DateTime fecha)
    {
        solicitud.Historial.Add(new HistorialEstado
        {
            EstadoNuevoId = estadoNuevo.Id,
            EstadoNuevo = estadoNuevo,
            Usuario = DatosPrueba.Usuario(DatosPrueba.ID_ANALISTA_UNO, RolUsuario.Analista),
            Comentario = comentario,
            Fecha = fecha
        });
    }

    private Task<Resultado<SolicitudDetalleDto>> Ejecutar(IUsuarioActual usuarioActual)
    {
        ObtenerDetalleSolicitudQueryHandler handler = new(_unitOfWork, usuarioActual);

        return handler.HandleAsync(new ObtenerDetalleSolicitudQuery(DatosPrueba.ID_SOLICITUD));
    }
}

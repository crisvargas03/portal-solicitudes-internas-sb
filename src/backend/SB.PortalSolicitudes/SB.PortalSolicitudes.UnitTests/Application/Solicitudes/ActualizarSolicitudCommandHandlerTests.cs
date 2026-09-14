using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.ActualizarSolicitud;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Solicitudes;

/// <summary>
/// Edicion parcial (ADR-0012): el propio Solicitante o un Administrador, solo en REGISTRADA,
/// y un campo nulo significa "sin cambios". Fuera del alcance responde 404.
/// </summary>
public class ActualizarSolicitudCommandHandlerTests
{
    private const string TITULO_NUEVO = "Titulo corregido";
    private const int ID_PRIORIDAD_NUEVA = 3;

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();

    public ActualizarSolicitudCommandHandlerTests()
    {
        _unitOfWork.Solicitudes.Returns(_solicitudes);
    }

    [Fact]
    public async Task HandleAsync_SolicitanteEditaSuSolicitudRegistrada_ActualizaSoloLosCamposEnviados()
    {
        Solicitud solicitud = PrepararSolicitud(DatosPrueba.Registrada());
        string descripcionOriginal = solicitud.Descripcion;
        int areaOriginal = solicitud.AreaId;

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(
            DatosPrueba.SolicitanteUno(),
            new ActualizarSolicitudCommand(DatosPrueba.ID_SOLICITUD, TITULO_NUEVO, null, null, ID_PRIORIDAD_NUEVA, null, null));

        Assert.True(resultado.EsExitoso);
        Assert.Equal(TITULO_NUEVO, solicitud.Titulo);
        Assert.Equal(ID_PRIORIDAD_NUEVA, solicitud.PrioridadId);
        Assert.Equal(descripcionOriginal, solicitud.Descripcion);
        Assert.Equal(areaOriginal, solicitud.AreaId);
        _solicitudes.Received(1).Actualizar(solicitud);
        await _unitOfWork.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_AdministradorEditaSolicitudDeOtro_Permitido()
    {
        Solicitud solicitud = PrepararSolicitud(DatosPrueba.Registrada());

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.Administrador(), ComandoConTitulo());

        Assert.True(resultado.EsExitoso);
        Assert.Equal(TITULO_NUEVO, solicitud.Titulo);
    }

    [Fact]
    public async Task HandleAsync_SolicitudFueraDeRegistrada_DevuelveConflictoSinGuardar()
    {
        Solicitud solicitud = PrepararSolicitud(DatosPrueba.EnAnalisis());
        string tituloOriginal = solicitud.Titulo;

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.SolicitanteUno(), ComandoConTitulo());

        Assert.Equal(TipoError.Conflicto, resultado.Error.Tipo);
        Assert.Equal("Solicitud.NoEditable", resultado.Error.Codigo);
        Assert.Equal(tituloOriginal, solicitud.Titulo);
        await AfirmarQueNoSeGuardo();
    }

    [Fact]
    public async Task HandleAsync_SolicitanteEditaSolicitudAjena_DevuelveNoEncontradaSinGuardar()
    {
        PrepararSolicitud(DatosPrueba.Registrada());

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.SolicitanteDos(), ComandoConTitulo());

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardo();
    }

    [Fact]
    public async Task HandleAsync_AnalistaDentroDeSuAlcance_DevuelveProhibidoPorqueNoPuedeEditar()
    {
        // Sin asignar: esta dentro del alcance del Analista (la ve), pero editar es solo del
        // Solicitante o del Administrador: 403, no 404.
        PrepararSolicitud(DatosPrueba.Registrada(), usuarioAsignadoId: null);

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.AnalistaUno(), ComandoConTitulo());

        Assert.Equal(TipoError.Prohibido, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardo();
    }

    [Fact]
    public async Task HandleAsync_SolicitudInexistente_DevuelveNoEncontrada()
    {
        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.Administrador(), ComandoConTitulo());

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
    }

    [Fact]
    public async Task HandleAsync_RolIndeterminado_FallaCerradoSinGuardar()
    {
        PrepararSolicitud(DatosPrueba.Registrada());

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.ConRolDesconocido(), ComandoConTitulo());

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardo();
    }

    private Solicitud PrepararSolicitud(EstadoSolicitud estado, int? usuarioAsignadoId = null)
    {
        Solicitud solicitud = DatosPrueba.Solicitud(estado, DatosPrueba.ID_SOLICITANTE_UNO, usuarioAsignadoId);

        _solicitudes.ObtenerParaCambioDeEstadoAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>()).Returns(solicitud);
        _solicitudes.ObtenerDetalleAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>()).Returns(solicitud);

        return solicitud;
    }

    private static ActualizarSolicitudCommand ComandoConTitulo() =>
        new(DatosPrueba.ID_SOLICITUD, TITULO_NUEVO, null, null, null, null, null);

    private Task<Resultado<SolicitudResumenDto>> Ejecutar(IUsuarioActual usuarioActual, ActualizarSolicitudCommand comando)
    {
        ActualizarSolicitudCommandHandler handler = new(_unitOfWork, usuarioActual);

        return handler.HandleAsync(comando);
    }

    private async Task AfirmarQueNoSeGuardo()
    {
        _solicitudes.DidNotReceiveWithAnyArgs().Actualizar(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().GuardarCambiosAsync(default);
    }
}

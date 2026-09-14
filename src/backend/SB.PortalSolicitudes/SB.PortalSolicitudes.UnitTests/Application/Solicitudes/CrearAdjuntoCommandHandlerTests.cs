using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearAdjunto;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Solicitudes;

/// <summary>
/// Evidencia como referencia de texto/URL, sin archivo fisico (ADR-0006), a nombre del
/// usuario del token y dentro de su alcance (ADR-0012, amendada).
/// </summary>
public class CrearAdjuntoCommandHandlerTests
{
    private const string DESCRIPCION = "Captura del error";
    private const string URL = "https://evidencias.portalsolicitudes.test/captura.png";

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();
    private readonly IAdjuntoRepository _adjuntos = Substitute.For<IAdjuntoRepository>();
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IProveedorFechaHora _proveedorFechaHora = Substitute.For<IProveedorFechaHora>();

    public CrearAdjuntoCommandHandlerTests()
    {
        _unitOfWork.Solicitudes.Returns(_solicitudes);
        _unitOfWork.Adjuntos.Returns(_adjuntos);
        _unitOfWork.Usuarios.Returns(_usuarios);
        _proveedorFechaHora.Ahora.Returns(DatosPrueba.AHORA);

        _usuarios.ObtenerPorIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(llamada => DatosPrueba.Usuario(llamada.Arg<int>(), RolUsuario.Solicitante));

        Solicitud solicitud = DatosPrueba.Solicitud(
            DatosPrueba.EnProgreso(), DatosPrueba.ID_SOLICITANTE_UNO, DatosPrueba.ID_ANALISTA_UNO);
        _solicitudes.ObtenerParaCambioDeEstadoAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>()).Returns(solicitud);
    }

    [Theory]
    [InlineData(DatosPrueba.ID_SOLICITANTE_UNO, RolUsuario.Solicitante)]
    [InlineData(DatosPrueba.ID_ANALISTA_UNO, RolUsuario.Analista)]
    [InlineData(DatosPrueba.ID_ADMINISTRADOR, RolUsuario.Administrador)]
    public async Task HandleAsync_UsuarioDentroDelAlcance_GuardaLaReferenciaASuNombre(int usuarioId, RolUsuario rol)
    {
        Resultado<AdjuntoDto> resultado = await Ejecutar(DatosPrueba.UsuarioActual(usuarioId, rol));

        Assert.True(resultado.EsExitoso);
        Assert.Equal(URL, resultado.Valor.Url);
        await _adjuntos.Received(1).AgregarAsync(
            Arg.Is<Adjunto>(adjunto =>
                adjunto.SolicitudId == DatosPrueba.ID_SOLICITUD
                && adjunto.UsuarioId == usuarioId
                && adjunto.Descripcion == DESCRIPCION
                && adjunto.Url == URL
                && adjunto.Fecha == DatosPrueba.AHORA),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SolicitanteSobreSolicitudAjena_DevuelveNoEncontradaSinGuardar()
    {
        Resultado<AdjuntoDto> resultado = await Ejecutar(DatosPrueba.SolicitanteDos());

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardo();
    }

    [Fact]
    public async Task HandleAsync_AnalistaSobreSolicitudAsignadaAOtroAnalista_DevuelveNoEncontradaSinGuardar()
    {
        Resultado<AdjuntoDto> resultado = await Ejecutar(DatosPrueba.AnalistaDos());

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardo();
    }

    [Fact]
    public async Task HandleAsync_SinSesion_DevuelveNoAutorizado()
    {
        Resultado<AdjuntoDto> resultado = await Ejecutar(DatosPrueba.UsuarioActual(null, null));

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardo();
    }

    [Fact]
    public async Task HandleAsync_RolIndeterminado_FallaCerradoSinGuardar()
    {
        Resultado<AdjuntoDto> resultado = await Ejecutar(DatosPrueba.ConRolDesconocido());

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardo();
    }

    [Fact]
    public async Task HandleAsync_SolicitudInexistente_DevuelveNoEncontradaSinGuardar()
    {
        _solicitudes.ObtenerParaCambioDeEstadoAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>()).Returns((Solicitud?)null);

        Resultado<AdjuntoDto> resultado = await Ejecutar(DatosPrueba.Administrador());

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardo();
    }

    private Task<Resultado<AdjuntoDto>> Ejecutar(IUsuarioActual usuarioActual)
    {
        CrearAdjuntoCommandHandler handler = new(_unitOfWork, usuarioActual, _proveedorFechaHora);

        return handler.HandleAsync(new CrearAdjuntoCommand(DatosPrueba.ID_SOLICITUD, DESCRIPCION, URL));
    }

    private async Task AfirmarQueNoSeGuardo()
    {
        await _adjuntos.DidNotReceiveWithAnyArgs().AgregarAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().GuardarCambiosAsync(default);
    }
}

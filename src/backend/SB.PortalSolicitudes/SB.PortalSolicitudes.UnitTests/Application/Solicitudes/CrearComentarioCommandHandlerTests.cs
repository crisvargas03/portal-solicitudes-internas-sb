using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearComentario;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Solicitudes;

/// <summary>
/// Visibilidad de comentarios (ADR-0023): un Solicitante nunca crea comentarios internos,
/// aunque los pida; y nadie comenta fuera de su alcance (ADR-0012, amendada).
/// </summary>
public class CrearComentarioCommandHandlerTests
{
    private const string TEXTO = "Comentario de seguimiento";

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();
    private readonly IComentarioRepository _comentarios = Substitute.For<IComentarioRepository>();
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IProveedorFechaHora _proveedorFechaHora = Substitute.For<IProveedorFechaHora>();

    public CrearComentarioCommandHandlerTests()
    {
        _unitOfWork.Solicitudes.Returns(_solicitudes);
        _unitOfWork.Comentarios.Returns(_comentarios);
        _unitOfWork.Usuarios.Returns(_usuarios);
        _proveedorFechaHora.Ahora.Returns(DatosPrueba.AHORA);

        _usuarios.ObtenerPorIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(llamada => DatosPrueba.Usuario(llamada.Arg<int>(), RolUsuario.Analista));
    }

    [Fact]
    public async Task HandleAsync_SolicitantePideComentarioInterno_SeGuardaComoPublico()
    {
        PrepararSolicitud(usuarioAsignadoId: DatosPrueba.ID_ANALISTA_UNO);

        Resultado<ComentarioDto> resultado = await Ejecutar(DatosPrueba.SolicitanteUno(), esInterno: true);

        Assert.True(resultado.EsExitoso);
        Assert.False(resultado.Valor.EsInterno);
        await _comentarios.Received(1).AgregarAsync(
            Arg.Is<Comentario>(comentario => !comentario.EsInterno && comentario.UsuarioId == DatosPrueba.ID_SOLICITANTE_UNO),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_AnalistaAsignadoPideComentarioInterno_SeGuardaComoInterno()
    {
        PrepararSolicitud(usuarioAsignadoId: DatosPrueba.ID_ANALISTA_UNO);

        Resultado<ComentarioDto> resultado = await Ejecutar(DatosPrueba.AnalistaUno(), esInterno: true);

        Assert.True(resultado.Valor.EsInterno);
        await _comentarios.Received(1).AgregarAsync(
            Arg.Is<Comentario>(comentario => comentario.EsInterno && comentario.Fecha == DatosPrueba.AHORA),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SolicitanteComentaSolicitudAjena_DevuelveNoEncontradaSinGuardar()
    {
        PrepararSolicitud(usuarioAsignadoId: DatosPrueba.ID_ANALISTA_UNO);

        Resultado<ComentarioDto> resultado = await Ejecutar(DatosPrueba.SolicitanteDos(), esInterno: false);

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardo();
    }

    [Fact]
    public async Task HandleAsync_AnalistaComentaSolicitudAsignadaAOtroAnalista_DevuelveNoEncontradaSinGuardar()
    {
        PrepararSolicitud(usuarioAsignadoId: DatosPrueba.ID_ANALISTA_DOS);

        Resultado<ComentarioDto> resultado = await Ejecutar(DatosPrueba.AnalistaUno(), esInterno: true);

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardo();
    }

    [Fact]
    public async Task HandleAsync_SolicitudInexistente_DevuelveNoEncontradaSinGuardar()
    {
        Resultado<ComentarioDto> resultado = await Ejecutar(DatosPrueba.Administrador(), esInterno: true);

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardo();
    }

    [Fact]
    public async Task HandleAsync_SinSesion_DevuelveNoAutorizadoSinGuardar()
    {
        Resultado<ComentarioDto> resultado = await Ejecutar(DatosPrueba.UsuarioActual(null, null), esInterno: false);

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardo();
    }

    [Fact]
    public async Task HandleAsync_RolIndeterminado_FallaCerradoSinGuardar()
    {
        PrepararSolicitud(usuarioAsignadoId: null);

        Resultado<ComentarioDto> resultado = await Ejecutar(DatosPrueba.ConRolDesconocido(), esInterno: false);

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await AfirmarQueNoSeGuardo();
    }

    private void PrepararSolicitud(int? usuarioAsignadoId)
    {
        Solicitud solicitud = DatosPrueba.Solicitud(DatosPrueba.EnProgreso(), DatosPrueba.ID_SOLICITANTE_UNO, usuarioAsignadoId);

        _solicitudes.ObtenerParaCambioDeEstadoAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>()).Returns(solicitud);
    }

    private Task<Resultado<ComentarioDto>> Ejecutar(IUsuarioActual usuarioActual, bool esInterno)
    {
        CrearComentarioCommandHandler handler = new(_unitOfWork, usuarioActual, _proveedorFechaHora);

        return handler.HandleAsync(new CrearComentarioCommand(DatosPrueba.ID_SOLICITUD, TEXTO, esInterno));
    }

    private async Task AfirmarQueNoSeGuardo()
    {
        await _comentarios.DidNotReceiveWithAnyArgs().AgregarAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().GuardarCambiosAsync(default);
    }
}

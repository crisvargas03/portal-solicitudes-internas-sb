using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.AdminActualizarSolicitud;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Solicitudes;

/// <summary>
/// Edicion completa exclusiva de Administrador (ADR-0021): sin restriccion de estado, y deja
/// constancia con un comentario interno en la misma transaccion. La restriccion de rol la
/// aplica el <c>[Authorize]</c> del controlador, no este handler.
/// </summary>
public class AdminActualizarSolicitudCommandHandlerTests
{
    private const int ID_CATALOGO_NUEVO = 4;

    private static readonly AdminActualizarSolicitudCommand COMANDO = new(
        DatosPrueba.ID_SOLICITUD, "Titulo nuevo", "Descripcion nueva", ID_CATALOGO_NUEVO, ID_CATALOGO_NUEVO, ID_CATALOGO_NUEVO);

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();
    private readonly IComentarioRepository _comentarios = Substitute.For<IComentarioRepository>();
    private readonly IProveedorFechaHora _proveedorFechaHora = Substitute.For<IProveedorFechaHora>();

    public AdminActualizarSolicitudCommandHandlerTests()
    {
        _unitOfWork.Solicitudes.Returns(_solicitudes);
        _unitOfWork.Comentarios.Returns(_comentarios);
        _proveedorFechaHora.Ahora.Returns(DatosPrueba.AHORA);
    }

    [Fact]
    public async Task HandleAsync_SolicitudEnCualquierEstado_ReemplazaTodosLosCampos()
    {
        Solicitud solicitud = PrepararSolicitud(DatosPrueba.EnProgreso());

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.Administrador());

        Assert.True(resultado.EsExitoso);
        Assert.Equal(COMANDO.Titulo, solicitud.Titulo);
        Assert.Equal(COMANDO.Descripcion, solicitud.Descripcion);
        Assert.Equal(ID_CATALOGO_NUEVO, solicitud.TipoSolicitudId);
        Assert.Equal(ID_CATALOGO_NUEVO, solicitud.PrioridadId);
        Assert.Equal(ID_CATALOGO_NUEVO, solicitud.AreaId);
        await _unitOfWork.Received(1).ConfirmarTransaccionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_EdicionExitosa_RegistraComentarioInternoDeAuditoria()
    {
        PrepararSolicitud(DatosPrueba.EnProgreso());

        await Ejecutar(DatosPrueba.Administrador());

        await _comentarios.Received(1).AgregarAsync(
            Arg.Is<Comentario>(comentario =>
                comentario.EsInterno
                && comentario.SolicitudId == DatosPrueba.ID_SOLICITUD
                && comentario.UsuarioId == DatosPrueba.ID_ADMINISTRADOR
                && comentario.Fecha == DatosPrueba.AHORA),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_FallaAlRegistrarComentario_RevierteLaTransaccion()
    {
        PrepararSolicitud(DatosPrueba.EnProgreso());
        _comentarios.AgregarAsync(Arg.Any<Comentario>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("Fallo de base de datos"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Ejecutar(DatosPrueba.Administrador()));

        await _unitOfWork.Received(1).RevertirTransaccionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceiveWithAnyArgs().ConfirmarTransaccionAsync(default);
    }

    [Fact]
    public async Task HandleAsync_SolicitudInexistente_DevuelveNoEncontradaSinAbrirTransaccion()
    {
        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.Administrador());

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
        await _unitOfWork.DidNotReceiveWithAnyArgs().IniciarTransaccionAsync(default);
    }

    [Fact]
    public async Task HandleAsync_SinSesion_DevuelveNoAutorizado()
    {
        PrepararSolicitud(DatosPrueba.EnProgreso());

        Resultado<SolicitudResumenDto> resultado = await Ejecutar(DatosPrueba.UsuarioActual(null, null));

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await _unitOfWork.DidNotReceiveWithAnyArgs().IniciarTransaccionAsync(default);
    }

    private Solicitud PrepararSolicitud(EstadoSolicitud estado)
    {
        Solicitud solicitud = DatosPrueba.Solicitud(estado, DatosPrueba.ID_SOLICITANTE_UNO, DatosPrueba.ID_ANALISTA_UNO);

        _solicitudes.ObtenerParaCambioDeEstadoAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>()).Returns(solicitud);
        _solicitudes.ObtenerDetalleAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>()).Returns(solicitud);

        return solicitud;
    }

    private Task<Resultado<SolicitudResumenDto>> Ejecutar(IUsuarioActual usuarioActual)
    {
        AdminActualizarSolicitudCommandHandler handler = new(_unitOfWork, usuarioActual, _proveedorFechaHora);

        return handler.HandleAsync(COMANDO);
    }
}

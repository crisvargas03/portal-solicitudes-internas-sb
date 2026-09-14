using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Queries.ObtenerTransicionesDisponibles;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Solicitudes;

/// <summary>
/// Botones de cambio de estado que ofrece el frontend: solo las transiciones que el rol del
/// usuario puede ejecutar desde el estado actual, y nada fuera de su alcance.
/// </summary>
public class ObtenerTransicionesDisponiblesQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();
    private readonly ITransicionPermitidaRepository _transiciones = Substitute.For<ITransicionPermitidaRepository>();

    public ObtenerTransicionesDisponiblesQueryHandlerTests()
    {
        _unitOfWork.Solicitudes.Returns(_solicitudes);
        _unitOfWork.TransicionesPermitidas.Returns(_transiciones);

        Solicitud solicitud = DatosPrueba.Solicitud(
            DatosPrueba.Resuelta(), DatosPrueba.ID_SOLICITANTE_UNO, DatosPrueba.ID_ANALISTA_UNO);
        _solicitudes.ObtenerParaCambioDeEstadoAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>()).Returns(solicitud);

        // Desde RESUELTA, como en la semilla: volver a EN_PROGRESO (solo personal) o cerrar (todos).
        List<TransicionPermitida> desdeResuelta =
        [
            DatosPrueba.Transicion(
                DatosPrueba.Resuelta(), DatosPrueba.EnProgreso(), requiereComentario: true,
                RolUsuario.Administrador, RolUsuario.Analista),
            DatosPrueba.Transicion(
                DatosPrueba.Resuelta(), DatosPrueba.Cerrada(), requiereComentario: false,
                RolUsuario.Administrador, RolUsuario.Analista, RolUsuario.Solicitante)
        ];
        _transiciones.ObtenerDesdeEstadoAsync(DatosPrueba.ID_ESTADO_RESUELTA, Arg.Any<CancellationToken>())
            .Returns(desdeResuelta);
    }

    [Fact]
    public async Task HandleAsync_Solicitante_SoloVeLasTransicionesDeSuRol()
    {
        Resultado<IReadOnlyList<TransicionDisponibleDto>> resultado = await Ejecutar(DatosPrueba.SolicitanteUno());

        TransicionDisponibleDto transicion = Assert.Single(resultado.Valor);
        Assert.Equal(CodigosEstadoSolicitud.CERRADA, transicion.Codigo);
        Assert.False(transicion.RequiereComentario);
    }

    [Fact]
    public async Task HandleAsync_AnalistaAsignado_VeTodasLasDeSuRolConSuRequisitoDeComentario()
    {
        Resultado<IReadOnlyList<TransicionDisponibleDto>> resultado = await Ejecutar(DatosPrueba.AnalistaUno());

        Assert.Equal(
            [CodigosEstadoSolicitud.EN_PROGRESO, CodigosEstadoSolicitud.CERRADA],
            resultado.Valor.Select(transicion => transicion.Codigo));
        Assert.True(resultado.Valor.Single(transicion => transicion.Codigo == CodigosEstadoSolicitud.EN_PROGRESO).RequiereComentario);
    }

    [Fact]
    public async Task HandleAsync_SolicitanteSobreSolicitudAjena_DevuelveNoEncontrada()
    {
        Resultado<IReadOnlyList<TransicionDisponibleDto>> resultado = await Ejecutar(DatosPrueba.SolicitanteDos());

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
        await _transiciones.DidNotReceiveWithAnyArgs().ObtenerDesdeEstadoAsync(default);
    }

    [Fact]
    public async Task HandleAsync_AnalistaSobreSolicitudAsignadaAOtroAnalista_DevuelveNoEncontrada()
    {
        Resultado<IReadOnlyList<TransicionDisponibleDto>> resultado = await Ejecutar(DatosPrueba.AnalistaDos());

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
    }

    [Fact]
    public async Task HandleAsync_RolIndeterminado_FallaCerrado()
    {
        Resultado<IReadOnlyList<TransicionDisponibleDto>> resultado =
            await Ejecutar(DatosPrueba.UsuarioActual(DatosPrueba.ID_SOLICITANTE_UNO, rol: null));

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        await _solicitudes.DidNotReceiveWithAnyArgs().ObtenerParaCambioDeEstadoAsync(default);
    }

    [Fact]
    public async Task HandleAsync_SolicitudInexistente_DevuelveNoEncontrada()
    {
        _solicitudes.ObtenerParaCambioDeEstadoAsync(DatosPrueba.ID_SOLICITUD, Arg.Any<CancellationToken>()).Returns((Solicitud?)null);

        Resultado<IReadOnlyList<TransicionDisponibleDto>> resultado = await Ejecutar(DatosPrueba.Administrador());

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
    }

    private Task<Resultado<IReadOnlyList<TransicionDisponibleDto>>> Ejecutar(IUsuarioActual usuarioActual)
    {
        ObtenerTransicionesDisponiblesQueryHandler handler = new(_unitOfWork, usuarioActual);

        return handler.HandleAsync(new ObtenerTransicionesDisponiblesQuery(DatosPrueba.ID_SOLICITUD));
    }
}

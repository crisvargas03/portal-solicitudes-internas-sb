using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarTipoSolicitud;
using SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearTipoSolicitud;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerEstadosSolicitudActivos;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerTiposSolicitudActivos;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerTiposSolicitudTodos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Catalogos;

/// <summary>
/// Catalogo de tipos de solicitud (ADR-0020) y la consulta de estados activos, el unico
/// catalogo de solo lectura (sus filas son la maquina de estados, ADR-0005).
/// </summary>
public class TiposSolicitudHandlersTests
{
    private const int ID_TIPO = 4;
    private const string NOMBRE = "Soporte tecnico";
    private const string NOMBRE_NUEVO = "Soporte de infraestructura";
    private const string DESCRIPCION = "Fallas de equipos o red.";
    private const string DESCRIPCION_NUEVA = "Fallas de servidores, red o equipos.";

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITipoSolicitudRepository _tipos = Substitute.For<ITipoSolicitudRepository>();
    private readonly IEstadoSolicitudRepository _estados = Substitute.For<IEstadoSolicitudRepository>();

    public TiposSolicitudHandlersTests()
    {
        _unitOfWork.TiposSolicitud.Returns(_tipos);
        _unitOfWork.EstadosSolicitud.Returns(_estados);
    }

    [Fact]
    public async Task CrearTipoSolicitud_NombreNuevo_CreaActivoConDescripcionOpcional()
    {
        Resultado<TipoSolicitudAdminDto> resultado = await Crear(descripcion: null);

        Assert.True(resultado.EsExitoso);
        Assert.Null(resultado.Valor.Descripcion);
        await _tipos.Received(1).AgregarAsync(
            Arg.Is<TipoSolicitud>(tipo => tipo.Nombre == NOMBRE && tipo.Descripcion == null && tipo.Activo),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CrearTipoSolicitud_NombreDuplicado_DevuelveConflictoSinCrear()
    {
        _tipos.ExisteNombreAsync(NOMBRE, null, Arg.Any<CancellationToken>()).Returns(true);

        Resultado<TipoSolicitudAdminDto> resultado = await Crear(DESCRIPCION);

        Assert.Equal(TipoError.Conflicto, resultado.Error.Tipo);
        await _tipos.DidNotReceiveWithAnyArgs().AgregarAsync(default!, default);
    }

    [Fact]
    public async Task ActualizarTipoSolicitud_CamposEnviados_SeAplicanYElRestoSeConserva()
    {
        TipoSolicitud tipo = PrepararTipo();

        Resultado<TipoSolicitudAdminDto> resultado = await Actualizar(
            new ActualizarTipoSolicitudCommand(ID_TIPO, NOMBRE_NUEVO, DESCRIPCION_NUEVA, null));

        Assert.Equal(new TipoSolicitudAdminDto(ID_TIPO, NOMBRE_NUEVO, DESCRIPCION_NUEVA, true), resultado.Valor);
        Assert.True(tipo.Activo);
        await _tipos.Received(1).ExisteNombreAsync(NOMBRE_NUEVO, ID_TIPO, Arg.Any<CancellationToken>());
        _tipos.Received(1).Actualizar(tipo);
    }

    [Fact]
    public async Task ActualizarTipoSolicitud_NombreDeOtroTipo_DevuelveConflictoSinGuardar()
    {
        TipoSolicitud tipo = PrepararTipo();
        _tipos.ExisteNombreAsync(NOMBRE_NUEVO, ID_TIPO, Arg.Any<CancellationToken>()).Returns(true);

        Resultado<TipoSolicitudAdminDto> resultado = await Actualizar(new ActualizarTipoSolicitudCommand(ID_TIPO, NOMBRE_NUEVO, null, null));

        Assert.Equal(TipoError.Conflicto, resultado.Error.Tipo);
        Assert.Equal(NOMBRE, tipo.Nombre);
        await _unitOfWork.DidNotReceiveWithAnyArgs().GuardarCambiosAsync(default);
    }

    [Fact]
    public async Task ActualizarTipoSolicitud_Inexistente_DevuelveNoEncontrado()
    {
        Resultado<TipoSolicitudAdminDto> resultado = await Actualizar(new ActualizarTipoSolicitudCommand(ID_TIPO, null, null, false));

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
    }

    [Fact]
    public async Task ObtenerTiposSolicitudActivos_DevuelveIdYNombre()
    {
        _tipos.ObtenerActivosAsync(Arg.Any<CancellationToken>()).Returns([new TipoSolicitud { Id = ID_TIPO, Nombre = NOMBRE }]);

        Resultado<IReadOnlyList<CatalogoDto>> resultado =
            await new ObtenerTiposSolicitudActivosQueryHandler(_unitOfWork).HandleAsync(new ObtenerTiposSolicitudActivosQuery());

        Assert.Equal(new CatalogoDto(ID_TIPO, NOMBRE), Assert.Single(resultado.Valor));
    }

    [Fact]
    public async Task ObtenerTiposSolicitudTodos_IncluyeDescripcionYEstado()
    {
        _tipos.ObtenerTodosAsync(Arg.Any<CancellationToken>())
            .Returns([new TipoSolicitud { Id = ID_TIPO, Nombre = NOMBRE, Descripcion = DESCRIPCION, Activo = false }]);

        Resultado<IReadOnlyList<TipoSolicitudAdminDto>> resultado =
            await new ObtenerTiposSolicitudTodosQueryHandler(_unitOfWork).HandleAsync(new ObtenerTiposSolicitudTodosQuery());

        Assert.Equal(new TipoSolicitudAdminDto(ID_TIPO, NOMBRE, DESCRIPCION, false), Assert.Single(resultado.Valor));
    }

    [Fact]
    public async Task ObtenerEstadosSolicitudActivos_ConservaOrdenCodigoYSiEsFinal()
    {
        _estados.ObtenerActivosOrdenadosAsync(Arg.Any<CancellationToken>())
            .Returns([DatosPrueba.Registrada(), DatosPrueba.Cerrada()]);

        Resultado<IReadOnlyList<EstadoSolicitudDto>> resultado =
            await new ObtenerEstadosSolicitudActivosQueryHandler(_unitOfWork).HandleAsync(new ObtenerEstadosSolicitudActivosQuery());

        Assert.Equal(
            [(CodigosEstadoSolicitud.REGISTRADA, false), (CodigosEstadoSolicitud.CERRADA, true)],
            resultado.Valor.Select(estado => (estado.Codigo, estado.EsFinal)));
    }

    private TipoSolicitud PrepararTipo()
    {
        TipoSolicitud tipo = new() { Id = ID_TIPO, Nombre = NOMBRE, Descripcion = DESCRIPCION, Activo = true };
        _tipos.ObtenerPorIdAsync(ID_TIPO, Arg.Any<CancellationToken>()).Returns(tipo);

        return tipo;
    }

    private Task<Resultado<TipoSolicitudAdminDto>> Crear(string? descripcion) =>
        new CrearTipoSolicitudCommandHandler(_unitOfWork).HandleAsync(new CrearTipoSolicitudCommand(NOMBRE, descripcion));

    private Task<Resultado<TipoSolicitudAdminDto>> Actualizar(ActualizarTipoSolicitudCommand comando) =>
        new ActualizarTipoSolicitudCommandHandler(_unitOfWork).HandleAsync(comando);
}

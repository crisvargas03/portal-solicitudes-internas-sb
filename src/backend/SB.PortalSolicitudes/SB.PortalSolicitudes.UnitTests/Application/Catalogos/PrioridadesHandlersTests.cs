using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarPrioridad;
using SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearPrioridad;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerPrioridadesActivas;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerPrioridadesTodas;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.UnitTests.Application.Catalogos;

/// <summary>
/// Catalogo de prioridades (ADR-0004, ADR-0020): como las areas, mas el <c>Nivel</c> que
/// ordena el tablero y el orden por urgencia del listado.
/// </summary>
public class PrioridadesHandlersTests
{
    private const int ID_PRIORIDAD = 3;
    private const string NOMBRE = "Alta";
    private const string NOMBRE_NUEVO = "Muy alta";
    private const int NIVEL = 3;
    private const int NIVEL_NUEVO = 5;

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPrioridadRepository _prioridades = Substitute.For<IPrioridadRepository>();

    public PrioridadesHandlersTests()
    {
        _unitOfWork.Prioridades.Returns(_prioridades);
    }

    [Fact]
    public async Task CrearPrioridad_NombreNuevo_CreaActivaConSuNivel()
    {
        Resultado<PrioridadAdminDto> resultado = await Crear();

        Assert.Equal(NIVEL, resultado.Valor.Nivel);
        await _prioridades.Received(1).AgregarAsync(
            Arg.Is<Prioridad>(prioridad => prioridad.Nombre == NOMBRE && prioridad.Nivel == NIVEL && prioridad.Activo),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CrearPrioridad_NombreDuplicado_DevuelveConflictoSinCrear()
    {
        _prioridades.ExisteNombreAsync(NOMBRE, null, Arg.Any<CancellationToken>()).Returns(true);

        Resultado<PrioridadAdminDto> resultado = await Crear();

        Assert.Equal(TipoError.Conflicto, resultado.Error.Tipo);
        await _prioridades.DidNotReceiveWithAnyArgs().AgregarAsync(default!, default);
    }

    [Fact]
    public async Task ActualizarPrioridad_SoloNivel_CambiaElNivelSinTocarLoDemas()
    {
        Prioridad prioridad = PrepararPrioridad();

        Resultado<PrioridadAdminDto> resultado = await Actualizar(new ActualizarPrioridadCommand(ID_PRIORIDAD, null, NIVEL_NUEVO, null));

        Assert.True(resultado.EsExitoso);
        Assert.Equal(NIVEL_NUEVO, prioridad.Nivel);
        Assert.Equal(NOMBRE, prioridad.Nombre);
        Assert.True(prioridad.Activo);
        _prioridades.Received(1).Actualizar(prioridad);
    }

    [Fact]
    public async Task ActualizarPrioridad_NombreLibre_ValidaUnicidadExcluyendoLaPropiaYLoAplica()
    {
        Prioridad prioridad = PrepararPrioridad();

        Resultado<PrioridadAdminDto> resultado = await Actualizar(new ActualizarPrioridadCommand(ID_PRIORIDAD, NOMBRE_NUEVO, null, false));

        Assert.Equal(new PrioridadAdminDto(ID_PRIORIDAD, NOMBRE_NUEVO, NIVEL, false), resultado.Valor);
        await _prioridades.Received(1).ExisteNombreAsync(NOMBRE_NUEVO, ID_PRIORIDAD, Arg.Any<CancellationToken>());
        Assert.False(prioridad.Activo);
    }

    [Fact]
    public async Task ActualizarPrioridad_NombreDeOtraPrioridad_DevuelveConflictoSinGuardar()
    {
        Prioridad prioridad = PrepararPrioridad();
        _prioridades.ExisteNombreAsync(NOMBRE_NUEVO, ID_PRIORIDAD, Arg.Any<CancellationToken>()).Returns(true);

        Resultado<PrioridadAdminDto> resultado = await Actualizar(new ActualizarPrioridadCommand(ID_PRIORIDAD, NOMBRE_NUEVO, null, null));

        Assert.Equal(TipoError.Conflicto, resultado.Error.Tipo);
        Assert.Equal(NOMBRE, prioridad.Nombre);
        await _unitOfWork.DidNotReceiveWithAnyArgs().GuardarCambiosAsync(default);
    }

    [Fact]
    public async Task ActualizarPrioridad_Inexistente_DevuelveNoEncontrada()
    {
        Resultado<PrioridadAdminDto> resultado = await Actualizar(new ActualizarPrioridadCommand(ID_PRIORIDAD, null, null, false));

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
    }

    [Fact]
    public async Task ObtenerPrioridadesActivas_UsaElOrdenPorNivelDelRepositorio()
    {
        _prioridades.ObtenerActivasOrdenadasPorNivelAsync(Arg.Any<CancellationToken>())
            .Returns([new Prioridad { Id = ID_PRIORIDAD, Nombre = NOMBRE, Nivel = NIVEL }]);

        Resultado<IReadOnlyList<PrioridadDto>> resultado =
            await new ObtenerPrioridadesActivasQueryHandler(_unitOfWork).HandleAsync(new ObtenerPrioridadesActivasQuery());

        Assert.Equal(new PrioridadDto(ID_PRIORIDAD, NOMBRE, NIVEL), Assert.Single(resultado.Valor));
        await _prioridades.DidNotReceiveWithAnyArgs().ObtenerActivosAsync(default);
    }

    [Fact]
    public async Task ObtenerPrioridadesTodas_IncluyeLasInactivasConSuEstado()
    {
        _prioridades.ObtenerTodosAsync(Arg.Any<CancellationToken>())
            .Returns([new Prioridad { Id = ID_PRIORIDAD, Nombre = NOMBRE, Nivel = NIVEL, Activo = false }]);

        Resultado<IReadOnlyList<PrioridadAdminDto>> resultado =
            await new ObtenerPrioridadesTodasQueryHandler(_unitOfWork).HandleAsync(new ObtenerPrioridadesTodasQuery());

        Assert.Equal(new PrioridadAdminDto(ID_PRIORIDAD, NOMBRE, NIVEL, false), Assert.Single(resultado.Valor));
    }

    private Prioridad PrepararPrioridad()
    {
        Prioridad prioridad = new() { Id = ID_PRIORIDAD, Nombre = NOMBRE, Nivel = NIVEL, Activo = true };
        _prioridades.ObtenerPorIdAsync(ID_PRIORIDAD, Arg.Any<CancellationToken>()).Returns(prioridad);

        return prioridad;
    }

    private Task<Resultado<PrioridadAdminDto>> Crear() =>
        new CrearPrioridadCommandHandler(_unitOfWork).HandleAsync(new CrearPrioridadCommand(NOMBRE, NIVEL));

    private Task<Resultado<PrioridadAdminDto>> Actualizar(ActualizarPrioridadCommand comando) =>
        new ActualizarPrioridadCommandHandler(_unitOfWork).HandleAsync(comando);
}

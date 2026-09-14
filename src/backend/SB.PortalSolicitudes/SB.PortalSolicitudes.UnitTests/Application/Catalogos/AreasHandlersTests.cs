using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarArea;
using SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearArea;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerAreasActivas;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerAreasTodas;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.UnitTests.Application.Catalogos;

/// <summary>
/// Catalogo administrable de areas (ADR-0020): nombre unico, sin borrado (se desactiva) y
/// edicion parcial donde un campo nulo significa "sin cambios".
/// </summary>
public class AreasHandlersTests
{
    private const int ID_AREA = 7;
    private const string NOMBRE = "Tecnologia";
    private const string NOMBRE_NUEVO = "Tecnologia de la Informacion";

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IAreaRepository _areas = Substitute.For<IAreaRepository>();

    public AreasHandlersTests()
    {
        _unitOfWork.Areas.Returns(_areas);
    }

    [Fact]
    public async Task CrearArea_NombreNuevo_CreaActivaYGuarda()
    {
        Resultado<AreaAdminDto> resultado = await new CrearAreaCommandHandler(_unitOfWork).HandleAsync(new CrearAreaCommand(NOMBRE));

        Assert.True(resultado.EsExitoso);
        Assert.True(resultado.Valor.Activo);
        await _areas.Received(1).AgregarAsync(Arg.Is<Area>(area => area.Nombre == NOMBRE && area.Activo), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CrearArea_NombreDuplicado_DevuelveConflictoSinCrear()
    {
        _areas.ExisteNombreAsync(NOMBRE, null, Arg.Any<CancellationToken>()).Returns(true);

        Resultado<AreaAdminDto> resultado = await new CrearAreaCommandHandler(_unitOfWork).HandleAsync(new CrearAreaCommand(NOMBRE));

        Assert.Equal(TipoError.Conflicto, resultado.Error.Tipo);
        await _areas.DidNotReceiveWithAnyArgs().AgregarAsync(default!, default);
    }

    [Fact]
    public async Task ActualizarArea_SoloActivo_DesactivaSinTocarElNombreNiValidarDuplicado()
    {
        Area area = PrepararArea();

        Resultado<AreaAdminDto> resultado = await Actualizar(new ActualizarAreaCommand(ID_AREA, null, Activo: false));

        Assert.True(resultado.EsExitoso);
        Assert.False(area.Activo);
        Assert.Equal(NOMBRE, area.Nombre);
        await _areas.DidNotReceiveWithAnyArgs().ExisteNombreAsync(default!, default, default);
        _areas.Received(1).Actualizar(area);
    }

    [Fact]
    public async Task ActualizarArea_NombreNuevo_ValidaUnicidadExcluyendoLaPropiaArea()
    {
        Area area = PrepararArea();

        await Actualizar(new ActualizarAreaCommand(ID_AREA, NOMBRE_NUEVO, null));

        await _areas.Received(1).ExisteNombreAsync(NOMBRE_NUEVO, ID_AREA, Arg.Any<CancellationToken>());
        Assert.Equal(NOMBRE_NUEVO, area.Nombre);
    }

    [Fact]
    public async Task ActualizarArea_NombreDeOtraArea_DevuelveConflictoSinGuardar()
    {
        Area area = PrepararArea();
        _areas.ExisteNombreAsync(NOMBRE_NUEVO, ID_AREA, Arg.Any<CancellationToken>()).Returns(true);

        Resultado<AreaAdminDto> resultado = await Actualizar(new ActualizarAreaCommand(ID_AREA, NOMBRE_NUEVO, null));

        Assert.Equal(TipoError.Conflicto, resultado.Error.Tipo);
        Assert.Equal(NOMBRE, area.Nombre);
        await _unitOfWork.DidNotReceiveWithAnyArgs().GuardarCambiosAsync(default);
    }

    [Fact]
    public async Task ActualizarArea_Inexistente_DevuelveNoEncontrada()
    {
        Resultado<AreaAdminDto> resultado = await Actualizar(new ActualizarAreaCommand(ID_AREA, NOMBRE_NUEVO, null));

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
    }

    [Fact]
    public async Task ObtenerAreasActivas_DevuelveIdYNombreDeLasActivas()
    {
        _areas.ObtenerActivosAsync(Arg.Any<CancellationToken>()).Returns([new Area { Id = ID_AREA, Nombre = NOMBRE }]);

        Resultado<IReadOnlyList<CatalogoDto>> resultado =
            await new ObtenerAreasActivasQueryHandler(_unitOfWork).HandleAsync(new ObtenerAreasActivasQuery());

        Assert.Equal(new CatalogoDto(ID_AREA, NOMBRE), Assert.Single(resultado.Valor));
    }

    [Fact]
    public async Task ObtenerAreasTodas_IncluyeLasInactivasConSuEstado()
    {
        _areas.ObtenerTodosAsync(Arg.Any<CancellationToken>())
            .Returns([new Area { Id = ID_AREA, Nombre = NOMBRE, Activo = false }]);

        Resultado<IReadOnlyList<AreaAdminDto>> resultado =
            await new ObtenerAreasTodasQueryHandler(_unitOfWork).HandleAsync(new ObtenerAreasTodasQuery());

        Assert.Equal(new AreaAdminDto(ID_AREA, NOMBRE, false), Assert.Single(resultado.Valor));
    }

    private Area PrepararArea()
    {
        Area area = new() { Id = ID_AREA, Nombre = NOMBRE, Activo = true };
        _areas.ObtenerPorIdAsync(ID_AREA, Arg.Any<CancellationToken>()).Returns(area);

        return area;
    }

    private Task<Resultado<AreaAdminDto>> Actualizar(ActualizarAreaCommand comando) =>
        new ActualizarAreaCommandHandler(_unitOfWork).HandleAsync(comando);
}

using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Commands.ActualizarEntidadGubernamental;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Commands.CrearEntidadGubernamental;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Dtos;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Queries.ObtenerEntidadesGubernamentalesActivas;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Queries.ObtenerEntidadesGubernamentalesTodas;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Queries.ObtenerEntidadGubernamentalPorId;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.UnitTests.Application.EntidadesGubernamentales;

/// <summary>
/// Mantenimiento de entidades gubernamentales sobre archivo de texto (ADR-0034). El
/// repositorio de archivo se simula: aqui solo se prueban las reglas del handler (nombre
/// unico, edicion parcial, alta activa). La escritura del archivo pertenece a Infraestructura.
/// </summary>
public class EntidadesGubernamentalesHandlersTests
{
    private const int ID_ENTIDAD = 42;
    private const int ID_ASIGNADO_POR_EL_ARCHIVO = 182;
    private const string NOMBRE = "Superintendencia de Bancos";
    private const string NOMBRE_NUEVO = "Superintendencia de Bancos de la Republica Dominicana";
    private const string CATEGORIA = "Organismo Autonomo";
    private const string PODER_DEL_ESTADO = "Organismos Autonomos";
    private const string SECTOR = "Finanzas";
    private const string SECTOR_NUEVO = "Sistema Financiero";

    private readonly IEntidadGubernamentalRepository _repositorio = Substitute.For<IEntidadGubernamentalRepository>();

    [Fact]
    public async Task Crear_NombreNuevo_CreaActivaYDevuelveElIdAsignadoPorElRepositorio()
    {
        _repositorio.CrearAsync(Arg.Any<EntidadGubernamental>(), Arg.Any<CancellationToken>())
            .Returns(llamada =>
            {
                EntidadGubernamental entidad = llamada.Arg<EntidadGubernamental>();
                entidad.Id = ID_ASIGNADO_POR_EL_ARCHIVO;

                return entidad;
            });

        Resultado<EntidadGubernamentalAdminDto> resultado = await Crear();

        Assert.Equal(
            new EntidadGubernamentalAdminDto(ID_ASIGNADO_POR_EL_ARCHIVO, NOMBRE, CATEGORIA, PODER_DEL_ESTADO, SECTOR, true),
            resultado.Valor);
    }

    [Fact]
    public async Task Crear_NombreDuplicado_DevuelveConflictoSinEscribir()
    {
        _repositorio.ExisteNombreAsync(NOMBRE, null, Arg.Any<CancellationToken>()).Returns(true);

        Resultado<EntidadGubernamentalAdminDto> resultado = await Crear();

        Assert.Equal(TipoError.Conflicto, resultado.Error.Tipo);
        await _repositorio.DidNotReceiveWithAnyArgs().CrearAsync(default!, default);
    }

    [Fact]
    public async Task Actualizar_CamposParciales_AplicaLosEnviadosYConservaElResto()
    {
        EntidadGubernamental entidad = PrepararEntidad();

        Resultado<EntidadGubernamentalAdminDto> resultado = await Actualizar(
            new ActualizarEntidadGubernamentalCommand(ID_ENTIDAD, null, null, null, SECTOR_NUEVO, Activo: false));

        Assert.Equal(
            new EntidadGubernamentalAdminDto(ID_ENTIDAD, NOMBRE, CATEGORIA, PODER_DEL_ESTADO, SECTOR_NUEVO, false),
            resultado.Valor);
        await _repositorio.Received(1).ActualizarAsync(entidad, Arg.Any<CancellationToken>());
        await _repositorio.DidNotReceiveWithAnyArgs().ExisteNombreAsync(default!, default, default);
    }

    [Fact]
    public async Task Actualizar_NombreNuevo_ValidaUnicidadExcluyendoLaPropiaEntidad()
    {
        PrepararEntidad();

        Resultado<EntidadGubernamentalAdminDto> resultado = await Actualizar(
            new ActualizarEntidadGubernamentalCommand(ID_ENTIDAD, NOMBRE_NUEVO, null, null, null, null));

        Assert.Equal(NOMBRE_NUEVO, resultado.Valor.Nombre);
        await _repositorio.Received(1).ExisteNombreAsync(NOMBRE_NUEVO, ID_ENTIDAD, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_NombreDeOtraEntidad_DevuelveConflictoSinEscribir()
    {
        EntidadGubernamental entidad = PrepararEntidad();
        _repositorio.ExisteNombreAsync(NOMBRE_NUEVO, ID_ENTIDAD, Arg.Any<CancellationToken>()).Returns(true);

        Resultado<EntidadGubernamentalAdminDto> resultado = await Actualizar(
            new ActualizarEntidadGubernamentalCommand(ID_ENTIDAD, NOMBRE_NUEVO, null, null, null, null));

        Assert.Equal(TipoError.Conflicto, resultado.Error.Tipo);
        Assert.Equal(NOMBRE, entidad.Nombre);
        await _repositorio.DidNotReceiveWithAnyArgs().ActualizarAsync(default!, default);
    }

    [Fact]
    public async Task Actualizar_Inexistente_DevuelveNoEncontrada()
    {
        Resultado<EntidadGubernamentalAdminDto> resultado = await Actualizar(
            new ActualizarEntidadGubernamentalCommand(ID_ENTIDAD, NOMBRE_NUEVO, null, null, null, null));

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
        await _repositorio.DidNotReceiveWithAnyArgs().ActualizarAsync(default!, default);
    }

    [Fact]
    public async Task ObtenerPorId_Existente_DevuelveTodosSusCampos()
    {
        PrepararEntidad();

        Resultado<EntidadGubernamentalAdminDto> resultado = await new ObtenerEntidadGubernamentalPorIdQueryHandler(_repositorio)
            .HandleAsync(new ObtenerEntidadGubernamentalPorIdQuery(ID_ENTIDAD));

        Assert.Equal(new EntidadGubernamentalAdminDto(ID_ENTIDAD, NOMBRE, CATEGORIA, PODER_DEL_ESTADO, SECTOR, true), resultado.Valor);
    }

    [Fact]
    public async Task ObtenerPorId_Inexistente_DevuelveNoEncontrada()
    {
        Resultado<EntidadGubernamentalAdminDto> resultado = await new ObtenerEntidadGubernamentalPorIdQueryHandler(_repositorio)
            .HandleAsync(new ObtenerEntidadGubernamentalPorIdQuery(ID_ENTIDAD));

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
    }

    [Fact]
    public async Task ObtenerActivas_UsaLaConsultaDeActivasDelRepositorio()
    {
        _repositorio.ObtenerActivasAsync(Arg.Any<CancellationToken>()).Returns([NuevaEntidad()]);

        Resultado<IReadOnlyList<EntidadGubernamentalAdminDto>> resultado =
            await new ObtenerEntidadesGubernamentalesActivasQueryHandler(_repositorio)
                .HandleAsync(new ObtenerEntidadesGubernamentalesActivasQuery());

        Assert.Equal(ID_ENTIDAD, Assert.Single(resultado.Valor).Id);
        await _repositorio.DidNotReceiveWithAnyArgs().ObtenerTodasAsync(default);
    }

    [Fact]
    public async Task ObtenerTodas_IncluyeLasInactivas()
    {
        EntidadGubernamental inactiva = NuevaEntidad();
        inactiva.Activo = false;
        _repositorio.ObtenerTodasAsync(Arg.Any<CancellationToken>()).Returns([inactiva]);

        Resultado<IReadOnlyList<EntidadGubernamentalAdminDto>> resultado =
            await new ObtenerEntidadesGubernamentalesTodasQueryHandler(_repositorio)
                .HandleAsync(new ObtenerEntidadesGubernamentalesTodasQuery());

        Assert.False(Assert.Single(resultado.Valor).Activo);
    }

    private static EntidadGubernamental NuevaEntidad() => new()
    {
        Id = ID_ENTIDAD,
        Nombre = NOMBRE,
        Categoria = CATEGORIA,
        PoderDelEstado = PODER_DEL_ESTADO,
        Sector = SECTOR,
        Activo = true
    };

    private EntidadGubernamental PrepararEntidad()
    {
        EntidadGubernamental entidad = NuevaEntidad();
        _repositorio.ObtenerPorIdAsync(ID_ENTIDAD, Arg.Any<CancellationToken>()).Returns(entidad);

        return entidad;
    }

    private Task<Resultado<EntidadGubernamentalAdminDto>> Crear() =>
        new CrearEntidadGubernamentalCommandHandler(_repositorio)
            .HandleAsync(new CrearEntidadGubernamentalCommand(NOMBRE, CATEGORIA, PODER_DEL_ESTADO, SECTOR));

    private Task<Resultado<EntidadGubernamentalAdminDto>> Actualizar(ActualizarEntidadGubernamentalCommand comando) =>
        new ActualizarEntidadGubernamentalCommandHandler(_repositorio).HandleAsync(comando);
}

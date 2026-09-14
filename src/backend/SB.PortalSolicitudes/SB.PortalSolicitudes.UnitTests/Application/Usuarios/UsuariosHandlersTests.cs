using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Usuarios.Commands.ActualizarUsuario;
using SB.PortalSolicitudes.Application.Features.Usuarios.Commands.CrearUsuario;
using SB.PortalSolicitudes.Application.Features.Usuarios.Queries.ObtenerUsuariosPaginado;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Usuarios;

/// <summary>
/// Administracion de usuarios (ADR-0022): alta con rol elegido y password hasheada, baja
/// logica via <c>Activo</c>, y la guarda "un Administrador no modifica a otro Administrador".
/// </summary>
public class UsuariosHandlersTests
{
    private const string EMAIL = "nuevo@portalsolicitudes.test";
    private const string PASSWORD = "Password123!";
    private const string HASH_GENERADO = "hash-bcrypt-generado";
    private const string NOMBRE_NUEVO = "Nombre Corregido";
    private const int ID_OTRO_ADMINISTRADOR = 9;
    private const int PAGINA = 2;
    private const int TAMANO_PAGINA = 10;
    private const int TOTAL_ELEMENTOS = 12;
    private const int CANTIDAD_ANALISTAS = 2;
    private const int TAMANO_PAGINA_MENOR_QUE_LA_LISTA = 1;

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IHasheadorPasswords _hasheadorPasswords = Substitute.For<IHasheadorPasswords>();

    public UsuariosHandlersTests()
    {
        _unitOfWork.Usuarios.Returns(_usuarios);
        _hasheadorPasswords.Hashear(PASSWORD).Returns(HASH_GENERADO);
    }

    // ---- CrearUsuario ----

    [Theory]
    [InlineData(RolUsuario.Analista)]
    [InlineData(RolUsuario.Administrador)]
    public async Task CrearUsuario_EmailNuevo_CreaActivoConElRolIndicadoYPasswordHasheada(RolUsuario rol)
    {
        Resultado<UsuarioResumenDto> resultado = await CrearUsuario(rol);

        Assert.True(resultado.EsExitoso);
        Assert.Equal(rol.ToString(), resultado.Valor.Rol);
        await _usuarios.Received(1).AgregarAsync(
            Arg.Is<Usuario>(usuario =>
                usuario.Email == EMAIL && usuario.Rol == rol && usuario.Activo && usuario.PasswordHash == HASH_GENERADO),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CrearUsuario_EmailExistente_DevuelveConflictoSinCrear()
    {
        _usuarios.ExisteEmailAsync(EMAIL, null, Arg.Any<CancellationToken>()).Returns(true);

        Resultado<UsuarioResumenDto> resultado = await CrearUsuario(RolUsuario.Analista);

        Assert.Equal(TipoError.Conflicto, resultado.Error.Tipo);
        await _usuarios.DidNotReceiveWithAnyArgs().AgregarAsync(default!, default);
    }

    // ---- ActualizarUsuario ----

    [Fact]
    public async Task ActualizarUsuario_AnalistaConCamposParciales_ActualizaSoloLosEnviados()
    {
        Usuario analista = PrepararUsuario(DatosPrueba.ID_ANALISTA_UNO, RolUsuario.Analista);
        string nombreOriginal = analista.Nombre;

        Resultado<UsuarioResumenDto> resultado = await ActualizarUsuario(
            DatosPrueba.Administrador(), new ActualizarUsuarioCommand(DatosPrueba.ID_ANALISTA_UNO, null, null, Activo: false));

        Assert.True(resultado.EsExitoso);
        Assert.False(analista.Activo);
        Assert.Equal(nombreOriginal, analista.Nombre);
        Assert.Equal(RolUsuario.Analista, analista.Rol);
        _usuarios.Received(1).Actualizar(analista);
        await _unitOfWork.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActualizarUsuario_CambioDeRol_SeAplica()
    {
        Usuario solicitante = PrepararUsuario(DatosPrueba.ID_SOLICITANTE_UNO, RolUsuario.Solicitante);

        await ActualizarUsuario(
            DatosPrueba.Administrador(),
            new ActualizarUsuarioCommand(DatosPrueba.ID_SOLICITANTE_UNO, NOMBRE_NUEVO, RolUsuario.Analista, null));

        Assert.Equal(RolUsuario.Analista, solicitante.Rol);
        Assert.Equal(NOMBRE_NUEVO, solicitante.Nombre);
        Assert.True(solicitante.Activo);
    }

    [Fact]
    public async Task ActualizarUsuario_AdministradorModificaAOtroAdministrador_DevuelveProhibidoSinGuardar()
    {
        Usuario otroAdministrador = PrepararUsuario(ID_OTRO_ADMINISTRADOR, RolUsuario.Administrador);

        Resultado<UsuarioResumenDto> resultado = await ActualizarUsuario(
            DatosPrueba.Administrador(), new ActualizarUsuarioCommand(ID_OTRO_ADMINISTRADOR, null, null, Activo: false));

        Assert.Equal(TipoError.Prohibido, resultado.Error.Tipo);
        Assert.True(otroAdministrador.Activo);
        await _unitOfWork.DidNotReceiveWithAnyArgs().GuardarCambiosAsync(default);
    }

    [Fact]
    public async Task ActualizarUsuario_AdministradorSeModificaASiMismo_Permitido()
    {
        Usuario administrador = PrepararUsuario(DatosPrueba.ID_ADMINISTRADOR, RolUsuario.Administrador);

        Resultado<UsuarioResumenDto> resultado = await ActualizarUsuario(
            DatosPrueba.Administrador(), new ActualizarUsuarioCommand(DatosPrueba.ID_ADMINISTRADOR, NOMBRE_NUEVO, null, null));

        Assert.True(resultado.EsExitoso);
        Assert.Equal(NOMBRE_NUEVO, administrador.Nombre);
    }

    [Fact]
    public async Task ActualizarUsuario_UsuarioInexistente_DevuelveNoEncontrado()
    {
        Resultado<UsuarioResumenDto> resultado = await ActualizarUsuario(
            DatosPrueba.Administrador(), new ActualizarUsuarioCommand(DatosPrueba.ID_ANALISTA_UNO, NOMBRE_NUEVO, null, null));

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
    }

    // ---- ObtenerUsuariosPaginado ----

    [Fact]
    public async Task ObtenerUsuariosPaginado_ConRol_DevuelveLaListaCompletaDelRolEnUnaSolaPagina()
    {
        _usuarios.ObtenerPorRolAsync(RolUsuario.Analista, true, Arg.Any<CancellationToken>())
            .Returns(
            [
                DatosPrueba.Usuario(DatosPrueba.ID_ANALISTA_UNO, RolUsuario.Analista),
                DatosPrueba.Usuario(DatosPrueba.ID_ANALISTA_DOS, RolUsuario.Analista)
            ]);

        Resultado<ResultadoPaginado<UsuarioResumenDto>> resultado = await ObtenerUsuarios(
            new ObtenerUsuariosPaginadoQuery(RolUsuario.Analista, SoloActivos: true, Pagina: PAGINA, TamanoPagina: TAMANO_PAGINA_MENOR_QUE_LA_LISTA));

        Assert.Equal(CANTIDAD_ANALISTAS, resultado.Valor.Elementos.Count);
        Assert.Equal(CANTIDAD_ANALISTAS, resultado.Valor.TotalElementos);
        Assert.Equal(ParametrosPaginacion.PAGINA_MINIMA, resultado.Valor.Pagina);
        Assert.Equal(ParametrosPaginacion.PAGINA_MINIMA, resultado.Valor.TotalPaginas);
        await _usuarios.DidNotReceiveWithAnyArgs().ObtenerPaginadoAsync(default!, default, default);
    }

    [Fact]
    public async Task ObtenerUsuariosPaginado_ConRolSinUsuarios_DevuelvePaginaVacia()
    {
        _usuarios.ObtenerPorRolAsync(RolUsuario.Analista, true, Arg.Any<CancellationToken>()).Returns([]);

        Resultado<ResultadoPaginado<UsuarioResumenDto>> resultado =
            await ObtenerUsuarios(new ObtenerUsuariosPaginadoQuery(RolUsuario.Analista));

        Assert.Empty(resultado.Valor.Elementos);
        Assert.Equal(0, resultado.Valor.TotalElementos);
    }

    [Fact]
    public async Task ObtenerUsuariosPaginado_SinRol_PaginaEnElRepositorioConLosParametrosDeLaConsulta()
    {
        ParametrosPaginacion? paginacionRecibida = null;
        _usuarios.ObtenerPaginadoAsync(Arg.Do<ParametrosPaginacion>(paginacion => paginacionRecibida = paginacion), false, Arg.Any<CancellationToken>())
            .Returns(new ResultadoPaginado<Usuario>(
                [DatosPrueba.Usuario(DatosPrueba.ID_SOLICITANTE_UNO, RolUsuario.Solicitante, activo: false)],
                TOTAL_ELEMENTOS, PAGINA, TAMANO_PAGINA));

        Resultado<ResultadoPaginado<UsuarioResumenDto>> resultado = await ObtenerUsuarios(
            new ObtenerUsuariosPaginadoQuery(null, SoloActivos: false, Pagina: PAGINA, TamanoPagina: TAMANO_PAGINA));

        Assert.Equal(PAGINA, paginacionRecibida!.Pagina);
        Assert.Equal(TAMANO_PAGINA, paginacionRecibida.TamanoPagina);
        Assert.Equal(TOTAL_ELEMENTOS, resultado.Valor.TotalElementos);
        Assert.False(Assert.Single(resultado.Valor.Elementos).Activo);
    }

    private Usuario PrepararUsuario(int id, RolUsuario rol)
    {
        Usuario usuario = DatosPrueba.Usuario(id, rol);
        _usuarios.ObtenerPorIdAsync(id, Arg.Any<CancellationToken>()).Returns(usuario);

        return usuario;
    }

    private Task<Resultado<UsuarioResumenDto>> CrearUsuario(RolUsuario rol) =>
        new CrearUsuarioCommandHandler(_unitOfWork, _hasheadorPasswords)
            .HandleAsync(new CrearUsuarioCommand("Usuario Nuevo", EMAIL, PASSWORD, rol));

    private Task<Resultado<UsuarioResumenDto>> ActualizarUsuario(IUsuarioActual usuarioActual, ActualizarUsuarioCommand comando) =>
        new ActualizarUsuarioCommandHandler(_unitOfWork, usuarioActual).HandleAsync(comando);

    private Task<Resultado<ResultadoPaginado<UsuarioResumenDto>>> ObtenerUsuarios(ObtenerUsuariosPaginadoQuery consulta) =>
        new ObtenerUsuariosPaginadoQueryHandler(_unitOfWork).HandleAsync(consulta);
}

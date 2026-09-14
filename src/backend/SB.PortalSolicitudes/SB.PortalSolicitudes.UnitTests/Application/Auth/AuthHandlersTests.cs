using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Auth.Commands.IniciarSesion;
using SB.PortalSolicitudes.Application.Features.Auth.Commands.RegistrarUsuario;
using SB.PortalSolicitudes.Application.Features.Auth.Commands.RenovarSesion;
using SB.PortalSolicitudes.Application.Features.Auth.Dtos;
using SB.PortalSolicitudes.Application.Features.Auth.Queries.ObtenerUsuarioActual;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Auth;

/// <summary>
/// Autenticacion (ADR-0007, ADR-0019): el hash y el JWT se simulan; lo que se prueba son
/// las decisiones del handler — que credencial es valida, que un usuario desactivado no
/// entra ni renueva, y que el auto-registro nunca crea otro rol que Solicitante.
/// </summary>
public class AuthHandlersTests
{
    private const string EMAIL = "usuario4@portalsolicitudes.test";
    private const string PASSWORD = "Password123!";
    private const string HASH = "hash";
    private const string HASH_GENERADO = "hash-bcrypt-generado";
    private const string TOKEN = "jwt-de-prueba";

    private static readonly DateTime EXPIRA_EN = DatosPrueba.AHORA.AddHours(1);

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IHasheadorPasswords _hasheadorPasswords = Substitute.For<IHasheadorPasswords>();
    private readonly IProveedorTokens _proveedorTokens = Substitute.For<IProveedorTokens>();

    public AuthHandlersTests()
    {
        _unitOfWork.Usuarios.Returns(_usuarios);
        _proveedorTokens.GenerarToken(Arg.Any<Usuario>()).Returns((TOKEN, EXPIRA_EN));
        _hasheadorPasswords.Verificar(PASSWORD, HASH).Returns(true);
        _hasheadorPasswords.Hashear(PASSWORD).Returns(HASH_GENERADO);
    }

    // ---- IniciarSesion ----

    [Fact]
    public async Task IniciarSesion_CredencialesValidas_DevuelveTokenYUsuarioSinHash()
    {
        Usuario usuario = PrepararUsuarioPorEmail(activo: true);

        Resultado<SesionDto> resultado = await IniciarSesion(PASSWORD);

        Assert.True(resultado.EsExitoso);
        Assert.Equal(TOKEN, resultado.Valor.Token);
        Assert.Equal(EXPIRA_EN, resultado.Valor.ExpiraEn);
        Assert.Equal(new UsuarioResumenDto(usuario.Id, usuario.Nombre, EMAIL, nameof(RolUsuario.Solicitante), true), resultado.Valor.Usuario);
        _proveedorTokens.Received(1).GenerarToken(usuario);
    }

    [Fact]
    public async Task IniciarSesion_PasswordIncorrecta_DevuelveNoAutorizadoSinToken()
    {
        PrepararUsuarioPorEmail(activo: true);

        Resultado<SesionDto> resultado = await IniciarSesion("otra-password");

        AfirmarCredencialesInvalidas(resultado);
    }

    [Fact]
    public async Task IniciarSesion_UsuarioDesactivadoConPasswordCorrecta_DevuelveNoAutorizado()
    {
        PrepararUsuarioPorEmail(activo: false);

        Resultado<SesionDto> resultado = await IniciarSesion(PASSWORD);

        AfirmarCredencialesInvalidas(resultado);
    }

    [Fact]
    public async Task IniciarSesion_EmailInexistente_DevuelveElMismoErrorQuePasswordIncorrecta()
    {
        // Mismo codigo para "no existe" y "password incorrecta": no revela que correos estan registrados.
        Resultado<SesionDto> resultado = await IniciarSesion(PASSWORD);

        AfirmarCredencialesInvalidas(resultado);
    }

    // ---- RegistrarUsuario ----

    [Fact]
    public async Task RegistrarUsuario_EmailNuevo_CreaSolicitanteActivoConPasswordHasheada()
    {
        Resultado<SesionDto> resultado = await RegistrarUsuario();

        Assert.True(resultado.EsExitoso);
        Assert.Equal(TOKEN, resultado.Valor.Token);
        Assert.Equal(nameof(RolUsuario.Solicitante), resultado.Valor.Usuario.Rol);
        await _usuarios.Received(1).AgregarAsync(
            Arg.Is<Usuario>(usuario =>
                usuario.Email == EMAIL
                && usuario.Rol == RolUsuario.Solicitante
                && usuario.Activo
                && usuario.PasswordHash == HASH_GENERADO),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegistrarUsuario_EmailExistente_DevuelveConflictoSinCrear()
    {
        _usuarios.ExisteEmailAsync(EMAIL, null, Arg.Any<CancellationToken>()).Returns(true);

        Resultado<SesionDto> resultado = await RegistrarUsuario();

        Assert.Equal(TipoError.Conflicto, resultado.Error.Tipo);
        await _usuarios.DidNotReceiveWithAnyArgs().AgregarAsync(default!, default);
        _proveedorTokens.DidNotReceiveWithAnyArgs().GenerarToken(default!);
    }

    // ---- RenovarSesion ----

    [Fact]
    public async Task RenovarSesion_UsuarioActivo_EmiteTokenConLosDatosActualesDeLaBase()
    {
        // El token viejo decia Solicitante, pero en la base ya es Analista: el token renovado
        // debe salir del usuario de la base, no de los claims del token anterior.
        Usuario usuario = PrepararUsuarioPorId(RolUsuario.Analista, activo: true);

        Resultado<SesionDto> resultado = await RenovarSesion(DatosPrueba.SolicitanteUno());

        Assert.True(resultado.EsExitoso);
        Assert.Equal(nameof(RolUsuario.Analista), resultado.Valor.Usuario.Rol);
        _proveedorTokens.Received(1).GenerarToken(usuario);
    }

    [Fact]
    public async Task RenovarSesion_UsuarioDesactivadoDespuesDeEmitirElToken_DevuelveNoAutorizado()
    {
        PrepararUsuarioPorId(RolUsuario.Solicitante, activo: false);

        Resultado<SesionDto> resultado = await RenovarSesion(DatosPrueba.SolicitanteUno());

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        Assert.Equal("Auth.SesionInvalida", resultado.Error.Codigo);
        _proveedorTokens.DidNotReceiveWithAnyArgs().GenerarToken(default!);
    }

    [Fact]
    public async Task RenovarSesion_UsuarioEliminado_DevuelveNoAutorizado()
    {
        Resultado<SesionDto> resultado = await RenovarSesion(DatosPrueba.SolicitanteUno());

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
    }

    [Fact]
    public async Task RenovarSesion_SinSesion_DevuelveNoAutorizadoSinConsultar()
    {
        Resultado<SesionDto> resultado = await RenovarSesion(DatosPrueba.UsuarioActual(null, null));

        Assert.Equal("Auth.NoAutenticado", resultado.Error.Codigo);
        await _usuarios.DidNotReceiveWithAnyArgs().ObtenerPorIdAsync(default);
    }

    // ---- ObtenerUsuarioActual ----

    [Fact]
    public async Task ObtenerUsuarioActual_UsuarioExistente_DevuelveSuResumen()
    {
        PrepararUsuarioPorId(RolUsuario.Solicitante, activo: true);

        Resultado<UsuarioResumenDto> resultado = await ObtenerUsuarioActual(DatosPrueba.SolicitanteUno());

        Assert.Equal(DatosPrueba.ID_SOLICITANTE_UNO, resultado.Valor.Id);
        Assert.Equal(nameof(RolUsuario.Solicitante), resultado.Valor.Rol);
    }

    [Fact]
    public async Task ObtenerUsuarioActual_UsuarioYaNoExiste_DevuelveNoEncontrado()
    {
        Resultado<UsuarioResumenDto> resultado = await ObtenerUsuarioActual(DatosPrueba.SolicitanteUno());

        Assert.Equal(TipoError.NoEncontrado, resultado.Error.Tipo);
    }

    [Fact]
    public async Task ObtenerUsuarioActual_SinSesion_DevuelveNoAutorizado()
    {
        Resultado<UsuarioResumenDto> resultado = await ObtenerUsuarioActual(DatosPrueba.UsuarioActual(null, null));

        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
    }

    private Usuario PrepararUsuarioPorEmail(bool activo)
    {
        Usuario usuario = DatosPrueba.Usuario(DatosPrueba.ID_SOLICITANTE_UNO, RolUsuario.Solicitante, activo);
        _usuarios.ObtenerPorEmailAsync(EMAIL, Arg.Any<CancellationToken>()).Returns(usuario);

        return usuario;
    }

    private Usuario PrepararUsuarioPorId(RolUsuario rol, bool activo)
    {
        Usuario usuario = DatosPrueba.Usuario(DatosPrueba.ID_SOLICITANTE_UNO, rol, activo);
        _usuarios.ObtenerPorIdAsync(DatosPrueba.ID_SOLICITANTE_UNO, Arg.Any<CancellationToken>()).Returns(usuario);

        return usuario;
    }

    private void AfirmarCredencialesInvalidas(Resultado<SesionDto> resultado)
    {
        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
        Assert.Equal("Auth.CredencialesInvalidas", resultado.Error.Codigo);
        _proveedorTokens.DidNotReceiveWithAnyArgs().GenerarToken(default!);
    }

    private Task<Resultado<SesionDto>> IniciarSesion(string password) =>
        new IniciarSesionCommandHandler(
                _unitOfWork, _hasheadorPasswords, _proveedorTokens, NullLogger<IniciarSesionCommandHandler>.Instance)
            .HandleAsync(new IniciarSesionCommand(EMAIL, password));

    private Task<Resultado<SesionDto>> RegistrarUsuario() =>
        new RegistrarUsuarioCommandHandler(
                _unitOfWork, _hasheadorPasswords, _proveedorTokens, NullLogger<RegistrarUsuarioCommandHandler>.Instance)
            .HandleAsync(new RegistrarUsuarioCommand("Usuario Nuevo", EMAIL, PASSWORD));

    private Task<Resultado<SesionDto>> RenovarSesion(IUsuarioActual usuarioActual) =>
        new RenovarSesionCommandHandler(
                _unitOfWork, usuarioActual, _proveedorTokens, NullLogger<RenovarSesionCommandHandler>.Instance)
            .HandleAsync(new RenovarSesionCommand());

    private Task<Resultado<UsuarioResumenDto>> ObtenerUsuarioActual(IUsuarioActual usuarioActual) =>
        new ObtenerUsuarioActualQueryHandler(_unitOfWork, usuarioActual).HandleAsync(new ObtenerUsuarioActualQuery());
}

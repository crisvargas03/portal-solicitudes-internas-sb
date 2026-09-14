using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Seed.Commands;
using SB.PortalSolicitudes.Application.Features.Seed.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Seed;

/// <summary>
/// Preparacion del entorno de evaluacion (ADR-0008, amendada): bloqueada fuera de
/// Development/<c>Seed:Habilitado</c>, aplica migraciones e idempotente — re-ejecutarla no
/// duplica usuarios ni solicitudes de demostracion.
/// </summary>
public class SeedCommandHandlerTests
{
    private const int CANTIDAD_USUARIOS_DEMO = 5;
    private const int CANTIDAD_SOLICITUDES_DEMO = 5;
    private const int CANTIDAD_ADMINISTRADORES_DEMO = 1;
    private const int CANTIDAD_ANALISTAS_DEMO = 2;
    private const int CANTIDAD_SOLICITANTES_DEMO = 2;
    private const string CODIGO_MARCADOR_DEMO = "SOL-DEMO-0001";
    private const string HASH = "hash-demo";

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();
    private readonly IEstadoSolicitudRepository _estados = Substitute.For<IEstadoSolicitudRepository>();
    private readonly IHistorialEstadoRepository _historial = Substitute.For<IHistorialEstadoRepository>();
    private readonly IHasheadorPasswords _hasheadorPasswords = Substitute.For<IHasheadorPasswords>();
    private readonly IProveedorFechaHora _proveedorFechaHora = Substitute.For<IProveedorFechaHora>();
    private readonly IEntornoEjecucion _entornoEjecucion = Substitute.For<IEntornoEjecucion>();
    private readonly IPreparadorBaseDeDatos _preparadorBaseDeDatos = Substitute.For<IPreparadorBaseDeDatos>();

    private readonly List<Solicitud> _solicitudesCreadas = [];

    public SeedCommandHandlerTests()
    {
        _unitOfWork.Usuarios.Returns(_usuarios);
        _unitOfWork.Solicitudes.Returns(_solicitudes);
        _unitOfWork.EstadosSolicitud.Returns(_estados);
        _unitOfWork.HistorialEstados.Returns(_historial);
        _proveedorFechaHora.Ahora.Returns(DatosPrueba.AHORA);
        _hasheadorPasswords.Hashear(Arg.Any<string>()).Returns(HASH);
        _entornoEjecucion.SeedHabilitado.Returns(true);

        EstadoSolicitud[] estados =
            [DatosPrueba.Registrada(), DatosPrueba.EnAnalisis(), DatosPrueba.EnProgreso(), DatosPrueba.Resuelta(), DatosPrueba.Cerrada()];
        _estados.ObtenerPorCodigoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(llamada => estados.SingleOrDefault(estado => estado.Codigo == llamada.Arg<string>()));

        // Simula la identidad que asigna la base de datos: sin ella todas las solicitudes
        // tendrian Id 0 y su historial no se podria distinguir.
        _solicitudes.When(repositorio => repositorio.AgregarAsync(Arg.Any<Solicitud>(), Arg.Any<CancellationToken>()))
            .Do(llamada =>
            {
                Solicitud solicitud = llamada.Arg<Solicitud>();
                _solicitudesCreadas.Add(solicitud);
                solicitud.Id = _solicitudesCreadas.Count;
            });
    }

    [Fact]
    public async Task HandleAsync_SeedDeshabilitado_DevuelveProhibidoSinTocarLaBase()
    {
        _entornoEjecucion.SeedHabilitado.Returns(false);

        Resultado<SeedResultadoDto> resultado = await Ejecutar();

        Assert.Equal(TipoError.Prohibido, resultado.Error.Tipo);
        await _preparadorBaseDeDatos.DidNotReceiveWithAnyArgs().AplicarMigracionesPendientesAsync(default);
        await _usuarios.DidNotReceiveWithAnyArgs().AgregarAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().GuardarCambiosAsync(default);
    }

    [Fact]
    public async Task HandleAsync_BaseVacia_AplicaMigracionesAntesDeCargarDatos()
    {
        List<string> eventos = [];
        _preparadorBaseDeDatos.When(preparador => preparador.AplicarMigracionesPendientesAsync(Arg.Any<CancellationToken>()))
            .Do(_ => eventos.Add(nameof(IPreparadorBaseDeDatos.AplicarMigracionesPendientesAsync)));
        _usuarios.When(repositorio => repositorio.ObtenerPorEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()))
            .Do(_ => eventos.Add(nameof(IUsuarioRepository.ObtenerPorEmailAsync)));

        await Ejecutar();

        Assert.Equal(nameof(IPreparadorBaseDeDatos.AplicarMigracionesPendientesAsync), eventos.First());
        Assert.Contains(nameof(IUsuarioRepository.ObtenerPorEmailAsync), eventos);
    }

    [Fact]
    public async Task HandleAsync_BaseVacia_CreaUnUsuarioPorRolDemoConPasswordHasheada()
    {
        await Ejecutar();

        await _usuarios.Received(CANTIDAD_USUARIOS_DEMO).AgregarAsync(
            Arg.Is<Usuario>(usuario => usuario.Activo && usuario.PasswordHash == HASH), Arg.Any<CancellationToken>());
        await _usuarios.Received(CANTIDAD_ADMINISTRADORES_DEMO).AgregarAsync(
            Arg.Is<Usuario>(usuario => usuario.Rol == RolUsuario.Administrador), Arg.Any<CancellationToken>());
        await _usuarios.Received(CANTIDAD_ANALISTAS_DEMO).AgregarAsync(
            Arg.Is<Usuario>(usuario => usuario.Rol == RolUsuario.Analista), Arg.Any<CancellationToken>());
        await _usuarios.Received(CANTIDAD_SOLICITANTES_DEMO).AgregarAsync(
            Arg.Is<Usuario>(usuario => usuario.Rol == RolUsuario.Solicitante), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_BaseVacia_CreaSolicitudesDemoCuyoEstadoEsElUltimoPasoDeSuHistorial()
    {
        List<HistorialEstado> historialCreado = [];
        _historial.When(repositorio => repositorio.AgregarAsync(Arg.Any<HistorialEstado>(), Arg.Any<CancellationToken>()))
            .Do(llamada => historialCreado.Add(llamada.Arg<HistorialEstado>()));

        await Ejecutar();

        Assert.Equal(CANTIDAD_SOLICITUDES_DEMO, _solicitudesCreadas.Count);
        Assert.Contains(_solicitudesCreadas, solicitud => solicitud.Codigo == CODIGO_MARCADOR_DEMO);

        Assert.All(_solicitudesCreadas, solicitud =>
        {
            HistorialEstado? ultimoPaso = historialCreado.LastOrDefault(paso => paso.SolicitudId == solicitud.Id);

            Assert.NotNull(ultimoPaso);
            Assert.Equal(solicitud.EstadoId, ultimoPaso.EstadoNuevoId);
        });
    }

    [Fact]
    public async Task HandleAsync_BaseVacia_ElPasoACerradaSoloOcurreDesdeResueltaConComentarioDeResolucion()
    {
        List<HistorialEstado> historialCreado = [];
        _historial.When(repositorio => repositorio.AgregarAsync(Arg.Any<HistorialEstado>(), Arg.Any<CancellationToken>()))
            .Do(llamada => historialCreado.Add(llamada.Arg<HistorialEstado>()));

        await Ejecutar();

        // Los datos de demostracion deben respetar las mismas reglas que la aplicacion (ADR-0001, ADR-0002).
        Assert.Contains(historialCreado, paso => paso.EstadoNuevoId == DatosPrueba.ID_ESTADO_CERRADA);
        Assert.All(
            historialCreado.Where(paso => paso.EstadoNuevoId == DatosPrueba.ID_ESTADO_CERRADA),
            paso => Assert.Equal(DatosPrueba.ID_ESTADO_RESUELTA, paso.EstadoAnteriorId));
        Assert.All(
            historialCreado.Where(paso => paso.EstadoNuevoId == DatosPrueba.ID_ESTADO_RESUELTA),
            paso => Assert.False(string.IsNullOrWhiteSpace(paso.Comentario)));
    }

    [Fact]
    public async Task HandleAsync_DatosYaCargados_NoDuplicaUsuariosNiSolicitudes()
    {
        _usuarios.ObtenerPorEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => DatosPrueba.Usuario(DatosPrueba.ID_ANALISTA_UNO, RolUsuario.Analista));
        _solicitudes.ExisteCodigoAsync(CODIGO_MARCADOR_DEMO, Arg.Any<CancellationToken>()).Returns(true);

        Resultado<SeedResultadoDto> resultado = await Ejecutar();

        Assert.True(resultado.EsExitoso);
        await _usuarios.DidNotReceiveWithAnyArgs().AgregarAsync(default!, default);
        await _solicitudes.DidNotReceiveWithAnyArgs().AgregarAsync(default!, default);
        await _preparadorBaseDeDatos.Received(1).AplicarMigracionesPendientesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Exitoso_DevuelveLasCredencialesDeLosTresRoles()
    {
        Resultado<SeedResultadoDto> resultado = await Ejecutar();

        Assert.Equal(CANTIDAD_USUARIOS_DEMO, resultado.Valor.Credenciales.Count);
        Assert.Equal(
            [nameof(RolUsuario.Administrador), nameof(RolUsuario.Analista), nameof(RolUsuario.Solicitante)],
            resultado.Valor.Credenciales.Select(credencial => credencial.Rol).Distinct());
    }

    private Task<Resultado<SeedResultadoDto>> Ejecutar() =>
        new SeedCommandHandler(_unitOfWork, _hasheadorPasswords, _proveedorFechaHora, _entornoEjecucion, _preparadorBaseDeDatos)
            .HandleAsync(new SeedCommand());
}

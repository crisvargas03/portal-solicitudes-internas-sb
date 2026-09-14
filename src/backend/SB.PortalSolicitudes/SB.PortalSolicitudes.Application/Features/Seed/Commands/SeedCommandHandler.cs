using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Seed.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Features.Seed.Commands;

public class SeedCommandHandler : ICommandHandler<SeedCommand, Resultado<SeedResultadoDto>>
{
    private const string PASSWORD_ADMIN = "Admin123!";
    private const string PASSWORD_ANALISTA = "Analista123!";
    private const string PASSWORD_SOLICITANTE = "Solicitante123!";

    private const string CODIGO_MARCADOR_DEMO = "SOL-DEMO-0001";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IHasheadorPasswords _hasheadorPasswords;
    private readonly IProveedorFechaHora _proveedorFechaHora;
    private readonly IEntornoEjecucion _entornoEjecucion;
    private readonly IPreparadorBaseDeDatos _preparadorBaseDeDatos;

    public SeedCommandHandler(
        IUnitOfWork unitOfWork,
        IHasheadorPasswords hasheadorPasswords,
        IProveedorFechaHora proveedorFechaHora,
        IEntornoEjecucion entornoEjecucion,
        IPreparadorBaseDeDatos preparadorBaseDeDatos)
    {
        _unitOfWork = unitOfWork;
        _hasheadorPasswords = hasheadorPasswords;
        _proveedorFechaHora = proveedorFechaHora;
        _entornoEjecucion = entornoEjecucion;
        _preparadorBaseDeDatos = preparadorBaseDeDatos;
    }

    public async Task<Resultado<SeedResultadoDto>> HandleAsync(SeedCommand command, CancellationToken cancellationToken = default)
    {
        if (!_entornoEjecucion.SeedHabilitado)
        {
            return Resultado.Fallido<SeedResultadoDto>(
                Error.Prohibido(
                    "Seed.Deshabilitado", "La carga de datos de demostracion no esta habilitada en este entorno."));
        }

        await _preparadorBaseDeDatos.AplicarMigracionesPendientesAsync(cancellationToken);

        Usuario admin = await AsegurarUsuarioAsync(
            "Administradora Demo", "admin@portalsolicitudes.test", PASSWORD_ADMIN, RolUsuario.Administrador, cancellationToken);
        Usuario analista1 = await AsegurarUsuarioAsync(
            "Analista Uno", "analista1@portalsolicitudes.test", PASSWORD_ANALISTA, RolUsuario.Analista, cancellationToken);
        Usuario analista2 = await AsegurarUsuarioAsync(
            "Analista Dos", "analista2@portalsolicitudes.test", PASSWORD_ANALISTA, RolUsuario.Analista, cancellationToken);
        Usuario solicitante1 = await AsegurarUsuarioAsync(
            "Solicitante Uno", "solicitante1@portalsolicitudes.test", PASSWORD_SOLICITANTE, RolUsuario.Solicitante, cancellationToken);
        Usuario solicitante2 = await AsegurarUsuarioAsync(
            "Solicitante Dos", "solicitante2@portalsolicitudes.test", PASSWORD_SOLICITANTE, RolUsuario.Solicitante, cancellationToken);

        bool yaTieneSolicitudesDemo = await _unitOfWork.Solicitudes.ExisteCodigoAsync(CODIGO_MARCADOR_DEMO, cancellationToken);

        if (!yaTieneSolicitudesDemo)
        {
            await CrearSolicitudesDemoAsync(admin, analista1, analista2, solicitante1, solicitante2, cancellationToken);
        }

        List<CredencialDemoDto> credenciales =
        [
            new("admin@portalsolicitudes.test", PASSWORD_ADMIN, nameof(RolUsuario.Administrador)),
            new("analista1@portalsolicitudes.test", PASSWORD_ANALISTA, nameof(RolUsuario.Analista)),
            new("analista2@portalsolicitudes.test", PASSWORD_ANALISTA, nameof(RolUsuario.Analista)),
            new("solicitante1@portalsolicitudes.test", PASSWORD_SOLICITANTE, nameof(RolUsuario.Solicitante)),
            new("solicitante2@portalsolicitudes.test", PASSWORD_SOLICITANTE, nameof(RolUsuario.Solicitante))
        ];

        return new SeedResultadoDto(credenciales);
    }

    private async Task<Usuario> AsegurarUsuarioAsync(
        string nombre, string email, string password, RolUsuario rol, CancellationToken cancellationToken)
    {
        Usuario? existente = await _unitOfWork.Usuarios.ObtenerPorEmailAsync(email, cancellationToken);

        if (existente is not null)
        {
            return existente;
        }

        Usuario usuario = new()
        {
            Nombre = nombre,
            Email = email,
            Rol = rol,
            PasswordHash = _hasheadorPasswords.Hashear(password),
            Activo = true
        };

        await _unitOfWork.Usuarios.AgregarAsync(usuario, cancellationToken);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return usuario;
    }

    private async Task CrearSolicitudesDemoAsync(
        Usuario admin, Usuario analista1, Usuario analista2, Usuario solicitante1, Usuario solicitante2,
        CancellationToken cancellationToken)
    {
        EstadoSolicitud registrada = (await _unitOfWork.EstadosSolicitud.ObtenerPorCodigoAsync(
            CodigosEstadoSolicitud.REGISTRADA, cancellationToken))!;
        EstadoSolicitud enAnalisis = (await _unitOfWork.EstadosSolicitud.ObtenerPorCodigoAsync(
            CodigosEstadoSolicitud.EN_ANALISIS, cancellationToken))!;
        EstadoSolicitud enProgreso = (await _unitOfWork.EstadosSolicitud.ObtenerPorCodigoAsync(
            CodigosEstadoSolicitud.EN_PROGRESO, cancellationToken))!;
        EstadoSolicitud resuelta = (await _unitOfWork.EstadosSolicitud.ObtenerPorCodigoAsync(
            CodigosEstadoSolicitud.RESUELTA, cancellationToken))!;
        EstadoSolicitud cerrada = (await _unitOfWork.EstadosSolicitud.ObtenerPorCodigoAsync(
            CodigosEstadoSolicitud.CERRADA, cancellationToken))!;

        DateTime ahora = _proveedorFechaHora.Ahora;

        await CrearSolicitudConHistorialAsync(
            "SOL-DEMO-0001", "Impresora no enciende", "La impresora del tercer piso no enciende desde ayer.",
            1, 1, 1, solicitante1, null,
            [(null, registrada, solicitante1, null)],
            ahora.AddDays(-5), null, cancellationToken);

        await CrearSolicitudConHistorialAsync(
            "SOL-DEMO-0002", "Acceso al sistema de nomina", "Solicito acceso de consulta al sistema de nomina.",
            2, 2, 2, solicitante1, analista1,
            [(null, registrada, solicitante1, null), (registrada, enAnalisis, analista1, null)],
            ahora.AddDays(-4), ahora.AddDays(3), cancellationToken);

        await CrearSolicitudConHistorialAsync(
            "SOL-DEMO-0003", "Instalar software de diseño", "Necesito licencia de Figma para el area de diseño.",
            4, 3, 1, solicitante2, analista1,
            [
                (null, registrada, solicitante2, null),
                (registrada, enAnalisis, analista1, null),
                (enAnalisis, enProgreso, analista1, null)
            ],
            ahora.AddDays(-10), ahora.AddDays(-2), cancellationToken);

        await CrearSolicitudConHistorialAsync(
            "SOL-DEMO-0004", "Reemplazo de laptop", "La laptop presenta fallas de encendido intermitentes.",
            3, 4, 1, solicitante2, analista2,
            [
                (null, registrada, solicitante2, null),
                (registrada, enAnalisis, analista2, null),
                (enAnalisis, enProgreso, analista2, null),
                (enProgreso, resuelta, analista2, "Se reemplazo el equipo por uno nuevo.")
            ],
            ahora.AddDays(-15), null, cancellationToken);

        await CrearSolicitudConHistorialAsync(
            "SOL-DEMO-0005", "Consulta sobre politica de VPN", "¿Cual es el procedimiento para solicitar acceso VPN remoto?",
            5, 1, 5, solicitante1, admin,
            [
                (null, registrada, solicitante1, null),
                (registrada, enAnalisis, admin, null),
                (enAnalisis, enProgreso, admin, null),
                (enProgreso, resuelta, admin, "Se envio el procedimiento por correo."),
                (resuelta, cerrada, admin, null)
            ],
            ahora.AddDays(-20), null, cancellationToken);
    }

    private async Task CrearSolicitudConHistorialAsync(
        string codigo,
        string titulo,
        string descripcion,
        int tipoSolicitudId,
        int prioridadId,
        int areaId,
        Usuario solicitante,
        Usuario? asignado,
        IReadOnlyList<(EstadoSolicitud? Anterior, EstadoSolicitud Nuevo, Usuario Usuario, string? Comentario)> pasos,
        DateTime fechaCreacion,
        DateTime? fechaCompromiso,
        CancellationToken cancellationToken)
    {
        Solicitud solicitud = new()
        {
            Codigo = codigo,
            Titulo = titulo,
            Descripcion = descripcion,
            FechaCreacion = fechaCreacion,
            FechaCompromiso = fechaCompromiso,
            PrioridadId = prioridadId,
            EstadoId = pasos[^1].Nuevo.Id,
            AreaId = areaId,
            TipoSolicitudId = tipoSolicitudId,
            UsuarioSolicitanteId = solicitante.Id,
            UsuarioAsignadoId = asignado?.Id
        };

        await _unitOfWork.Solicitudes.AgregarAsync(solicitud, cancellationToken);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        DateTime fechaPaso = fechaCreacion;

        foreach ((EstadoSolicitud? anterior, EstadoSolicitud nuevo, Usuario usuario, string? comentario) in pasos)
        {
            HistorialEstado historial = new()
            {
                SolicitudId = solicitud.Id,
                EstadoAnteriorId = anterior?.Id,
                EstadoNuevoId = nuevo.Id,
                UsuarioId = usuario.Id,
                Comentario = comentario,
                Fecha = fechaPaso
            };

            await _unitOfWork.HistorialEstados.AgregarAsync(historial, cancellationToken);

            fechaPaso = fechaPaso.AddDays(1);
        }

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);
    }
}

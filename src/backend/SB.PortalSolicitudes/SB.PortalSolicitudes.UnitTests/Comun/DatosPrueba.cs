using NSubstitute;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.UnitTests.Comun;

/// <summary>
/// Datos de prueba compartidos. Los identificadores son constantes con nombre (ver
/// docs/conventions.md: sin numeros magicos) y reproducen el reparto de la semilla de
/// demostracion: dos solicitantes, dos analistas y una administradora.
/// </summary>
public static class DatosPrueba
{
    public const int ID_ADMINISTRADOR = 1;
    public const int ID_ANALISTA_UNO = 2;
    public const int ID_ANALISTA_DOS = 3;
    public const int ID_SOLICITANTE_UNO = 4;
    public const int ID_SOLICITANTE_DOS = 5;

    public const int ID_SOLICITUD = 100;
    public const int ID_CATALOGO = 1;
    public const int NIVEL_PRIORIDAD_MEDIA = 2;

    public const int ID_ESTADO_REGISTRADA = 1;
    public const int ID_ESTADO_EN_ANALISIS = 2;
    public const int ID_ESTADO_EN_PROGRESO = 3;
    public const int ID_ESTADO_RESUELTA = 5;
    public const int ID_ESTADO_CERRADA = 6;

    public static readonly DateTime AHORA = new(2026, 9, 14, 10, 0, 0, DateTimeKind.Utc);

    public static IUsuarioActual UsuarioActual(int? id, RolUsuario? rol)
    {
        IUsuarioActual usuarioActual = Substitute.For<IUsuarioActual>();
        usuarioActual.Id.Returns(id);
        usuarioActual.Rol.Returns(rol);
        usuarioActual.EstaAutenticado.Returns(id is not null);

        return usuarioActual;
    }

    public static IUsuarioActual Administrador() => UsuarioActual(ID_ADMINISTRADOR, RolUsuario.Administrador);

    public static IUsuarioActual AnalistaUno() => UsuarioActual(ID_ANALISTA_UNO, RolUsuario.Analista);

    public static IUsuarioActual AnalistaDos() => UsuarioActual(ID_ANALISTA_DOS, RolUsuario.Analista);

    public static IUsuarioActual SolicitanteUno() => UsuarioActual(ID_SOLICITANTE_UNO, RolUsuario.Solicitante);

    public static IUsuarioActual SolicitanteDos() => UsuarioActual(ID_SOLICITANTE_DOS, RolUsuario.Solicitante);

    public const RolUsuario ROL_INEXISTENTE = (RolUsuario)999;

    /// <summary>
    /// Id y Rol presentes, pero un rol que no existe en el enum: pasa los chequeos de "hay
    /// sesion" y solo lo detiene la falla cerrada del alcance (ADR-0029).
    /// </summary>
    public static IUsuarioActual ConRolDesconocido() => UsuarioActual(ID_SOLICITANTE_UNO, ROL_INEXISTENTE);

    public static Usuario Usuario(int id, RolUsuario rol, bool activo = true) => new()
    {
        Id = id,
        Nombre = $"{rol} {id}",
        Email = $"usuario{id}@portalsolicitudes.test",
        PasswordHash = "hash",
        Rol = rol,
        Activo = activo
    };

    public static EstadoSolicitud Estado(int id, string codigo, bool esFinal = false) => new()
    {
        Id = id,
        Codigo = codigo,
        Nombre = codigo,
        Orden = id,
        EsFinal = esFinal
    };

    public static EstadoSolicitud Registrada() => Estado(ID_ESTADO_REGISTRADA, CodigosEstadoSolicitud.REGISTRADA);

    public static EstadoSolicitud EnAnalisis() => Estado(ID_ESTADO_EN_ANALISIS, CodigosEstadoSolicitud.EN_ANALISIS);

    public static EstadoSolicitud EnProgreso() => Estado(ID_ESTADO_EN_PROGRESO, CodigosEstadoSolicitud.EN_PROGRESO);

    public static EstadoSolicitud Resuelta() => Estado(ID_ESTADO_RESUELTA, CodigosEstadoSolicitud.RESUELTA);

    public static EstadoSolicitud Cerrada() => Estado(ID_ESTADO_CERRADA, CodigosEstadoSolicitud.CERRADA, esFinal: true);

    /// <summary>
    /// Solicitud con todas sus navegaciones cargadas, como la devuelve
    /// <c>ObtenerDetalleAsync</c>: lista para pasar por <c>MapeosSolicitud</c>.
    /// </summary>
    public static Solicitud Solicitud(
        EstadoSolicitud estado, int usuarioSolicitanteId = ID_SOLICITANTE_UNO, int? usuarioAsignadoId = null)
    {
        return new Solicitud
        {
            Id = ID_SOLICITUD,
            Codigo = "SOL-2026-0001",
            Titulo = "Titulo de prueba",
            Descripcion = "Descripcion de prueba",
            FechaCreacion = AHORA,
            EstadoId = estado.Id,
            Estado = estado,
            PrioridadId = ID_CATALOGO,
            Prioridad = new Prioridad { Id = ID_CATALOGO, Nombre = "Media", Nivel = NIVEL_PRIORIDAD_MEDIA },
            AreaId = ID_CATALOGO,
            Area = new Area { Id = ID_CATALOGO, Nombre = "Tecnologia" },
            TipoSolicitudId = ID_CATALOGO,
            TipoSolicitud = new TipoSolicitud { Id = ID_CATALOGO, Nombre = "Soporte" },
            UsuarioSolicitanteId = usuarioSolicitanteId,
            UsuarioSolicitante = Usuario(usuarioSolicitanteId, RolUsuario.Solicitante),
            UsuarioAsignadoId = usuarioAsignadoId,
            UsuarioAsignado = usuarioAsignadoId is null ? null : Usuario(usuarioAsignadoId.Value, RolUsuario.Analista)
        };
    }

    public static TransicionPermitida Transicion(
        EstadoSolicitud origen, EstadoSolicitud destino, bool requiereComentario, params RolUsuario[] roles)
    {
        return new TransicionPermitida
        {
            EstadoOrigenId = origen.Id,
            EstadoOrigen = origen,
            EstadoDestinoId = destino.Id,
            EstadoDestino = destino,
            RequiereComentario = requiereComentario,
            RolesPermitidos = roles.Select(rol => new TransicionPermitidaRol { Rol = rol }).ToList()
        };
    }
}

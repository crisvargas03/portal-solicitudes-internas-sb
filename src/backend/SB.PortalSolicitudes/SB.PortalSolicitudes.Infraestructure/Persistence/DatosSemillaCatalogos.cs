using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Infraestructure.Persistence;

/// <summary>
/// Catalogos estructurales, incluidos en la migracion mediante <c>HasData</c>: la
/// aplicacion no es valida sin ellos, porque la maquina de estados de ADR-0005 *es* el
/// contenido de <see cref="TransicionPermitida"/>. Los datos de demostracion (usuarios y
/// solicitudes de prueba) no viven aqui: se cargan a demanda por endpoint (ver ADR-0008).
/// </summary>
public static class DatosSemillaCatalogos
{
    public static void Aplicar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EstadoSolicitud>().HasData(ConstruirEstados());
        modelBuilder.Entity<Prioridad>().HasData(ConstruirPrioridades());
        modelBuilder.Entity<Area>().HasData(ConstruirAreas());
        modelBuilder.Entity<TipoSolicitud>().HasData(ConstruirTiposSolicitud());
        modelBuilder.Entity<TransicionPermitida>().HasData(ConstruirTransiciones());
        modelBuilder.Entity<TransicionPermitidaRol>().HasData(ConstruirRolesDeTransiciones());
    }

    /// <summary>Flujo base del requerimiento, en orden.</summary>
    private static EstadoSolicitud[] ConstruirEstados()
    {
        return
        [
            new EstadoSolicitud
            {
                Id = IdentificadoresEstado.REGISTRADA,
                Codigo = CodigosEstadoSolicitud.REGISTRADA,
                Nombre = "Registrada",
                Orden = 1,
                EsFinal = false,
                Activo = true
            },
            new EstadoSolicitud
            {
                Id = IdentificadoresEstado.EN_ANALISIS,
                Codigo = CodigosEstadoSolicitud.EN_ANALISIS,
                Nombre = "En análisis",
                Orden = 2,
                EsFinal = false,
                Activo = true
            },
            new EstadoSolicitud
            {
                Id = IdentificadoresEstado.EN_PROGRESO,
                Codigo = CodigosEstadoSolicitud.EN_PROGRESO,
                Nombre = "En progreso",
                Orden = 3,
                EsFinal = false,
                Activo = true
            },
            new EstadoSolicitud
            {
                Id = IdentificadoresEstado.EN_ESPERA_SOLICITANTE,
                Codigo = CodigosEstadoSolicitud.EN_ESPERA_SOLICITANTE,
                Nombre = "En espera del solicitante",
                Orden = 4,
                EsFinal = false,
                Activo = true
            },
            new EstadoSolicitud
            {
                Id = IdentificadoresEstado.RESUELTA,
                Codigo = CodigosEstadoSolicitud.RESUELTA,
                Nombre = "Resuelta",
                Orden = 5,
                EsFinal = false,
                Activo = true
            },
            new EstadoSolicitud
            {
                Id = IdentificadoresEstado.CERRADA,
                Codigo = CodigosEstadoSolicitud.CERRADA,
                Nombre = "Cerrada",
                Orden = 6,
                EsFinal = true,
                Activo = true
            }
        ];
    }

    private static Prioridad[] ConstruirPrioridades()
    {
        return
        [
            new Prioridad { Id = IdentificadoresPrioridad.BAJA, Nombre = "Baja", Nivel = 1, Activo = true },
            new Prioridad { Id = IdentificadoresPrioridad.MEDIA, Nombre = "Media", Nivel = 2, Activo = true },
            new Prioridad { Id = IdentificadoresPrioridad.ALTA, Nombre = "Alta", Nivel = 3, Activo = true },
            new Prioridad { Id = IdentificadoresPrioridad.CRITICA, Nombre = "Crítica", Nivel = 4, Activo = true }
        ];
    }

    private static Area[] ConstruirAreas()
    {
        return
        [
            new Area { Id = 1, Nombre = "Tecnología de la Información", Activo = true },
            new Area { Id = 2, Nombre = "Supervisión Bancaria", Activo = true },
            new Area { Id = 3, Nombre = "Recursos Humanos", Activo = true },
            new Area { Id = 4, Nombre = "Administración y Finanzas", Activo = true },
            new Area { Id = 5, Nombre = "Consultoría Jurídica", Activo = true }
        ];
    }

    private static TipoSolicitud[] ConstruirTiposSolicitud()
    {
        return
        [
            new TipoSolicitud
            {
                Id = 1,
                Nombre = "Soporte técnico",
                Descripcion = "Fallas de equipos, red o servicios de tecnología.",
                Activo = true
            },
            new TipoSolicitud
            {
                Id = 2,
                Nombre = "Acceso a sistemas",
                Descripcion = "Altas, bajas y cambios de permisos en aplicaciones internas.",
                Activo = true
            },
            new TipoSolicitud
            {
                Id = 3,
                Nombre = "Equipo y hardware",
                Descripcion = "Solicitud, reemplazo o traslado de equipos.",
                Activo = true
            },
            new TipoSolicitud
            {
                Id = 4,
                Nombre = "Requerimiento de software",
                Descripcion = "Instalaciones, licencias y nuevas funcionalidades.",
                Activo = true
            },
            new TipoSolicitud
            {
                Id = 5,
                Nombre = "Consulta general",
                Descripcion = "Dudas y orientación que no encajan en otro tipo.",
                Activo = true
            }
        ];
    }

    /// <summary>
    /// Caminos autorizados de la maquina de estados. Aqui se materializan las reglas:
    /// la unica fila con destino <c>CERRADA</c> parte de <c>RESUELTA</c> (ADR-0002), y las
    /// filas que desembocan en <c>RESUELTA</c> exigen comentario (ADR-0001).
    /// </summary>
    private static TransicionPermitida[] ConstruirTransiciones()
    {
        return
        [
            new TransicionPermitida
            {
                Id = IdentificadoresTransicion.REGISTRADA_A_EN_ANALISIS,
                EstadoOrigenId = IdentificadoresEstado.REGISTRADA,
                EstadoDestinoId = IdentificadoresEstado.EN_ANALISIS,
                RequiereComentario = false,
                Activo = true
            },
            new TransicionPermitida
            {
                Id = IdentificadoresTransicion.EN_ANALISIS_A_EN_PROGRESO,
                EstadoOrigenId = IdentificadoresEstado.EN_ANALISIS,
                EstadoDestinoId = IdentificadoresEstado.EN_PROGRESO,
                RequiereComentario = false,
                Activo = true
            },
            new TransicionPermitida
            {
                Id = IdentificadoresTransicion.EN_PROGRESO_A_EN_ESPERA_SOLICITANTE,
                EstadoOrigenId = IdentificadoresEstado.EN_PROGRESO,
                EstadoDestinoId = IdentificadoresEstado.EN_ESPERA_SOLICITANTE,
                RequiereComentario = true,
                Activo = true
            },
            new TransicionPermitida
            {
                Id = IdentificadoresTransicion.EN_ESPERA_SOLICITANTE_A_EN_PROGRESO,
                EstadoOrigenId = IdentificadoresEstado.EN_ESPERA_SOLICITANTE,
                EstadoDestinoId = IdentificadoresEstado.EN_PROGRESO,
                RequiereComentario = false,
                Activo = true
            },
            // El comentario obligatorio de esta transicion es el comentario de resolucion.
            new TransicionPermitida
            {
                Id = IdentificadoresTransicion.EN_PROGRESO_A_RESUELTA,
                EstadoOrigenId = IdentificadoresEstado.EN_PROGRESO,
                EstadoDestinoId = IdentificadoresEstado.RESUELTA,
                RequiereComentario = true,
                Activo = true
            },
            new TransicionPermitida
            {
                Id = IdentificadoresTransicion.RESUELTA_A_EN_PROGRESO,
                EstadoOrigenId = IdentificadoresEstado.RESUELTA,
                EstadoDestinoId = IdentificadoresEstado.EN_PROGRESO,
                RequiereComentario = true,
                Activo = true
            },
            // Unica via de entrada a CERRADA.
            new TransicionPermitida
            {
                Id = IdentificadoresTransicion.RESUELTA_A_CERRADA,
                EstadoOrigenId = IdentificadoresEstado.RESUELTA,
                EstadoDestinoId = IdentificadoresEstado.CERRADA,
                RequiereComentario = false,
                Activo = true
            },
            // Reapertura: restringida por rol mas abajo.
            new TransicionPermitida
            {
                Id = IdentificadoresTransicion.CERRADA_A_EN_ANALISIS,
                EstadoOrigenId = IdentificadoresEstado.CERRADA,
                EstadoDestinoId = IdentificadoresEstado.EN_ANALISIS,
                RequiereComentario = true,
                Activo = true
            }
        ];
    }

    /// <summary>
    /// Roles autorizados por transicion. La reapertura desde <c>CERRADA</c> solo la
    /// permiten Administrador y Analista, como exige el requerimiento.
    /// </summary>
    private static List<TransicionPermitidaRol> ConstruirRolesDeTransiciones()
    {
        (int TransicionId, RolUsuario[] Roles)[] autorizaciones =
        [
            (IdentificadoresTransicion.REGISTRADA_A_EN_ANALISIS,
                [RolUsuario.Administrador, RolUsuario.Analista]),
            (IdentificadoresTransicion.EN_ANALISIS_A_EN_PROGRESO,
                [RolUsuario.Administrador, RolUsuario.Analista]),
            (IdentificadoresTransicion.EN_PROGRESO_A_EN_ESPERA_SOLICITANTE,
                [RolUsuario.Administrador, RolUsuario.Analista]),
            (IdentificadoresTransicion.EN_ESPERA_SOLICITANTE_A_EN_PROGRESO,
                [RolUsuario.Administrador, RolUsuario.Analista, RolUsuario.Solicitante]),
            (IdentificadoresTransicion.EN_PROGRESO_A_RESUELTA,
                [RolUsuario.Administrador, RolUsuario.Analista]),
            (IdentificadoresTransicion.RESUELTA_A_EN_PROGRESO,
                [RolUsuario.Administrador, RolUsuario.Analista]),
            (IdentificadoresTransicion.RESUELTA_A_CERRADA,
                [RolUsuario.Administrador, RolUsuario.Analista, RolUsuario.Solicitante]),
            (IdentificadoresTransicion.CERRADA_A_EN_ANALISIS,
                [RolUsuario.Administrador, RolUsuario.Analista])
        ];

        List<TransicionPermitidaRol> filas = new();
        int siguienteId = 1;

        foreach ((int transicionId, RolUsuario[] roles) in autorizaciones)
        {
            foreach (RolUsuario rol in roles)
            {
                filas.Add(new TransicionPermitidaRol
                {
                    Id = siguienteId,
                    TransicionPermitidaId = transicionId,
                    Rol = rol
                });

                siguienteId++;
            }
        }

        return filas;
    }

    /// <summary>Identificadores fijos de los estados sembrados.</summary>
    public static class IdentificadoresEstado
    {
        public const int REGISTRADA = 1;
        public const int EN_ANALISIS = 2;
        public const int EN_PROGRESO = 3;
        public const int EN_ESPERA_SOLICITANTE = 4;
        public const int RESUELTA = 5;
        public const int CERRADA = 6;
    }

    /// <summary>Identificadores fijos de las prioridades sembradas.</summary>
    public static class IdentificadoresPrioridad
    {
        public const int BAJA = 1;
        public const int MEDIA = 2;
        public const int ALTA = 3;
        public const int CRITICA = 4;
    }

    /// <summary>Identificadores fijos de las transiciones sembradas.</summary>
    public static class IdentificadoresTransicion
    {
        public const int REGISTRADA_A_EN_ANALISIS = 1;
        public const int EN_ANALISIS_A_EN_PROGRESO = 2;
        public const int EN_PROGRESO_A_EN_ESPERA_SOLICITANTE = 3;
        public const int EN_ESPERA_SOLICITANTE_A_EN_PROGRESO = 4;
        public const int EN_PROGRESO_A_RESUELTA = 5;
        public const int RESUELTA_A_EN_PROGRESO = 6;
        public const int RESUELTA_A_CERRADA = 7;
        public const int CERRADA_A_EN_ANALISIS = 8;
    }
}

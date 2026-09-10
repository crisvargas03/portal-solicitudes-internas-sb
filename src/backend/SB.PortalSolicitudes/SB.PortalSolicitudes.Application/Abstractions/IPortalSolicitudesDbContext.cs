using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Abstractions;

/// <summary>
/// Contrato de persistencia que consumen los casos de uso. Lo declara <c>Application</c> y
/// lo implementa <c>Infraestructure</c> (inversion de dependencias, ver docs/conventions.md):
/// asi la capa de aplicacion consulta y guarda sin conocer el <c>DbContext</c> concreto ni
/// el motor de base de datos.
/// </summary>
public interface IPortalSolicitudesDbContext
{
    DbSet<Usuario> Usuarios { get; }

    DbSet<Solicitud> Solicitudes { get; }

    DbSet<Area> Areas { get; }

    DbSet<TipoSolicitud> TiposSolicitud { get; }

    DbSet<Prioridad> Prioridades { get; }

    DbSet<EstadoSolicitud> EstadosSolicitud { get; }

    DbSet<TransicionPermitida> TransicionesPermitidas { get; }

    DbSet<TransicionPermitidaRol> TransicionesPermitidasRoles { get; }

    DbSet<HistorialEstado> HistorialEstados { get; }

    DbSet<Comentario> Comentarios { get; }

    DbSet<Adjunto> Adjuntos { get; }

    DbSet<Notificacion> Notificaciones { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

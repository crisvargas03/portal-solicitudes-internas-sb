using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence;

/// <summary>
/// Contexto de EF Core del portal. El mapeo no usa anotaciones en las entidades: vive en
/// las clases <c>IEntityTypeConfiguration</c> de esta misma carpeta, para que el proyecto
/// <c>Domain</c> siga sin dependencias (ver docs/architecture.md).
/// Las migraciones son la unica fuente de verdad del esquema (ver ADR-0008).
/// No se expone directamente a <c>Application</c>: los handlers dependen de
/// <c>IUnitOfWork</c> y de los repositorios (ver ADR-0009).
/// </summary>
public class PortalSolicitudesDbContext : DbContext
{
    public PortalSolicitudesDbContext(DbContextOptions<PortalSolicitudesDbContext> opciones)
        : base(opciones)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();

    public DbSet<Area> Areas => Set<Area>();

    public DbSet<TipoSolicitud> TiposSolicitud => Set<TipoSolicitud>();

    public DbSet<Prioridad> Prioridades => Set<Prioridad>();

    public DbSet<EstadoSolicitud> EstadosSolicitud => Set<EstadoSolicitud>();

    public DbSet<TransicionPermitida> TransicionesPermitidas => Set<TransicionPermitida>();

    public DbSet<TransicionPermitidaRol> TransicionesPermitidasRoles => Set<TransicionPermitidaRol>();

    public DbSet<HistorialEstado> HistorialEstados => Set<HistorialEstado>();

    public DbSet<Comentario> Comentarios => Set<Comentario>();

    public DbSet<Adjunto> Adjuntos => Set<Adjunto>();

    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortalSolicitudesDbContext).Assembly);

        DatosSemillaCatalogos.Aplicar(modelBuilder);
    }
}

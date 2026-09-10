using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Configurations;

public class SolicitudConfiguration : IEntityTypeConfiguration<Solicitud>
{
    public void Configure(EntityTypeBuilder<Solicitud> constructor)
    {
        constructor.ToTable("Solicitudes");

        constructor.HasKey(solicitud => solicitud.Id);

        constructor.Property(solicitud => solicitud.Codigo)
            .IsRequired()
            .HasMaxLength(Solicitud.MAX_LONGITUD_CODIGO);

        constructor.Property(solicitud => solicitud.Titulo)
            .IsRequired()
            .HasMaxLength(Solicitud.MAX_LONGITUD_TITULO);

        constructor.Property(solicitud => solicitud.Descripcion)
            .IsRequired()
            .HasMaxLength(Solicitud.MAX_LONGITUD_DESCRIPCION);

        constructor.Property(solicitud => solicitud.FechaCreacion)
            .IsRequired();

        // Los catalogos y los usuarios nunca se borran en cascada: una solicitud historica
        // no puede perder su area, su estado ni su solicitante.
        constructor.HasOne(solicitud => solicitud.Prioridad)
            .WithMany(prioridad => prioridad.Solicitudes)
            .HasForeignKey(solicitud => solicitud.PrioridadId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne(solicitud => solicitud.Estado)
            .WithMany(estado => estado.Solicitudes)
            .HasForeignKey(solicitud => solicitud.EstadoId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne(solicitud => solicitud.Area)
            .WithMany(area => area.Solicitudes)
            .HasForeignKey(solicitud => solicitud.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne(solicitud => solicitud.TipoSolicitud)
            .WithMany(tipoSolicitud => tipoSolicitud.Solicitudes)
            .HasForeignKey(solicitud => solicitud.TipoSolicitudId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne(solicitud => solicitud.UsuarioSolicitante)
            .WithMany(usuario => usuario.SolicitudesSolicitadas)
            .HasForeignKey(solicitud => solicitud.UsuarioSolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne(solicitud => solicitud.UsuarioAsignado)
            .WithMany(usuario => usuario.SolicitudesAsignadas)
            .HasForeignKey(solicitud => solicitud.UsuarioAsignadoId)
            .OnDelete(DeleteBehavior.Restrict);

        // El codigo legible es unico.
        constructor.HasIndex(solicitud => solicitud.Codigo)
            .IsUnique();

        constructor.HasIndex(solicitud => solicitud.EstadoId);
        constructor.HasIndex(solicitud => solicitud.PrioridadId);
        constructor.HasIndex(solicitud => solicitud.AreaId);
        constructor.HasIndex(solicitud => solicitud.TipoSolicitudId);
        constructor.HasIndex(solicitud => solicitud.UsuarioSolicitanteId);
        constructor.HasIndex(solicitud => solicitud.UsuarioAsignadoId);
        constructor.HasIndex(solicitud => solicitud.FechaCreacion);
        constructor.HasIndex(solicitud => solicitud.FechaCompromiso);
    }
}

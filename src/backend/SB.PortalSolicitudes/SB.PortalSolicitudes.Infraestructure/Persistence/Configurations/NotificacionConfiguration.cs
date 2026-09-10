using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Configurations;

public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> constructor)
    {
        constructor.ToTable("Notificaciones");

        constructor.HasKey(notificacion => notificacion.Id);

        constructor.Property(notificacion => notificacion.Asunto)
            .IsRequired()
            .HasMaxLength(Notificacion.MAX_LONGITUD_ASUNTO);

        constructor.Property(notificacion => notificacion.Mensaje)
            .IsRequired()
            .HasMaxLength(Notificacion.MAX_LONGITUD_MENSAJE);

        constructor.Property(notificacion => notificacion.Canal)
            .IsRequired()
            .HasConversion<int>();

        constructor.Property(notificacion => notificacion.Estado)
            .IsRequired()
            .HasConversion<int>();

        constructor.Property(notificacion => notificacion.Fecha)
            .IsRequired();

        constructor.HasOne(notificacion => notificacion.Solicitud)
            .WithMany(solicitud => solicitud.Notificaciones)
            .HasForeignKey(notificacion => notificacion.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        constructor.HasOne(notificacion => notificacion.UsuarioDestino)
            .WithMany()
            .HasForeignKey(notificacion => notificacion.UsuarioDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasIndex(notificacion => notificacion.Estado);
    }
}

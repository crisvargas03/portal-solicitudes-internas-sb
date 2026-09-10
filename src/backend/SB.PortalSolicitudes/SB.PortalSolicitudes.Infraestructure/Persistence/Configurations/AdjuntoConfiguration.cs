using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Configurations;

public class AdjuntoConfiguration : IEntityTypeConfiguration<Adjunto>
{
    public void Configure(EntityTypeBuilder<Adjunto> constructor)
    {
        constructor.ToTable("Adjuntos");

        constructor.HasKey(adjunto => adjunto.Id);

        constructor.Property(adjunto => adjunto.Descripcion)
            .IsRequired()
            .HasMaxLength(Adjunto.MAX_LONGITUD_DESCRIPCION);

        // Solo la direccion de la evidencia: no se almacenan archivos (ADR-0006).
        constructor.Property(adjunto => adjunto.Url)
            .IsRequired()
            .HasMaxLength(Adjunto.MAX_LONGITUD_URL);

        constructor.Property(adjunto => adjunto.Fecha)
            .IsRequired();

        constructor.HasOne(adjunto => adjunto.Solicitud)
            .WithMany(solicitud => solicitud.Adjuntos)
            .HasForeignKey(adjunto => adjunto.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        constructor.HasOne(adjunto => adjunto.Usuario)
            .WithMany()
            .HasForeignKey(adjunto => adjunto.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasIndex(adjunto => adjunto.SolicitudId);
    }
}

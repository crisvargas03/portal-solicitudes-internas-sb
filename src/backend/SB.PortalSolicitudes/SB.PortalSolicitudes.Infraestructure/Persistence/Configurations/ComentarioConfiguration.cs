using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Configurations;

public class ComentarioConfiguration : IEntityTypeConfiguration<Comentario>
{
    public void Configure(EntityTypeBuilder<Comentario> constructor)
    {
        constructor.ToTable("Comentarios");

        constructor.HasKey(comentario => comentario.Id);

        constructor.Property(comentario => comentario.Texto)
            .IsRequired()
            .HasMaxLength(Comentario.MAX_LONGITUD_TEXTO);

        constructor.Property(comentario => comentario.EsInterno)
            .IsRequired();

        constructor.Property(comentario => comentario.Fecha)
            .IsRequired();

        constructor.HasOne(comentario => comentario.Solicitud)
            .WithMany(solicitud => solicitud.Comentarios)
            .HasForeignKey(comentario => comentario.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        constructor.HasOne(comentario => comentario.Usuario)
            .WithMany(usuario => usuario.Comentarios)
            .HasForeignKey(comentario => comentario.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // El solicitante solo ve los comentarios publicos: se filtra por esta pareja.
        constructor.HasIndex(comentario => new { comentario.SolicitudId, comentario.EsInterno });
    }
}

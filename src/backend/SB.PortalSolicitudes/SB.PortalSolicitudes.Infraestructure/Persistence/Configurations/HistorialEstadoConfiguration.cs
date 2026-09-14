using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Configurations;

public class HistorialEstadoConfiguration : IEntityTypeConfiguration<HistorialEstado>
{
    public void Configure(EntityTypeBuilder<HistorialEstado> constructor)
    {
        constructor.ToTable("HistorialEstados");

        constructor.HasKey(historial => historial.Id);

        constructor.Property(historial => historial.Comentario)
            .HasMaxLength(HistorialEstado.MAX_LONGITUD_COMENTARIO);

        constructor.Property(historial => historial.Fecha)
            .IsRequired();

        constructor.HasOne(historial => historial.Solicitud)
            .WithMany(solicitud => solicitud.Historial)
            .HasForeignKey(historial => historial.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        // Sin definir en el registro inicial: la solicitud no tenia estado previo.
        constructor.HasOne(historial => historial.EstadoAnterior)
            .WithMany()
            .HasForeignKey(historial => historial.EstadoAnteriorId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne(historial => historial.EstadoNuevo)
            .WithMany()
            .HasForeignKey(historial => historial.EstadoNuevoId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne(historial => historial.Usuario)
            .WithMany()
            .HasForeignKey(historial => historial.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // El comentario de resolucion se localiza buscando el ultimo registro cuyo
        // estado nuevo es RESUELTA (ADR-0001): este indice sostiene esa consulta.
        constructor.HasIndex(historial => new { historial.SolicitudId, historial.EstadoNuevoId });
    }
}

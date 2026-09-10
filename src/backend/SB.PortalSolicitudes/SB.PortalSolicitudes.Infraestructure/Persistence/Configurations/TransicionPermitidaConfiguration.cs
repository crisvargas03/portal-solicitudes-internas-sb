using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Configurations;

public class TransicionPermitidaConfiguration : IEntityTypeConfiguration<TransicionPermitida>
{
    public void Configure(EntityTypeBuilder<TransicionPermitida> constructor)
    {
        constructor.ToTable("TransicionesPermitidas");

        constructor.HasKey(transicion => transicion.Id);

        constructor.Property(transicion => transicion.RequiereComentario)
            .IsRequired();

        constructor.Property(transicion => transicion.Activo)
            .IsRequired();

        constructor.HasOne(transicion => transicion.EstadoOrigen)
            .WithMany(estado => estado.TransicionesDeSalida)
            .HasForeignKey(transicion => transicion.EstadoOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne(transicion => transicion.EstadoDestino)
            .WithMany(estado => estado.TransicionesDeEntrada)
            .HasForeignKey(transicion => transicion.EstadoDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un unico camino declarado por par origen-destino.
        constructor.HasIndex(transicion => new { transicion.EstadoOrigenId, transicion.EstadoDestinoId })
            .IsUnique();
    }
}

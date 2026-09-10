using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.PortalSolicitudes.Domain.Common;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Configurations;

public class PrioridadConfiguration : IEntityTypeConfiguration<Prioridad>
{
    public void Configure(EntityTypeBuilder<Prioridad> constructor)
    {
        constructor.ToTable("Prioridades");

        constructor.HasKey(prioridad => prioridad.Id);

        constructor.Property(prioridad => prioridad.Nombre)
            .IsRequired()
            .HasMaxLength(CatalogoBase.MAX_LONGITUD_NOMBRE);

        constructor.Property(prioridad => prioridad.Nivel)
            .IsRequired();

        constructor.Property(prioridad => prioridad.Activo)
            .IsRequired();

        constructor.HasIndex(prioridad => prioridad.Nombre)
            .IsUnique();

        constructor.HasIndex(prioridad => prioridad.Nivel)
            .IsUnique();
    }
}

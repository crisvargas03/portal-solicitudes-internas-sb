using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.PortalSolicitudes.Domain.Common;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Configurations;

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> constructor)
    {
        constructor.ToTable("Areas");

        constructor.HasKey(area => area.Id);

        constructor.Property(area => area.Nombre)
            .IsRequired()
            .HasMaxLength(CatalogoBase.MAX_LONGITUD_NOMBRE);

        constructor.Property(area => area.Activo)
            .IsRequired();

        constructor.HasIndex(area => area.Nombre)
            .IsUnique();
    }
}

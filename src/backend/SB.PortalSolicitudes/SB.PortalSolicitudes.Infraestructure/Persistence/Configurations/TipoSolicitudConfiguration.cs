using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.PortalSolicitudes.Domain.Common;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Configurations;

public class TipoSolicitudConfiguration : IEntityTypeConfiguration<TipoSolicitud>
{
    public void Configure(EntityTypeBuilder<TipoSolicitud> constructor)
    {
        constructor.ToTable("TiposSolicitud");

        constructor.HasKey(tipoSolicitud => tipoSolicitud.Id);

        constructor.Property(tipoSolicitud => tipoSolicitud.Nombre)
            .IsRequired()
            .HasMaxLength(CatalogoBase.MAX_LONGITUD_NOMBRE);

        constructor.Property(tipoSolicitud => tipoSolicitud.Descripcion)
            .HasMaxLength(TipoSolicitud.MAX_LONGITUD_DESCRIPCION);

        constructor.Property(tipoSolicitud => tipoSolicitud.Activo)
            .IsRequired();

        constructor.HasIndex(tipoSolicitud => tipoSolicitud.Nombre)
            .IsUnique();
    }
}

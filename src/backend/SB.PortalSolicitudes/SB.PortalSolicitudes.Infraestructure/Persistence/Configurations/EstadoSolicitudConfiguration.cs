using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.PortalSolicitudes.Domain.Common;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Configurations;

public class EstadoSolicitudConfiguration : IEntityTypeConfiguration<EstadoSolicitud>
{
    public void Configure(EntityTypeBuilder<EstadoSolicitud> constructor)
    {
        constructor.ToTable("EstadosSolicitud");

        constructor.HasKey(estadoSolicitud => estadoSolicitud.Id);

        constructor.Property(estadoSolicitud => estadoSolicitud.Codigo)
            .IsRequired()
            .HasMaxLength(EstadoSolicitud.MAX_LONGITUD_CODIGO);

        constructor.Property(estadoSolicitud => estadoSolicitud.Nombre)
            .IsRequired()
            .HasMaxLength(CatalogoBase.MAX_LONGITUD_NOMBRE);

        constructor.Property(estadoSolicitud => estadoSolicitud.Orden)
            .IsRequired();

        constructor.Property(estadoSolicitud => estadoSolicitud.EsFinal)
            .IsRequired();

        constructor.Property(estadoSolicitud => estadoSolicitud.Activo)
            .IsRequired();

        // El codigo es la clave estable con la que se referencian los estados (ADR-0004).
        constructor.HasIndex(estadoSolicitud => estadoSolicitud.Codigo)
            .IsUnique();

        constructor.HasIndex(estadoSolicitud => estadoSolicitud.Orden)
            .IsUnique();
    }
}

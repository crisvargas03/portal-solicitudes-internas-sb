using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Configurations;

public class ContadorCodigoConfiguration : IEntityTypeConfiguration<ContadorCodigo>
{
    public void Configure(EntityTypeBuilder<ContadorCodigo> constructor)
    {
        constructor.ToTable("ContadoresCodigo");

        constructor.HasKey(contador => contador.Anio);

        constructor.Property(contador => contador.Anio)
            .ValueGeneratedNever();

        constructor.Property(contador => contador.Ultimo)
            .IsRequired();
    }
}

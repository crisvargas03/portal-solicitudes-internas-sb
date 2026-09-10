using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Configurations;

public class TransicionPermitidaRolConfiguration : IEntityTypeConfiguration<TransicionPermitidaRol>
{
    public void Configure(EntityTypeBuilder<TransicionPermitidaRol> constructor)
    {
        constructor.ToTable("TransicionesPermitidasRoles");

        constructor.HasKey(transicionRol => transicionRol.Id);

        // Se persiste como entero (ADR-0004).
        constructor.Property(transicionRol => transicionRol.Rol)
            .IsRequired()
            .HasConversion<int>();

        constructor.HasOne(transicionRol => transicionRol.TransicionPermitida)
            .WithMany(transicion => transicion.RolesPermitidos)
            .HasForeignKey(transicionRol => transicionRol.TransicionPermitidaId)
            .OnDelete(DeleteBehavior.Cascade);

        constructor.HasIndex(transicionRol => new { transicionRol.TransicionPermitidaId, transicionRol.Rol })
            .IsUnique();
    }
}

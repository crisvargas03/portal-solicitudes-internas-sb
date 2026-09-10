using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> constructor)
    {
        constructor.ToTable("Usuarios");

        constructor.HasKey(usuario => usuario.Id);

        constructor.Property(usuario => usuario.Nombre)
            .IsRequired()
            .HasMaxLength(Usuario.MAX_LONGITUD_NOMBRE);

        constructor.Property(usuario => usuario.Email)
            .IsRequired()
            .HasMaxLength(Usuario.MAX_LONGITUD_EMAIL);

        constructor.Property(usuario => usuario.PasswordHash)
            .IsRequired()
            .HasMaxLength(Usuario.MAX_LONGITUD_PASSWORD_HASH);

        constructor.Property(usuario => usuario.Rol)
            .IsRequired()
            .HasConversion<int>();

        constructor.Property(usuario => usuario.Activo)
            .IsRequired();

        // El email identifica al usuario en el inicio de sesion.
        constructor.HasIndex(usuario => usuario.Email)
            .IsUnique();
    }
}

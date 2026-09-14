namespace SB.PortalSolicitudes.Application.Abstractions.Autenticacion;

/// <summary>
/// Hasheo de contraseñas de <see cref="Domain.Entities.Usuario"/>. El algoritmo concreto
/// (BCrypt) vive en Infraestructure (ver ADR-0007: modelo propio en lugar de Identity).
/// </summary>
public interface IHasheadorPasswords
{
    string Hashear(string password);

    bool Verificar(string password, string hash);
}

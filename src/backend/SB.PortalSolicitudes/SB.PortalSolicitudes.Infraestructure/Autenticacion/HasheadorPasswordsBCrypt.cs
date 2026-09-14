using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;

namespace SB.PortalSolicitudes.Infraestructure.Autenticacion;

public class HasheadorPasswordsBCrypt : IHasheadorPasswords
{
    public string Hashear(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verificar(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}

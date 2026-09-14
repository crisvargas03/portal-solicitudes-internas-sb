using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Autenticacion;

public class ProveedorTokensJwt : IProveedorTokens
{
    private readonly OpcionesJwt _opciones;

    public ProveedorTokensJwt(IOptions<OpcionesJwt> opciones)
    {
        _opciones = opciones.Value;
    }

    public (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario)
    {
        DateTime expiraEn = DateTime.UtcNow.AddMinutes(_opciones.MinutosExpiracion);

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        SymmetricSecurityKey clave = new(Encoding.UTF8.GetBytes(_opciones.ClaveSecreta));
        SigningCredentials credenciales = new(clave, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: _opciones.Emisor,
            audience: _opciones.Audiencia,
            claims: claims,
            expires: expiraEn,
            signingCredentials: credenciales);

        string tokenSerializado = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenSerializado, expiraEn);
    }
}

using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Abstractions.Autenticacion;

/// <summary>
/// Emite el JWT de sesion. El algoritmo y la clave de firma viven en Infraestructure;
/// Application solo conoce esta interfaz (ver docs/architecture.md).
/// </summary>
public interface IProveedorTokens
{
    (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario);
}

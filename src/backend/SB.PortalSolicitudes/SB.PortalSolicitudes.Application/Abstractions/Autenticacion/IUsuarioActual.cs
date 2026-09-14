using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Abstractions.Autenticacion;

/// <summary>
/// Identidad del usuario autenticado en la peticion HTTP actual. La implementacion vive en
/// Api (unica capa que conoce <c>HttpContext</c>); los handlers dependen solo de esta
/// interfaz para autorizar sin acoplarse a ASP.NET Core.
/// </summary>
public interface IUsuarioActual
{
    int? Id { get; }

    RolUsuario? Rol { get; }

    bool EstaAutenticado { get; }
}

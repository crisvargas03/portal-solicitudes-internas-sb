using System.Security.Claims;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.API.Autenticacion;

/// <summary>Lee la identidad del <c>ClaimsPrincipal</c> de la peticion actual (unica pieza que conoce <c>HttpContext</c>).</summary>
public class UsuarioActual : IUsuarioActual
{
    private readonly ClaimsPrincipal? _usuario;

    public UsuarioActual(IHttpContextAccessor httpContextAccessor)
    {
        _usuario = httpContextAccessor.HttpContext?.User;
    }

    public bool EstaAutenticado => _usuario?.Identity?.IsAuthenticated ?? false;

    public int? Id
    {
        get
        {
            string? valor = _usuario?.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(valor, out int id) ? id : null;
        }
    }

    public RolUsuario? Rol
    {
        get
        {
            string? valor = _usuario?.FindFirstValue(ClaimTypes.Role);

            return Enum.TryParse(valor, out RolUsuario rol) ? rol : null;
        }
    }
}

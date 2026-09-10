using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Common;

/// <summary>
/// Calcula el <see cref="AlcanceSolicitudes"/> del usuario autenticado segun su rol
/// (ver ADR-0012). Punto unico para que el listado y el dashboard apliquen exactamente
/// el mismo recorte.
/// </summary>
public static class AlcanceSolicitudesFactory
{
    public static AlcanceSolicitudes Calcular(IUsuarioActual usuarioActual)
    {
        return usuarioActual.Rol switch
        {
            RolUsuario.Solicitante => new AlcanceSolicitudes(usuarioActual.Id, null, false),
            RolUsuario.Analista => new AlcanceSolicitudes(null, usuarioActual.Id, true),
            _ => AlcanceSolicitudes.SinRestriccion()
        };
    }
}

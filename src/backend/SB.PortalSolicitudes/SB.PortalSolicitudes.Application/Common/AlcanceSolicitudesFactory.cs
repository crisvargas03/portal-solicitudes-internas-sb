using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Common;

/// <summary>
/// Calcula el <see cref="AlcanceSolicitudes"/> del usuario autenticado segun su rol
/// (ver ADR-0012 y ADR-0029). Punto unico para que el listado y el dashboard apliquen
/// exactamente el mismo recorte.
/// </summary>
public static class AlcanceSolicitudesFactory
{
    /// <summary>
    /// Falla cerrado (ADR-0029): un rol desconocido o un <c>Id</c> ausente no caen en un
    /// caso por defecto sin restriccion, sino en <see cref="Resultado{T}"/> fallido.
    /// </summary>
    public static Resultado<AlcanceSolicitudes> Calcular(IUsuarioActual usuarioActual)
    {
        return usuarioActual.Rol switch
        {
            RolUsuario.Solicitante when usuarioActual.Id is int id => new AlcanceSolicitudes(id, null, false),
            RolUsuario.Analista when usuarioActual.Id is int id => new AlcanceSolicitudes(null, id, true),
            RolUsuario.Administrador => AlcanceSolicitudes.SinRestriccion(),
            _ => Resultado.Fallido<AlcanceSolicitudes>(
                Error.NoAutorizado("Auth.AlcanceIndeterminado", "No se pudo determinar el alcance del usuario autenticado.")),
        };
    }
}

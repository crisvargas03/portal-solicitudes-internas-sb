using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Common;

/// <summary>
/// Recorta el conjunto de <see cref="Solicitud"/> visible para el usuario autenticado,
/// segun su rol (ver ADR-0012). Se construye una sola vez a partir de
/// <c>IUsuarioActual</c> y la aplican el listado y el dashboard (como filtro de consulta)
/// y todo handler que opera sobre una solicitud por su <c>Id</c> (via <see cref="Incluye"/>),
/// para que todos coincidan siempre sobre que ve cada rol.
/// </summary>
public sealed record AlcanceSolicitudes(int? UsuarioSolicitanteId, int? UsuarioAsignadoId, bool IncluirSinAsignar)
{
    /// <summary>Sin recorte: el rol Administrador ve todas las solicitudes.</summary>
    public static AlcanceSolicitudes SinRestriccion() => new(null, null, false);

    /// <summary>
    /// Misma regla que <c>SolicitudRepository.AplicarAlcance</c>, evaluada sobre una solicitud
    /// ya cargada: si el listado no la mostraria, ningun handler debe dejar leerla ni operarla.
    /// </summary>
    public bool Incluye(Solicitud solicitud)
    {
        bool cumpleSolicitante = UsuarioSolicitanteId is null || solicitud.UsuarioSolicitanteId == UsuarioSolicitanteId;

        bool cumpleAsignacion = UsuarioAsignadoId is null
            || solicitud.UsuarioAsignadoId == UsuarioAsignadoId
            || (IncluirSinAsignar && solicitud.UsuarioAsignadoId is null);

        return cumpleSolicitante && cumpleAsignacion;
    }
}

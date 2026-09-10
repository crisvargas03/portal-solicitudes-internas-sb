namespace SB.PortalSolicitudes.Application.Common;

/// <summary>
/// Recorta el conjunto de <see cref="Domain.Entities.Solicitud"/> visible para el usuario
/// autenticado, segun su rol (ver ADR-0012). Se construye una sola vez a partir de
/// <c>IUsuarioActual</c> y la aplican tanto el listado como el dashboard, para que ambos
/// coincidan siempre sobre que ve cada rol.
/// </summary>
public sealed record AlcanceSolicitudes(int? UsuarioSolicitanteId, int? UsuarioAsignadoId, bool IncluirSinAsignar)
{
    /// <summary>Sin recorte: el rol Administrador ve todas las solicitudes.</summary>
    public static AlcanceSolicitudes SinRestriccion() => new(null, null, false);
}

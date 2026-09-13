namespace SB.PortalSolicitudes.Application.Common;

/// <summary>
/// Corte adicional sobre el alcance ya recortado por rol (ver ADR-0012, ADR-0026): nunca
/// sustituye el alcance, solo lo estrecha. Sin efecto para Administrador salvo que tambien
/// quiera mirar por responsable.
/// </summary>
public enum FiltroAsignacion
{
    /// <summary>Sin corte adicional: todo lo que el alcance del rol ya deja ver.</summary>
    Todas,

    /// <summary>Solo lo asignado al usuario autenticado.</summary>
    Asignadas,

    /// <summary>Solo lo que no tiene responsable todavia.</summary>
    Disponibles,
}

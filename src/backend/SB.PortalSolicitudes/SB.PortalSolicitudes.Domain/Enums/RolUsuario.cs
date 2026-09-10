namespace SB.PortalSolicitudes.Domain.Enums;

/// <summary>
/// Roles minimos exigidos por el requerimiento. Se persiste como entero.
/// </summary>
public enum RolUsuario
{
    /// <summary>Acceso total a catalogos, usuarios y solicitudes.</summary>
    Administrador = 1,

    /// <summary>Acceso a las solicitudes asignadas o disponibles para gestion.</summary>
    Analista = 2,

    /// <summary>Acceso a crear y dar seguimiento a sus propias solicitudes.</summary>
    Solicitante = 3
}

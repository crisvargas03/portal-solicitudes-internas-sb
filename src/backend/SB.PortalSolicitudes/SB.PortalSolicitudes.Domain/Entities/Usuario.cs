using SB.PortalSolicitudes.Domain.Common;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Empleado del portal. Modelo propio simplificado en lugar de ASP.NET Core Identity,
/// para no arrastrar dependencias de infraestructura al dominio: ver ADR-0007.
/// La entidad solo guarda el hash; el algoritmo de hasheo vive en Infraestructure.
/// </summary>
public class Usuario : EntidadBase
{
    public const int MAX_LONGITUD_NOMBRE = 150;
    public const int MAX_LONGITUD_EMAIL = 150;
    public const int MAX_LONGITUD_PASSWORD_HASH = 500;

    public required string Nombre { get; set; }

    public required string Email { get; set; }

    public RolUsuario Rol { get; set; }

    public required string PasswordHash { get; set; }

    public bool Activo { get; set; } = true;

    /// <summary>Solicitudes que este usuario registro como solicitante.</summary>
    public ICollection<Solicitud> SolicitudesSolicitadas { get; set; } = new List<Solicitud>();

    /// <summary>Solicitudes de las que este usuario es responsable tecnico.</summary>
    public ICollection<Solicitud> SolicitudesAsignadas { get; set; } = new List<Solicitud>();

    public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
}

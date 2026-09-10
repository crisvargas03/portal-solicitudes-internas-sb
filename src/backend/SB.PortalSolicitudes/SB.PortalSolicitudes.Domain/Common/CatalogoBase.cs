namespace SB.PortalSolicitudes.Domain.Common;

/// <summary>
/// Base de los catalogos administrables (Area, TipoSolicitud, Prioridad, EstadoSolicitud).
/// Los catalogos no se eliminan: se desactivan, para no romper las solicitudes historicas.
/// </summary>
public abstract class CatalogoBase : EntidadBase
{
    public const int MAX_LONGITUD_NOMBRE = 100;

    public required string Nombre { get; set; }

    public bool Activo { get; set; } = true;
}

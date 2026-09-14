namespace SB.PortalSolicitudes.Domain.Common;

/// <summary>
/// Raiz de todas las entidades persistidas. La identidad la asigna la base de datos.
/// </summary>
public abstract class EntidadBase
{
    public int Id { get; set; }
}

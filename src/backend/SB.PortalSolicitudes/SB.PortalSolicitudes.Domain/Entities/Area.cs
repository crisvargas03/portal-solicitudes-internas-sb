using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Area institucional que origina las solicitudes. Catalogo administrable.
/// </summary>
public class Area : CatalogoBase
{
    public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
}

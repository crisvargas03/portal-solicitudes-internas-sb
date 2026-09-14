using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Prioridad de atencion. Es un catalogo en tabla y no una enumeracion, para poder
/// agregar niveles sin recompilar: ver ADR-0004.
/// </summary>
public class Prioridad : CatalogoBase
{
    /// <summary>
    /// Severidad relativa: a mayor numero, mayor urgencia. Ordena listados y el dashboard.
    /// </summary>
    public int Nivel { get; set; }

    public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
}

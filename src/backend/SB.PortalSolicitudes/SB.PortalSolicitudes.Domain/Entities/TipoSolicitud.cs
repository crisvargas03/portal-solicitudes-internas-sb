using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Clasificacion de la solicitud (soporte, tecnologia, etc.). Es un catalogo en tabla
/// y no una enumeracion, para poder agregar tipos nuevos sin recompilar: ver ADR-0004.
/// </summary>
public class TipoSolicitud : CatalogoBase
{
    public const int MAX_LONGITUD_DESCRIPCION = 500;

    public string? Descripcion { get; set; }

    public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
}

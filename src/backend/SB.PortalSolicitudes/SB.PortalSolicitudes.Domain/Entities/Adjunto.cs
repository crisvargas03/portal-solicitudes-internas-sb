using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Referencia o evidencia asociada a una solicitud. Solo se guarda el texto y la direccion:
/// el requerimiento excluye explicitamente el almacenamiento de archivos fisicos.
/// Se modela como entidad propia para admitir varias referencias por solicitud, con autor
/// y fecha (ver ADR-0006).
/// </summary>
public class Adjunto : EntidadBase
{
    public const int MAX_LONGITUD_DESCRIPCION = 200;
    public const int MAX_LONGITUD_URL = 500;

    public int SolicitudId { get; set; }

    public Solicitud? Solicitud { get; set; }

    public int UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }

    public required string Descripcion { get; set; }

    /// <summary>Direccion de la evidencia; no se descarga ni se almacena su contenido.</summary>
    public required string Url { get; set; }

    public DateTime Fecha { get; set; }
}

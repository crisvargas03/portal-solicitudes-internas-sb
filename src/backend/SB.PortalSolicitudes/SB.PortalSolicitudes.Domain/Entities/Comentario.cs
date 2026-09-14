using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Comentario de seguimiento sobre una solicitud. La visibilidad se modela con la bandera
/// <see cref="EsInterno"/>: los internos solo los ve el personal que gestiona la solicitud,
/// los publicos tambien los ve el solicitante.
/// </summary>
public class Comentario : EntidadBase
{
    public const int MAX_LONGITUD_TEXTO = 1000;

    public int SolicitudId { get; set; }

    public Solicitud? Solicitud { get; set; }

    public int UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }

    public required string Texto { get; set; }

    /// <summary><c>true</c> oculta el comentario al solicitante.</summary>
    public bool EsInterno { get; set; }

    public DateTime Fecha { get; set; }
}

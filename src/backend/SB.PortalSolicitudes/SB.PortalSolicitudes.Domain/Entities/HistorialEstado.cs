using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Traza de un cambio de estado: fecha, usuario y comentario, como exige el requerimiento.
/// El comentario de la transicion hacia <c>RESUELTA</c> es el comentario de resolucion de
/// la solicitud: no se duplica en <see cref="Solicitud"/> (ver ADR-0001).
/// </summary>
public class HistorialEstado : EntidadBase
{
    public const int MAX_LONGITUD_COMENTARIO = 1000;

    public int SolicitudId { get; set; }

    public Solicitud? Solicitud { get; set; }

    /// <summary>Sin definir en el registro inicial, donde la solicitud aun no tenia estado previo.</summary>
    public int? EstadoAnteriorId { get; set; }

    public EstadoSolicitud? EstadoAnterior { get; set; }

    public int EstadoNuevoId { get; set; }

    public EstadoSolicitud? EstadoNuevo { get; set; }

    public int UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }

    public string? Comentario { get; set; }

    public DateTime Fecha { get; set; }
}

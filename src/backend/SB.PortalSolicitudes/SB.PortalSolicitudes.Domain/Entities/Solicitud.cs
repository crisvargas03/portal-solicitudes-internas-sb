using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Entidad central del dominio: la solicitud interna registrada por un empleado.
/// No expone un campo de comentario de resolucion: ese comentario es el de la transicion
/// hacia <c>RESUELTA</c>, guardado en <see cref="HistorialEstado"/> (ver ADR-0001).
/// El cambio de estado y su registro en el historial los realiza la capa de aplicacion,
/// validando antes contra <see cref="TransicionPermitida"/> (ver ADR-0005).
/// </summary>
public class Solicitud : EntidadBase
{
    public const int MAX_LONGITUD_CODIGO = 20;
    public const int MAX_LONGITUD_TITULO = 150;
    public const int MAX_LONGITUD_DESCRIPCION = 2000;

    /// <summary>Codigo legible y unico, con el formato <c>SOL-2026-0001</c>.</summary>
    public required string Codigo { get; set; }

    public required string Titulo { get; set; }

    public required string Descripcion { get; set; }

    public DateTime FechaCreacion { get; set; }

    /// <summary>Fecha limite de atencion. Se captura manualmente y puede quedar sin definir.</summary>
    public DateTime? FechaCompromiso { get; set; }

    public int PrioridadId { get; set; }

    public Prioridad? Prioridad { get; set; }

    public int EstadoId { get; set; }

    public EstadoSolicitud? Estado { get; set; }

    public int AreaId { get; set; }

    public Area? Area { get; set; }

    public int TipoSolicitudId { get; set; }

    public TipoSolicitud? TipoSolicitud { get; set; }

    public int UsuarioSolicitanteId { get; set; }

    public Usuario? UsuarioSolicitante { get; set; }

    /// <summary>Responsable tecnico. Sin definir mientras la solicitud no se asigne.</summary>
    public int? UsuarioAsignadoId { get; set; }

    public Usuario? UsuarioAsignado { get; set; }

    public ICollection<HistorialEstado> Historial { get; set; } = new List<HistorialEstado>();

    public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public ICollection<Adjunto> Adjuntos { get; set; } = new List<Adjunto>();

    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
}

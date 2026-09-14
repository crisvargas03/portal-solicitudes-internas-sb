using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Cambio de estado autorizado por la maquina de estados. Convierte en datos las reglas
/// de ADR-0001 y ADR-0002 (ver ADR-0005):
/// la unica fila con destino <c>CERRADA</c> parte de <c>RESUELTA</c>; las filas que
/// desembocan en <c>RESUELTA</c> exigen comentario (el comentario de resolucion); y la
/// reapertura desde <c>CERRADA</c> solo lista los roles Administrador y Analista.
/// </summary>
public class TransicionPermitida : EntidadBase
{
    public int EstadoOrigenId { get; set; }

    public EstadoSolicitud? EstadoOrigen { get; set; }

    public int EstadoDestinoId { get; set; }

    public EstadoSolicitud? EstadoDestino { get; set; }

    /// <summary>
    /// Obliga a capturar un comentario al ejecutar la transicion. En el paso a
    /// <c>RESUELTA</c> ese comentario es el comentario de resolucion (ADR-0001).
    /// </summary>
    public bool RequiereComentario { get; set; }

    public bool Activo { get; set; } = true;

    /// <summary>Roles autorizados a ejecutar esta transicion.</summary>
    public ICollection<TransicionPermitidaRol> RolesPermitidos { get; set; } = new List<TransicionPermitidaRol>();
}

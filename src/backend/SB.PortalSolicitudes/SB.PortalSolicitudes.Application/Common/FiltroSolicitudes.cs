namespace SB.PortalSolicitudes.Application.Common;

/// <summary>
/// Criterios opcionales para el listado paginado de <see cref="Domain.Entities.Solicitud"/>
/// (ver <c>ISolicitudRepository.ObtenerPaginadoAsync</c>). Todos los campos son opcionales:
/// un filtro sin definir no se aplica.
/// </summary>
public class FiltroSolicitudes : ParametrosPaginacion
{
    public int? EstadoId { get; set; }

    public int? PrioridadId { get; set; }

    public int? AreaId { get; set; }

    public int? TipoSolicitudId { get; set; }

    public int? UsuarioSolicitanteId { get; set; }

    public int? UsuarioAsignadoId { get; set; }

    /// <summary>
    /// Cuando es <c>true</c>, el filtro por <see cref="UsuarioAsignadoId"/> tambien admite
    /// solicitudes sin asignar (usado por el alcance del rol Analista, ver ADR-0012:
    /// asignadas a si mismo o sin asignar). Sin efecto si <see cref="UsuarioAsignadoId"/> es nulo.
    /// </summary>
    public bool IncluirSinAsignar { get; set; }

    public DateTime? FechaCreacionDesde { get; set; }

    public DateTime? FechaCreacionHasta { get; set; }

    /// <summary>Coincidencia parcial contra <c>Codigo</c> o <c>Titulo</c>.</summary>
    public string? TextoBusqueda { get; set; }

    /// <summary>Cuando es <c>true</c>, limita el listado a solicitudes con <c>FechaCompromiso</c> vencida.</summary>
    public bool SoloVencidas { get; set; }

    /// <summary>Fecha de referencia para evaluar <see cref="SoloVencidas"/>. La decide quien llama, no el repositorio.</summary>
    public DateTime? FechaReferencia { get; set; }
}

using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

public interface ISolicitudRepository : IRepositorioBase<Solicitud>
{
    /// <summary>
    /// Vista de detalle: Estado, Prioridad, Area, TipoSolicitud, UsuarioSolicitante,
    /// UsuarioAsignado, Historial (con EstadoAnterior/EstadoNuevo/Usuario), Comentarios
    /// (con Usuario) y Adjuntos (con Usuario).
    /// </summary>
    Task<Solicitud?> ObtenerDetalleAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Carga minima para un cambio de estado: solo <c>Estado</c>. Sin historial ni comentarios.</summary>
    Task<Solicitud?> ObtenerParaCambioDeEstadoAsync(int id, CancellationToken cancellationToken = default);

    Task<Solicitud?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<Solicitud>> ObtenerPaginadoAsync(
        FiltroSolicitudes filtro, CancellationToken cancellationToken = default);

    /// <summary>
    /// Conteo agrupado por <c>EstadoSolicitud.Codigo</c>, para el dashboard.
    /// <paramref name="criterio"/> aplica el recorte por rol mas el corte aditivo por
    /// asignacion (ADR-0012, ADR-0027), para que el dashboard nunca muestre mas de lo que el
    /// listado dejaria ver al mismo usuario con el mismo filtro.
    /// </summary>
    Task<IReadOnlyDictionary<string, int>> ContarPorCodigoDeEstadoAsync(
        CriterioDashboard criterio, CancellationToken cancellationToken = default);

    /// <summary>Conteo agrupado por <c>PrioridadId</c>, para el dashboard.</summary>
    Task<IReadOnlyDictionary<int, int>> ContarPorPrioridadAsync(
        CriterioDashboard criterio, CancellationToken cancellationToken = default);

    /// <summary>
    /// <paramref name="fechaReferencia"/> la decide quien llama: el repositorio no lee la
    /// hora del sistema. Excluye las solicitudes en un estado final (<c>EsFinal</c>): una
    /// solicitud cerrada con fecha de compromiso pasada no esta vencida.
    /// </summary>
    Task<int> ContarVencidasAsync(
        DateTime fechaReferencia, CriterioDashboard criterio, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Solicitud>> ObtenerRecientesAsync(
        int cantidad, CriterioDashboard criterio, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cuenta lo sin responsable dentro del alcance del usuario (indicador del pool
    /// disponible, ADR-0027) — a diferencia de los demas conteos del dashboard, no toma
    /// <see cref="CriterioDashboard"/> porque no tiene sentido combinarlo con un corte de
    /// asignacion adicional.
    /// </summary>
    Task<int> ContarSinAsignarAsync(AlcanceSolicitudes alcance, CancellationToken cancellationToken = default);

    /// <summary>Soporte para la generacion del codigo legible (ver docs/open-decisions.md #2). Solo informa, no decide.</summary>
    Task<int> ContarPorAnioDeCreacionAsync(int anio, CancellationToken cancellationToken = default);
}

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

    /// <summary>Conteo agrupado por <c>EstadoSolicitud.Codigo</c>, para el dashboard.</summary>
    Task<IReadOnlyDictionary<string, int>> ContarPorCodigoDeEstadoAsync(CancellationToken cancellationToken = default);

    /// <summary>Conteo agrupado por <c>PrioridadId</c>, para el dashboard.</summary>
    Task<IReadOnlyDictionary<int, int>> ContarPorPrioridadAsync(CancellationToken cancellationToken = default);

    /// <summary><paramref name="fechaReferencia"/> la decide quien llama: el repositorio no lee la hora del sistema.</summary>
    Task<int> ContarVencidasAsync(DateTime fechaReferencia, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Solicitud>> ObtenerRecientesAsync(int cantidad, CancellationToken cancellationToken = default);

    /// <summary>Soporte para la generacion del codigo legible (ver docs/open-decisions.md #2). Solo informa, no decide.</summary>
    Task<int> ContarPorAnioDeCreacionAsync(int anio, CancellationToken cancellationToken = default);
}

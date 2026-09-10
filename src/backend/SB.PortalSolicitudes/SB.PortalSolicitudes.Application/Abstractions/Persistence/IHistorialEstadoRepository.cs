using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

public interface IHistorialEstadoRepository : IRepositorioBase<HistorialEstado>
{
    Task<IReadOnlyList<HistorialEstado>> ObtenerPorSolicitudAsync(
        int solicitudId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ultima transicion hacia el estado con el <paramref name="codigoEstado"/> indicado.
    /// Es la forma de recuperar el comentario de resolucion (ver ADR-0001): se busca con
    /// <c>codigoEstado = CodigosEstadoSolicitud.RESUELTA</c>.
    /// </summary>
    Task<HistorialEstado?> ObtenerUltimoPorCodigoDeEstadoNuevoAsync(
        int solicitudId, string codigoEstado, CancellationToken cancellationToken = default);
}

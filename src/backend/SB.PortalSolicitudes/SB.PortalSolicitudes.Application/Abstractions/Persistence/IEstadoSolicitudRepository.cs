using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

/// <summary>
/// Catalogo de <see cref="EstadoSolicitud"/>. <c>Codigo</c> es la clave estable
/// (ver <see cref="CodigosEstadoSolicitud"/> y ADR-0004): nunca comparar por <c>Id</c>.
/// </summary>
public interface IEstadoSolicitudRepository : ICatalogoRepository<EstadoSolicitud>
{
    Task<EstadoSolicitud?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EstadoSolicitud>> ObtenerActivosOrdenadosAsync(CancellationToken cancellationToken = default);
}

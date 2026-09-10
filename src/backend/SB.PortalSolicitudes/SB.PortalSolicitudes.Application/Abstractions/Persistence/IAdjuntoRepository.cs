using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

public interface IAdjuntoRepository : IRepositorioBase<Adjunto>
{
    Task<IReadOnlyList<Adjunto>> ObtenerPorSolicitudAsync(int solicitudId, CancellationToken cancellationToken = default);
}

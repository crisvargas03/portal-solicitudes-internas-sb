using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

public class AdjuntoRepository : RepositorioBase<Adjunto>, IAdjuntoRepository
{
    public AdjuntoRepository(PortalSolicitudesDbContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<Adjunto>> ObtenerPorSolicitudAsync(
        int solicitudId, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Include(adjunto => adjunto.Usuario)
            .Where(adjunto => adjunto.SolicitudId == solicitudId)
            .OrderBy(adjunto => adjunto.Fecha)
            .ToListAsync(cancellationToken);
    }
}

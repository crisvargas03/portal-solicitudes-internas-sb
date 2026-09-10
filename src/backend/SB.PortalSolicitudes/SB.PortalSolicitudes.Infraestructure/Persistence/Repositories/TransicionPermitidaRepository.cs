using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

public class TransicionPermitidaRepository : RepositorioBase<TransicionPermitida>, ITransicionPermitidaRepository
{
    public TransicionPermitidaRepository(PortalSolicitudesDbContext contexto) : base(contexto)
    {
    }

    public async Task<TransicionPermitida?> ObtenerAsync(
        int estadoOrigenId, int estadoDestinoId, CancellationToken cancellationToken = default)
    {
        return await ConsultaBase()
            .FirstOrDefaultAsync(
                transicion => transicion.EstadoOrigenId == estadoOrigenId
                    && transicion.EstadoDestinoId == estadoDestinoId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<TransicionPermitida>> ObtenerDesdeEstadoAsync(
        int estadoOrigenId, CancellationToken cancellationToken = default)
    {
        return await ConsultaBase()
            .Where(transicion => transicion.EstadoOrigenId == estadoOrigenId)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<TransicionPermitida> ConsultaBase()
    {
        return Conjunto.AsNoTracking()
            .Include(transicion => transicion.EstadoOrigen)
            .Include(transicion => transicion.EstadoDestino)
            .Include(transicion => transicion.RolesPermitidos)
            .Where(transicion => transicion.Activo);
    }
}

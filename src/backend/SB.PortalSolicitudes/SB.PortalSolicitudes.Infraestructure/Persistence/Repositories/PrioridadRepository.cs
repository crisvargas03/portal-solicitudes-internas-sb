using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

public class PrioridadRepository : CatalogoRepository<Prioridad>, IPrioridadRepository
{
    public PrioridadRepository(PortalSolicitudesDbContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<Prioridad>> ObtenerActivasOrdenadasPorNivelAsync(
        CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Where(prioridad => prioridad.Activo)
            .OrderBy(prioridad => prioridad.Nivel)
            .ToListAsync(cancellationToken);
    }
}

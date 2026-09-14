using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

public class EstadoSolicitudRepository : CatalogoRepository<EstadoSolicitud>, IEstadoSolicitudRepository
{
    public EstadoSolicitudRepository(PortalSolicitudesDbContext contexto) : base(contexto)
    {
    }

    public async Task<EstadoSolicitud?> ObtenerPorCodigoAsync(
        string codigo, CancellationToken cancellationToken = default)
    {
        return await Conjunto.FirstOrDefaultAsync(estado => estado.Codigo == codigo, cancellationToken);
    }

    public async Task<IReadOnlyList<EstadoSolicitud>> ObtenerActivosOrdenadosAsync(
        CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Where(estado => estado.Activo)
            .OrderBy(estado => estado.Orden)
            .ToListAsync(cancellationToken);
    }
}

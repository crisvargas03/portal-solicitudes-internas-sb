using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

public class AreaRepository : CatalogoRepository<Area>, IAreaRepository
{
    public AreaRepository(PortalSolicitudesDbContext contexto) : base(contexto)
    {
    }
}

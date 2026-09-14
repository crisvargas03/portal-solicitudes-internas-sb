using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

public class TipoSolicitudRepository : CatalogoRepository<TipoSolicitud>, ITipoSolicitudRepository
{
    public TipoSolicitudRepository(PortalSolicitudesDbContext contexto) : base(contexto)
    {
    }
}

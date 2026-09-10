using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

public class ComentarioRepository : RepositorioBase<Comentario>, IComentarioRepository
{
    public ComentarioRepository(PortalSolicitudesDbContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<Comentario>> ObtenerPorSolicitudAsync(
        int solicitudId, bool incluirInternos, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Include(comentario => comentario.Usuario)
            .Where(comentario => comentario.SolicitudId == solicitudId)
            .Where(comentario => incluirInternos || !comentario.EsInterno)
            .OrderBy(comentario => comentario.Fecha)
            .ToListAsync(cancellationToken);
    }
}

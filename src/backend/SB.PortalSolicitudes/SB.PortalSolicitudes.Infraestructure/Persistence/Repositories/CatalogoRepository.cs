using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

/// <summary>Base comun para los repositorios de catalogo (Area, TipoSolicitud, Prioridad, EstadoSolicitud).</summary>
public abstract class CatalogoRepository<TCatalogo> : RepositorioBase<TCatalogo>, ICatalogoRepository<TCatalogo>
    where TCatalogo : CatalogoBase
{
    protected CatalogoRepository(PortalSolicitudesDbContext contexto) : base(contexto)
    {
    }

    public virtual async Task<IReadOnlyList<TCatalogo>> ObtenerActivosAsync(CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Where(catalogo => catalogo.Activo)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> ExisteNombreAsync(
        string nombre, int? idExcluido = null, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Where(catalogo => catalogo.Nombre == nombre)
            .Where(catalogo => idExcluido == null || catalogo.Id != idExcluido)
            .AnyAsync(cancellationToken);
    }
}

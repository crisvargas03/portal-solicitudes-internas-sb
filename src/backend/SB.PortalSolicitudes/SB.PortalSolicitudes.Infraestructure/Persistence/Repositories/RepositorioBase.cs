using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

/// <summary>
/// Implementacion comun de <see cref="IRepositorioBase{TEntidad}"/> sobre EF Core.
/// No contiene reglas de negocio ni llama a <c>SaveChanges</c>: eso vive en
/// <see cref="UnitOfWork"/>.
/// </summary>
public abstract class RepositorioBase<TEntidad> : IRepositorioBase<TEntidad>
    where TEntidad : EntidadBase
{
    protected readonly PortalSolicitudesDbContext Contexto;

    protected DbSet<TEntidad> Conjunto => Contexto.Set<TEntidad>();

    protected RepositorioBase(PortalSolicitudesDbContext contexto)
    {
        Contexto = contexto;
    }

    public virtual async Task<TEntidad?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Conjunto.FirstOrDefaultAsync(entidad => entidad.Id == id, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntidad>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking().AnyAsync(entidad => entidad.Id == id, cancellationToken);
    }

    public virtual async Task AgregarAsync(TEntidad entidad, CancellationToken cancellationToken = default)
    {
        await Conjunto.AddAsync(entidad, cancellationToken);
    }

    public virtual void Actualizar(TEntidad entidad)
    {
        Conjunto.Update(entidad);
    }

    public virtual void Eliminar(TEntidad entidad)
    {
        Conjunto.Remove(entidad);
    }
}

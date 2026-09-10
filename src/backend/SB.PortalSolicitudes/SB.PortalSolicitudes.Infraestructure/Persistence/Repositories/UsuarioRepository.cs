using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

public class UsuarioRepository : RepositorioBase<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(PortalSolicitudesDbContext contexto) : base(contexto)
    {
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Conjunto.FirstOrDefaultAsync(usuario => usuario.Email == email, cancellationToken);
    }

    public async Task<bool> ExisteEmailAsync(
        string email, int? idExcluido = null, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Where(usuario => usuario.Email == email)
            .Where(usuario => idExcluido == null || usuario.Id != idExcluido)
            .AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Usuario>> ObtenerPorRolAsync(
        RolUsuario rol, bool soloActivos = true, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Where(usuario => usuario.Rol == rol)
            .Where(usuario => !soloActivos || usuario.Activo)
            .ToListAsync(cancellationToken);
    }

    public async Task<ResultadoPaginado<Usuario>> ObtenerPaginadoAsync(
        ParametrosPaginacion paginacion, bool soloActivos = true, CancellationToken cancellationToken = default)
    {
        IQueryable<Usuario> consulta = Conjunto.AsNoTracking()
            .Where(usuario => !soloActivos || usuario.Activo);

        int totalElementos = await consulta.CountAsync(cancellationToken);

        List<Usuario> elementos = await consulta
            .OrderBy(usuario => usuario.Nombre)
            .Skip((paginacion.Pagina - 1) * paginacion.TamanoPagina)
            .Take(paginacion.TamanoPagina)
            .ToListAsync(cancellationToken);

        return new ResultadoPaginado<Usuario>(elementos, totalElementos, paginacion.Pagina, paginacion.TamanoPagina);
    }
}

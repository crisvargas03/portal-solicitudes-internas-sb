using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

public interface IUsuarioRepository : IRepositorioBase<Usuario>
{
    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary><paramref name="idExcluido"/> permite validar unicidad al editar sin chocar con el propio registro.</summary>
    Task<bool> ExisteEmailAsync(string email, int? idExcluido = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Usuario>> ObtenerPorRolAsync(
        RolUsuario rol, bool soloActivos = true, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<Usuario>> ObtenerPaginadoAsync(
        ParametrosPaginacion paginacion, bool soloActivos = true, CancellationToken cancellationToken = default);
}

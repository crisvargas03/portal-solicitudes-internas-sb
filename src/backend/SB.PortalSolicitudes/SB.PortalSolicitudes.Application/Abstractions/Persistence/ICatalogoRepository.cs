using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

/// <summary>
/// Operaciones comunes a los catalogos administrables (<see cref="Area"/>,
/// <see cref="Domain.Entities.TipoSolicitud"/>, <see cref="Domain.Entities.Prioridad"/>,
/// <see cref="Domain.Entities.EstadoSolicitud"/>). Los catalogos no se eliminan, se
/// desactivan (ver <c>CatalogoBase.Activo</c>), por eso no hay un metodo de borrado propio.
/// </summary>
public interface ICatalogoRepository<TCatalogo> : IRepositorioBase<TCatalogo>
    where TCatalogo : CatalogoBase
{
    Task<IReadOnlyList<TCatalogo>> ObtenerActivosAsync(CancellationToken cancellationToken = default);

    Task<bool> ExisteNombreAsync(string nombre, int? idExcluido = null, CancellationToken cancellationToken = default);
}

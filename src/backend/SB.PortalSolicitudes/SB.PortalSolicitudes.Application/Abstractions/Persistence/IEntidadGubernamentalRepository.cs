using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

/// <summary>
/// Persistencia de <see cref="EntidadGubernamental"/>, respaldada por un archivo de texto
/// en vez de la base de datos relacional (ver ADR-0034). No participa de <see cref="IUnitOfWork"/>:
/// cada metodo persiste su propio cambio de inmediato, sin un paso de <c>GuardarCambios</c> aparte.
/// </summary>
public interface IEntidadGubernamentalRepository
{
    Task<IReadOnlyList<EntidadGubernamental>> ObtenerActivasAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EntidadGubernamental>> ObtenerTodasAsync(CancellationToken cancellationToken = default);

    Task<EntidadGubernamental?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExisteNombreAsync(string nombre, int? idExcluido = null, CancellationToken cancellationToken = default);

    Task<EntidadGubernamental> CrearAsync(EntidadGubernamental entidad, CancellationToken cancellationToken = default);

    Task ActualizarAsync(EntidadGubernamental entidad, CancellationToken cancellationToken = default);
}

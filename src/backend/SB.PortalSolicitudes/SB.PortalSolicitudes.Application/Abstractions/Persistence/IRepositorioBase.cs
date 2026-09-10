using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

/// <summary>
/// Operaciones de persistencia comunes a toda entidad. No contiene reglas de negocio:
/// solo lee y marca el estado de la entidad ante el <c>ChangeTracker</c>. Guardar los
/// cambios es responsabilidad de <see cref="IUnitOfWork.GuardarCambiosAsync"/>.
/// </summary>
public interface IRepositorioBase<TEntidad> where TEntidad : EntidadBase
{
    Task<TEntidad?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntidad>> ObtenerTodosAsync(CancellationToken cancellationToken = default);

    Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default);

    Task AgregarAsync(TEntidad entidad, CancellationToken cancellationToken = default);

    /// <summary>Marca la entidad como modificada ante el <c>ChangeTracker</c>. No accede a la base de datos.</summary>
    void Actualizar(TEntidad entidad);

    /// <summary>Marca la entidad como eliminada ante el <c>ChangeTracker</c>. No accede a la base de datos.</summary>
    void Eliminar(TEntidad entidad);
}

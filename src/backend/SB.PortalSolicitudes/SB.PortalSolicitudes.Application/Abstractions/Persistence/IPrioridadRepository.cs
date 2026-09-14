using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

/// <summary>Catalogo de <see cref="Prioridad"/>, ordenable por <c>Nivel</c> de urgencia.</summary>
public interface IPrioridadRepository : ICatalogoRepository<Prioridad>
{
    Task<IReadOnlyList<Prioridad>> ObtenerActivasOrdenadasPorNivelAsync(CancellationToken cancellationToken = default);
}

using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

/// <summary>Punto de inyeccion nombrado para el catalogo de <see cref="Area"/>.</summary>
public interface IAreaRepository : ICatalogoRepository<Area>
{
}

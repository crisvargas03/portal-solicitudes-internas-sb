using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

public class HistorialEstadoRepository : RepositorioBase<HistorialEstado>, IHistorialEstadoRepository
{
    public HistorialEstadoRepository(PortalSolicitudesDbContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<HistorialEstado>> ObtenerPorSolicitudAsync(
        int solicitudId, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Include(historial => historial.EstadoAnterior)
            .Include(historial => historial.EstadoNuevo)
            .Include(historial => historial.Usuario)
            .Where(historial => historial.SolicitudId == solicitudId)
            .OrderBy(historial => historial.Fecha)
            .ToListAsync(cancellationToken);
    }

    public async Task<HistorialEstado?> ObtenerUltimoPorCodigoDeEstadoNuevoAsync(
        int solicitudId, string codigoEstado, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Include(historial => historial.EstadoNuevo)
            .Where(historial => historial.SolicitudId == solicitudId)
            .Where(historial => historial.EstadoNuevo!.Codigo == codigoEstado)
            .OrderByDescending(historial => historial.Fecha)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

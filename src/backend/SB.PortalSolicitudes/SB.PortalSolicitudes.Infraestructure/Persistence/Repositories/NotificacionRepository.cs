using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

public class NotificacionRepository : RepositorioBase<Notificacion>, INotificacionRepository
{
    public NotificacionRepository(PortalSolicitudesDbContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<Notificacion>> ObtenerPendientesAsync(
        int cantidadMaxima, CancellationToken cancellationToken = default)
    {
        return await Conjunto
            .Where(notificacion => notificacion.Estado == EstadoNotificacion.Pendiente)
            .OrderBy(notificacion => notificacion.Fecha)
            .Take(cantidadMaxima)
            .ToListAsync(cancellationToken);
    }

    public async Task<ResultadoPaginado<Notificacion>> ObtenerPaginadoAsync(
        FiltroNotificaciones filtro, CancellationToken cancellationToken = default)
    {
        IQueryable<Notificacion> consulta = Conjunto.AsNoTracking()
            .Include(notificacion => notificacion.Solicitud)
            .Where(notificacion => filtro.UsuarioDestinoId == null || notificacion.UsuarioDestinoId == filtro.UsuarioDestinoId)
            .Where(notificacion => filtro.Estado == null || notificacion.Estado == filtro.Estado)
            .Where(notificacion => filtro.Canal == null || notificacion.Canal == filtro.Canal)
            .Where(notificacion => filtro.SolicitudId == null || notificacion.SolicitudId == filtro.SolicitudId);

        int totalElementos = await consulta.CountAsync(cancellationToken);

        List<Notificacion> elementos = await consulta
            .OrderByDescending(notificacion => notificacion.Fecha)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .ToListAsync(cancellationToken);

        return new ResultadoPaginado<Notificacion>(elementos, totalElementos, filtro.Pagina, filtro.TamanoPagina);
    }

    public async Task<int> ContarNoLeidasPorUsuarioAsync(
        int usuarioDestinoId, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Where(notificacion => notificacion.UsuarioDestinoId == usuarioDestinoId)
            .Where(notificacion => notificacion.Estado == EstadoNotificacion.Pendiente)
            .CountAsync(cancellationToken);
    }
}

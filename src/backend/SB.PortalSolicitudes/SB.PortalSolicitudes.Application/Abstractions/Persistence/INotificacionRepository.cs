using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

public interface INotificacionRepository : IRepositorioBase<Notificacion>
{
    Task<IReadOnlyList<Notificacion>> ObtenerPendientesAsync(
        int cantidadMaxima, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<Notificacion>> ObtenerPaginadoAsync(
        FiltroNotificaciones filtro, CancellationToken cancellationToken = default);

    Task<int> ContarNoLeidasPorUsuarioAsync(int usuarioDestinoId, CancellationToken cancellationToken = default);
}

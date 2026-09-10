using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

public interface IComentarioRepository : IRepositorioBase<Comentario>
{
    /// <summary><paramref name="incluirInternos"/> = false devuelve solo los comentarios visibles para el solicitante.</summary>
    Task<IReadOnlyList<Comentario>> ObtenerPorSolicitudAsync(
        int solicitudId, bool incluirInternos, CancellationToken cancellationToken = default);
}

using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

/// <summary>
/// Consulta la maquina de estados declarada en <see cref="TransicionPermitida"/>
/// (ver ADR-0005). No decide si una transicion es legitima: eso es responsabilidad del
/// handler que la consulta.
/// </summary>
public interface ITransicionPermitidaRepository : IRepositorioBase<TransicionPermitida>
{
    /// <summary>Incluye <c>RolesPermitidos</c>, <c>EstadoOrigen</c> y <c>EstadoDestino</c>; filtra por <c>Activo == true</c>.</summary>
    Task<TransicionPermitida?> ObtenerAsync(
        int estadoOrigenId, int estadoDestinoId, CancellationToken cancellationToken = default);

    /// <summary>Todas las transiciones activas que parten de un estado, con las mismas inclusiones.</summary>
    Task<IReadOnlyList<TransicionPermitida>> ObtenerDesdeEstadoAsync(
        int estadoOrigenId, CancellationToken cancellationToken = default);
}

using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerEstadosSolicitudActivos;

public class ObtenerEstadosSolicitudActivosQueryHandler
    : IQueryHandler<ObtenerEstadosSolicitudActivosQuery, Resultado<IReadOnlyList<EstadoSolicitudDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ObtenerEstadosSolicitudActivosQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<IReadOnlyList<EstadoSolicitudDto>>> HandleAsync(
        ObtenerEstadosSolicitudActivosQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EstadoSolicitud> estados = await _unitOfWork.EstadosSolicitud.ObtenerActivosOrdenadosAsync(cancellationToken);

        List<EstadoSolicitudDto> dtos = estados
            .Select(estado => new EstadoSolicitudDto(estado.Id, estado.Codigo, estado.Nombre, estado.Orden, estado.EsFinal))
            .ToList();

        return dtos;
    }
}

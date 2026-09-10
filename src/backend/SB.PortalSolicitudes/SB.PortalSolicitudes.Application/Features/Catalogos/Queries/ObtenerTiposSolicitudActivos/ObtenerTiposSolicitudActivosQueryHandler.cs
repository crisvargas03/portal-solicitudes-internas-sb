using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerTiposSolicitudActivos;

public class ObtenerTiposSolicitudActivosQueryHandler
    : IQueryHandler<ObtenerTiposSolicitudActivosQuery, Resultado<IReadOnlyList<CatalogoDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ObtenerTiposSolicitudActivosQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<IReadOnlyList<CatalogoDto>>> HandleAsync(
        ObtenerTiposSolicitudActivosQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TipoSolicitud> tipos = await _unitOfWork.TiposSolicitud.ObtenerActivosAsync(cancellationToken);

        List<CatalogoDto> dtos = tipos.Select(tipo => new CatalogoDto(tipo.Id, tipo.Nombre)).ToList();

        return dtos;
    }
}

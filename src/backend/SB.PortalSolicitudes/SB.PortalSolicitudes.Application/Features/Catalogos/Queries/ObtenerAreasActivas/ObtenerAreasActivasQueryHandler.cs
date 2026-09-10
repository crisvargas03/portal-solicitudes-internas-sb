using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerAreasActivas;

public class ObtenerAreasActivasQueryHandler : IQueryHandler<ObtenerAreasActivasQuery, Resultado<IReadOnlyList<CatalogoDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ObtenerAreasActivasQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<IReadOnlyList<CatalogoDto>>> HandleAsync(
        ObtenerAreasActivasQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Area> areas = await _unitOfWork.Areas.ObtenerActivosAsync(cancellationToken);

        List<CatalogoDto> dtos = areas.Select(area => new CatalogoDto(area.Id, area.Nombre)).ToList();

        return dtos;
    }
}

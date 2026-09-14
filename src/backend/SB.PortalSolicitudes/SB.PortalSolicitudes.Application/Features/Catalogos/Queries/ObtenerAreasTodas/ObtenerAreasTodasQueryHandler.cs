using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerAreasTodas;

public class ObtenerAreasTodasQueryHandler : IQueryHandler<ObtenerAreasTodasQuery, Resultado<IReadOnlyList<AreaAdminDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ObtenerAreasTodasQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<IReadOnlyList<AreaAdminDto>>> HandleAsync(
        ObtenerAreasTodasQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Area> areas = await _unitOfWork.Areas.ObtenerTodosAsync(cancellationToken);

        List<AreaAdminDto> dtos = areas
            .Select(area => new AreaAdminDto(area.Id, area.Nombre, area.Activo))
            .ToList();

        return dtos;
    }
}

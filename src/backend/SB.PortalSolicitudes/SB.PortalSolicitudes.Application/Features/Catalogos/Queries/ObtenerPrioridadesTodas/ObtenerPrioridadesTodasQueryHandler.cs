using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerPrioridadesTodas;

public class ObtenerPrioridadesTodasQueryHandler
    : IQueryHandler<ObtenerPrioridadesTodasQuery, Resultado<IReadOnlyList<PrioridadAdminDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ObtenerPrioridadesTodasQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<IReadOnlyList<PrioridadAdminDto>>> HandleAsync(
        ObtenerPrioridadesTodasQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Prioridad> prioridades = await _unitOfWork.Prioridades.ObtenerTodosAsync(cancellationToken);

        List<PrioridadAdminDto> dtos = prioridades
            .Select(prioridad => new PrioridadAdminDto(prioridad.Id, prioridad.Nombre, prioridad.Nivel, prioridad.Activo))
            .ToList();

        return dtos;
    }
}

using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerPrioridadesActivas;

public class ObtenerPrioridadesActivasQueryHandler
    : IQueryHandler<ObtenerPrioridadesActivasQuery, Resultado<IReadOnlyList<PrioridadDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ObtenerPrioridadesActivasQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<IReadOnlyList<PrioridadDto>>> HandleAsync(
        ObtenerPrioridadesActivasQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Prioridad> prioridades = await _unitOfWork.Prioridades.ObtenerActivasOrdenadasPorNivelAsync(cancellationToken);

        List<PrioridadDto> dtos = prioridades
            .Select(prioridad => new PrioridadDto(prioridad.Id, prioridad.Nombre, prioridad.Nivel))
            .ToList();

        return dtos;
    }
}

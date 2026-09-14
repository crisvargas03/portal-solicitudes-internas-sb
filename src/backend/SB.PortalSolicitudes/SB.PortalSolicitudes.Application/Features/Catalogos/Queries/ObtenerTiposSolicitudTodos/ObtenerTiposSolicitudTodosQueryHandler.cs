using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerTiposSolicitudTodos;

public class ObtenerTiposSolicitudTodosQueryHandler
    : IQueryHandler<ObtenerTiposSolicitudTodosQuery, Resultado<IReadOnlyList<TipoSolicitudAdminDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ObtenerTiposSolicitudTodosQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<IReadOnlyList<TipoSolicitudAdminDto>>> HandleAsync(
        ObtenerTiposSolicitudTodosQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TipoSolicitud> tipos = await _unitOfWork.TiposSolicitud.ObtenerTodosAsync(cancellationToken);

        List<TipoSolicitudAdminDto> dtos = tipos
            .Select(tipo => new TipoSolicitudAdminDto(tipo.Id, tipo.Nombre, tipo.Descripcion, tipo.Activo))
            .ToList();

        return dtos;
    }
}

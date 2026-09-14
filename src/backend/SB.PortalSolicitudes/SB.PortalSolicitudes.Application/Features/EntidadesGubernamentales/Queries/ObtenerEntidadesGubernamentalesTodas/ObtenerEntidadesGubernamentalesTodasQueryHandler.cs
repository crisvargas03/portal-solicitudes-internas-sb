using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Queries.ObtenerEntidadesGubernamentalesTodas;

public class ObtenerEntidadesGubernamentalesTodasQueryHandler
    : IQueryHandler<ObtenerEntidadesGubernamentalesTodasQuery, Resultado<IReadOnlyList<EntidadGubernamentalAdminDto>>>
{
    private readonly IEntidadGubernamentalRepository _repositorio;

    public ObtenerEntidadesGubernamentalesTodasQueryHandler(IEntidadGubernamentalRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<Resultado<IReadOnlyList<EntidadGubernamentalAdminDto>>> HandleAsync(
        ObtenerEntidadesGubernamentalesTodasQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EntidadGubernamental> entidades = await _repositorio.ObtenerTodasAsync(cancellationToken);

        List<EntidadGubernamentalAdminDto> dtos = entidades
            .Select(entidad => new EntidadGubernamentalAdminDto(
                entidad.Id, entidad.Nombre, entidad.Categoria, entidad.PoderDelEstado, entidad.Sector, entidad.Activo))
            .ToList();

        return dtos;
    }
}

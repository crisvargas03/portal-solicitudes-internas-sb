using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Queries.ObtenerEntidadesGubernamentalesActivas;

public class ObtenerEntidadesGubernamentalesActivasQueryHandler
    : IQueryHandler<ObtenerEntidadesGubernamentalesActivasQuery, Resultado<IReadOnlyList<EntidadGubernamentalAdminDto>>>
{
    private readonly IEntidadGubernamentalRepository _repositorio;

    public ObtenerEntidadesGubernamentalesActivasQueryHandler(IEntidadGubernamentalRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<Resultado<IReadOnlyList<EntidadGubernamentalAdminDto>>> HandleAsync(
        ObtenerEntidadesGubernamentalesActivasQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EntidadGubernamental> entidades = await _repositorio.ObtenerActivasAsync(cancellationToken);

        List<EntidadGubernamentalAdminDto> dtos = entidades.Select(AEntidadGubernamentalAdminDto).ToList();

        return dtos;
    }

    private static EntidadGubernamentalAdminDto AEntidadGubernamentalAdminDto(EntidadGubernamental entidad) =>
        new(entidad.Id, entidad.Nombre, entidad.Categoria, entidad.PoderDelEstado, entidad.Sector, entidad.Activo);
}

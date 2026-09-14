using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Queries.ObtenerEntidadGubernamentalPorId;

public class ObtenerEntidadGubernamentalPorIdQueryHandler
    : IQueryHandler<ObtenerEntidadGubernamentalPorIdQuery, Resultado<EntidadGubernamentalAdminDto>>
{
    private readonly IEntidadGubernamentalRepository _repositorio;

    public ObtenerEntidadGubernamentalPorIdQueryHandler(IEntidadGubernamentalRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<Resultado<EntidadGubernamentalAdminDto>> HandleAsync(
        ObtenerEntidadGubernamentalPorIdQuery query, CancellationToken cancellationToken = default)
    {
        EntidadGubernamental? entidad = await _repositorio.ObtenerPorIdAsync(query.Id, cancellationToken);

        if (entidad is null)
        {
            return Resultado.Fallido<EntidadGubernamentalAdminDto>(
                Error.NoEncontrado("EntidadGubernamental.NoEncontrada", "La entidad gubernamental no existe."));
        }

        return new EntidadGubernamentalAdminDto(
            entidad.Id, entidad.Nombre, entidad.Categoria, entidad.PoderDelEstado, entidad.Sector, entidad.Activo);
    }
}

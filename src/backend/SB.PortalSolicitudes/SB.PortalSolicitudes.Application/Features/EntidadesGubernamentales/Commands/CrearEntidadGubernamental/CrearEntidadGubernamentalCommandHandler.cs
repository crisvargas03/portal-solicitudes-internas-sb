using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Commands.CrearEntidadGubernamental;

public class CrearEntidadGubernamentalCommandHandler
    : ICommandHandler<CrearEntidadGubernamentalCommand, Resultado<EntidadGubernamentalAdminDto>>
{
    private readonly IEntidadGubernamentalRepository _repositorio;

    public CrearEntidadGubernamentalCommandHandler(IEntidadGubernamentalRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<Resultado<EntidadGubernamentalAdminDto>> HandleAsync(
        CrearEntidadGubernamentalCommand command, CancellationToken cancellationToken = default)
    {
        bool existeNombre = await _repositorio.ExisteNombreAsync(command.Nombre, cancellationToken: cancellationToken);

        if (existeNombre)
        {
            return Resultado.Fallido<EntidadGubernamentalAdminDto>(
                Error.Conflicto("EntidadGubernamental.NombreDuplicado", "Ya existe una entidad gubernamental con ese nombre."));
        }

        EntidadGubernamental entidad = new()
        {
            Nombre = command.Nombre,
            Categoria = command.Categoria,
            PoderDelEstado = command.PoderDelEstado,
            Sector = command.Sector,
            Activo = true
        };

        entidad = await _repositorio.CrearAsync(entidad, cancellationToken);

        return new EntidadGubernamentalAdminDto(
            entidad.Id, entidad.Nombre, entidad.Categoria, entidad.PoderDelEstado, entidad.Sector, entidad.Activo);
    }
}

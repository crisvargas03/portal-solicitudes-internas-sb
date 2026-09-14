using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Commands.ActualizarEntidadGubernamental;

public class ActualizarEntidadGubernamentalCommandHandler
    : ICommandHandler<ActualizarEntidadGubernamentalCommand, Resultado<EntidadGubernamentalAdminDto>>
{
    private readonly IEntidadGubernamentalRepository _repositorio;

    public ActualizarEntidadGubernamentalCommandHandler(IEntidadGubernamentalRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<Resultado<EntidadGubernamentalAdminDto>> HandleAsync(
        ActualizarEntidadGubernamentalCommand command, CancellationToken cancellationToken = default)
    {
        EntidadGubernamental? entidad = await _repositorio.ObtenerPorIdAsync(command.Id, cancellationToken);

        if (entidad is null)
        {
            return Resultado.Fallido<EntidadGubernamentalAdminDto>(
                Error.NoEncontrado("EntidadGubernamental.NoEncontrada", "La entidad gubernamental no existe."));
        }

        if (command.Nombre is not null)
        {
            bool existeNombre = await _repositorio.ExisteNombreAsync(command.Nombre, command.Id, cancellationToken);

            if (existeNombre)
            {
                return Resultado.Fallido<EntidadGubernamentalAdminDto>(
                    Error.Conflicto("EntidadGubernamental.NombreDuplicado", "Ya existe una entidad gubernamental con ese nombre."));
            }
        }

        entidad.Nombre = command.Nombre ?? entidad.Nombre;
        entidad.Categoria = command.Categoria ?? entidad.Categoria;
        entidad.PoderDelEstado = command.PoderDelEstado ?? entidad.PoderDelEstado;
        entidad.Sector = command.Sector ?? entidad.Sector;
        entidad.Activo = command.Activo ?? entidad.Activo;

        await _repositorio.ActualizarAsync(entidad, cancellationToken);

        return new EntidadGubernamentalAdminDto(
            entidad.Id, entidad.Nombre, entidad.Categoria, entidad.PoderDelEstado, entidad.Sector, entidad.Activo);
    }
}

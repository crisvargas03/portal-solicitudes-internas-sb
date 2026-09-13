using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarPrioridad;

public class ActualizarPrioridadCommandHandler : ICommandHandler<ActualizarPrioridadCommand, Resultado<PrioridadAdminDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarPrioridadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<PrioridadAdminDto>> HandleAsync(
        ActualizarPrioridadCommand command, CancellationToken cancellationToken = default)
    {
        Prioridad? prioridad = await _unitOfWork.Prioridades.ObtenerPorIdAsync(command.Id, cancellationToken);

        if (prioridad is null)
        {
            return Resultado.Fallido<PrioridadAdminDto>(
                Error.NoEncontrado("Prioridad.NoEncontrada", "La prioridad no existe."));
        }

        if (command.Nombre is not null)
        {
            bool existeNombre = await _unitOfWork.Prioridades.ExisteNombreAsync(command.Nombre, command.Id, cancellationToken);

            if (existeNombre)
            {
                return Resultado.Fallido<PrioridadAdminDto>(
                    Error.Conflicto("Prioridad.NombreDuplicado", "Ya existe una prioridad con ese nombre."));
            }
        }

        prioridad.Nombre = command.Nombre ?? prioridad.Nombre;
        prioridad.Nivel = command.Nivel ?? prioridad.Nivel;
        prioridad.Activo = command.Activo ?? prioridad.Activo;

        _unitOfWork.Prioridades.Actualizar(prioridad);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new PrioridadAdminDto(prioridad.Id, prioridad.Nombre, prioridad.Nivel, prioridad.Activo);
    }
}

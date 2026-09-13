using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearPrioridad;

public class CrearPrioridadCommandHandler : ICommandHandler<CrearPrioridadCommand, Resultado<PrioridadAdminDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CrearPrioridadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<PrioridadAdminDto>> HandleAsync(
        CrearPrioridadCommand command, CancellationToken cancellationToken = default)
    {
        bool existeNombre = await _unitOfWork.Prioridades.ExisteNombreAsync(command.Nombre, cancellationToken: cancellationToken);

        if (existeNombre)
        {
            return Resultado.Fallido<PrioridadAdminDto>(
                Error.Conflicto("Prioridad.NombreDuplicado", "Ya existe una prioridad con ese nombre."));
        }

        Prioridad prioridad = new() { Nombre = command.Nombre, Nivel = command.Nivel, Activo = true };

        await _unitOfWork.Prioridades.AgregarAsync(prioridad, cancellationToken);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new PrioridadAdminDto(prioridad.Id, prioridad.Nombre, prioridad.Nivel, prioridad.Activo);
    }
}

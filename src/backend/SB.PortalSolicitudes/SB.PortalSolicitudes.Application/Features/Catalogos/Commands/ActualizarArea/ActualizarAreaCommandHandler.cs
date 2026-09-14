using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarArea;

public class ActualizarAreaCommandHandler : ICommandHandler<ActualizarAreaCommand, Resultado<AreaAdminDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarAreaCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<AreaAdminDto>> HandleAsync(
        ActualizarAreaCommand command, CancellationToken cancellationToken = default)
    {
        Area? area = await _unitOfWork.Areas.ObtenerPorIdAsync(command.Id, cancellationToken);

        if (area is null)
        {
            return Resultado.Fallido<AreaAdminDto>(Error.NoEncontrado("Area.NoEncontrada", "El area no existe."));
        }

        if (command.Nombre is not null)
        {
            bool existeNombre = await _unitOfWork.Areas.ExisteNombreAsync(command.Nombre, command.Id, cancellationToken);

            if (existeNombre)
            {
                return Resultado.Fallido<AreaAdminDto>(
                    Error.Conflicto("Area.NombreDuplicado", "Ya existe un area con ese nombre."));
            }
        }

        area.Nombre = command.Nombre ?? area.Nombre;
        area.Activo = command.Activo ?? area.Activo;

        _unitOfWork.Areas.Actualizar(area);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new AreaAdminDto(area.Id, area.Nombre, area.Activo);
    }
}

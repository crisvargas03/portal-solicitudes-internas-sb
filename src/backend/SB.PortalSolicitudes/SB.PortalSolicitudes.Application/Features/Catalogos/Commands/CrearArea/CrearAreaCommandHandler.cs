using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearArea;

public class CrearAreaCommandHandler : ICommandHandler<CrearAreaCommand, Resultado<AreaAdminDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CrearAreaCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<AreaAdminDto>> HandleAsync(
        CrearAreaCommand command, CancellationToken cancellationToken = default)
    {
        bool existeNombre = await _unitOfWork.Areas.ExisteNombreAsync(command.Nombre, cancellationToken: cancellationToken);

        if (existeNombre)
        {
            return Resultado.Fallido<AreaAdminDto>(
                Error.Conflicto("Area.NombreDuplicado", "Ya existe un area con ese nombre."));
        }

        Area area = new() { Nombre = command.Nombre, Activo = true };

        await _unitOfWork.Areas.AgregarAsync(area, cancellationToken);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new AreaAdminDto(area.Id, area.Nombre, area.Activo);
    }
}

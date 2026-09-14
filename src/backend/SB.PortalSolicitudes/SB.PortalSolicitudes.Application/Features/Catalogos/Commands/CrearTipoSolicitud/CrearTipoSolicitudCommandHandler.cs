using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearTipoSolicitud;

public class CrearTipoSolicitudCommandHandler : ICommandHandler<CrearTipoSolicitudCommand, Resultado<TipoSolicitudAdminDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CrearTipoSolicitudCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<TipoSolicitudAdminDto>> HandleAsync(
        CrearTipoSolicitudCommand command, CancellationToken cancellationToken = default)
    {
        bool existeNombre = await _unitOfWork.TiposSolicitud.ExisteNombreAsync(command.Nombre, cancellationToken: cancellationToken);

        if (existeNombre)
        {
            return Resultado.Fallido<TipoSolicitudAdminDto>(
                Error.Conflicto("TipoSolicitud.NombreDuplicado", "Ya existe un tipo de solicitud con ese nombre."));
        }

        TipoSolicitud tipo = new() { Nombre = command.Nombre, Descripcion = command.Descripcion, Activo = true };

        await _unitOfWork.TiposSolicitud.AgregarAsync(tipo, cancellationToken);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new TipoSolicitudAdminDto(tipo.Id, tipo.Nombre, tipo.Descripcion, tipo.Activo);
    }
}

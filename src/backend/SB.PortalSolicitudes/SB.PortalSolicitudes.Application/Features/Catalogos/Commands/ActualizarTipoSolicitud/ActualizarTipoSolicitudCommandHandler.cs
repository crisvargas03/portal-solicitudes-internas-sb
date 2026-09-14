using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarTipoSolicitud;

public class ActualizarTipoSolicitudCommandHandler
    : ICommandHandler<ActualizarTipoSolicitudCommand, Resultado<TipoSolicitudAdminDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarTipoSolicitudCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<TipoSolicitudAdminDto>> HandleAsync(
        ActualizarTipoSolicitudCommand command, CancellationToken cancellationToken = default)
    {
        TipoSolicitud? tipo = await _unitOfWork.TiposSolicitud.ObtenerPorIdAsync(command.Id, cancellationToken);

        if (tipo is null)
        {
            return Resultado.Fallido<TipoSolicitudAdminDto>(
                Error.NoEncontrado("TipoSolicitud.NoEncontrado", "El tipo de solicitud no existe."));
        }

        if (command.Nombre is not null)
        {
            bool existeNombre = await _unitOfWork.TiposSolicitud.ExisteNombreAsync(command.Nombre, command.Id, cancellationToken);

            if (existeNombre)
            {
                return Resultado.Fallido<TipoSolicitudAdminDto>(
                    Error.Conflicto("TipoSolicitud.NombreDuplicado", "Ya existe un tipo de solicitud con ese nombre."));
            }
        }

        tipo.Nombre = command.Nombre ?? tipo.Nombre;
        tipo.Descripcion = command.Descripcion ?? tipo.Descripcion;
        tipo.Activo = command.Activo ?? tipo.Activo;

        _unitOfWork.TiposSolicitud.Actualizar(tipo);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new TipoSolicitudAdminDto(tipo.Id, tipo.Nombre, tipo.Descripcion, tipo.Activo);
    }
}

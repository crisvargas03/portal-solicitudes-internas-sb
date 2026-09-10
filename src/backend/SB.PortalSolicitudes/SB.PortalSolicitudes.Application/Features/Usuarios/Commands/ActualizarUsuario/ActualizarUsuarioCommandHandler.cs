using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Usuarios.Commands.ActualizarUsuario;

public class ActualizarUsuarioCommandHandler : ICommandHandler<ActualizarUsuarioCommand, Resultado<UsuarioResumenDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarUsuarioCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<UsuarioResumenDto>> HandleAsync(
        ActualizarUsuarioCommand command, CancellationToken cancellationToken = default)
    {
        Usuario? usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(command.Id, cancellationToken);

        if (usuario is null)
        {
            return Resultado.Fallido<UsuarioResumenDto>(Error.NoEncontrado("Usuario.NoEncontrado", "El usuario no existe."));
        }

        usuario.Nombre = command.Nombre ?? usuario.Nombre;
        usuario.Rol = command.Rol ?? usuario.Rol;
        usuario.Activo = command.Activo ?? usuario.Activo;

        _unitOfWork.Usuarios.Actualizar(usuario);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new UsuarioResumenDto(usuario.Id, usuario.Nombre, usuario.Email, usuario.Rol.ToString());
    }
}

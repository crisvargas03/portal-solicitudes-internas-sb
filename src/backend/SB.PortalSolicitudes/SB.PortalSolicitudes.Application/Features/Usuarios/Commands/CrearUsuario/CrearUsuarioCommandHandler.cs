using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Usuarios.Commands.CrearUsuario;

public class CrearUsuarioCommandHandler : ICommandHandler<CrearUsuarioCommand, Resultado<UsuarioResumenDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHasheadorPasswords _hasheadorPasswords;

    public CrearUsuarioCommandHandler(IUnitOfWork unitOfWork, IHasheadorPasswords hasheadorPasswords)
    {
        _unitOfWork = unitOfWork;
        _hasheadorPasswords = hasheadorPasswords;
    }

    public async Task<Resultado<UsuarioResumenDto>> HandleAsync(
        CrearUsuarioCommand command, CancellationToken cancellationToken = default)
    {
        bool existeEmail = await _unitOfWork.Usuarios.ExisteEmailAsync(command.Email, cancellationToken: cancellationToken);

        if (existeEmail)
        {
            return Resultado.Fallido<UsuarioResumenDto>(
                Error.Conflicto("Usuario.EmailDuplicado", "Ya existe un usuario con ese correo."));
        }

        Usuario usuario = new()
        {
            Nombre = command.Nombre,
            Email = command.Email,
            Rol = command.Rol,
            PasswordHash = _hasheadorPasswords.Hashear(command.Password),
            Activo = true
        };

        await _unitOfWork.Usuarios.AgregarAsync(usuario, cancellationToken);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new UsuarioResumenDto(usuario.Id, usuario.Nombre, usuario.Email, usuario.Rol.ToString());
    }
}

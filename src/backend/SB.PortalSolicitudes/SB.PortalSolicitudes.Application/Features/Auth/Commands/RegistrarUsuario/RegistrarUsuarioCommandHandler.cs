using LiteBus.Commands.Abstractions;
using Microsoft.Extensions.Logging;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Auth.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Features.Auth.Commands.RegistrarUsuario;

public class RegistrarUsuarioCommandHandler : ICommandHandler<RegistrarUsuarioCommand, Resultado<SesionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHasheadorPasswords _hasheadorPasswords;
    private readonly IProveedorTokens _proveedorTokens;
    private readonly ILogger<RegistrarUsuarioCommandHandler> _logger;

    public RegistrarUsuarioCommandHandler(
        IUnitOfWork unitOfWork,
        IHasheadorPasswords hasheadorPasswords,
        IProveedorTokens proveedorTokens,
        ILogger<RegistrarUsuarioCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _hasheadorPasswords = hasheadorPasswords;
        _proveedorTokens = proveedorTokens;
        _logger = logger;
    }

    public async Task<Resultado<SesionDto>> HandleAsync(
        RegistrarUsuarioCommand command, CancellationToken cancellationToken = default)
    {
        bool existeEmail = await _unitOfWork.Usuarios.ExisteEmailAsync(command.Email, cancellationToken: cancellationToken);

        if (existeEmail)
        {
            return Resultado.Fallido<SesionDto>(
                Error.Conflicto("Usuario.EmailDuplicado", "Ya existe un usuario con ese correo."));
        }

        Usuario usuario = new()
        {
            Nombre = command.Nombre,
            Email = command.Email,
            Rol = RolUsuario.Solicitante,
            PasswordHash = _hasheadorPasswords.Hashear(command.Password),
            Activo = true
        };

        await _unitOfWork.Usuarios.AgregarAsync(usuario, cancellationToken);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        _logger.LogInformation("Registro exitoso para {Email}", command.Email);

        (string token, DateTime expiraEn) = _proveedorTokens.GenerarToken(usuario);

        UsuarioResumenDto usuarioDto = new(usuario.Id, usuario.Nombre, usuario.Email, usuario.Rol.ToString());

        return new SesionDto(token, expiraEn, usuarioDto);
    }
}

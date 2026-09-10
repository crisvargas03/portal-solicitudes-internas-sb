using LiteBus.Commands.Abstractions;
using Microsoft.Extensions.Logging;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Auth.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Auth.Commands.IniciarSesion;

public class IniciarSesionCommandHandler : ICommandHandler<IniciarSesionCommand, Resultado<SesionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHasheadorPasswords _hasheadorPasswords;
    private readonly IProveedorTokens _proveedorTokens;
    private readonly ILogger<IniciarSesionCommandHandler> _logger;

    public IniciarSesionCommandHandler(
        IUnitOfWork unitOfWork,
        IHasheadorPasswords hasheadorPasswords,
        IProveedorTokens proveedorTokens,
        ILogger<IniciarSesionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _hasheadorPasswords = hasheadorPasswords;
        _proveedorTokens = proveedorTokens;
        _logger = logger;
    }

    public async Task<Resultado<SesionDto>> HandleAsync(
        IniciarSesionCommand command, CancellationToken cancellationToken = default)
    {
        Usuario? usuario = await _unitOfWork.Usuarios.ObtenerPorEmailAsync(command.Email, cancellationToken);

        bool credencialesValidas = usuario is not null
            && usuario.Activo
            && _hasheadorPasswords.Verificar(command.Password, usuario.PasswordHash);

        if (!credencialesValidas)
        {
            _logger.LogWarning("Inicio de sesion fallido para {Email}", command.Email);

            return Resultado.Fallido<SesionDto>(
                Error.NoAutorizado("Auth.CredencialesInvalidas", "Correo o contraseña incorrectos."));
        }

        (string token, DateTime expiraEn) = _proveedorTokens.GenerarToken(usuario!);

        _logger.LogInformation("Inicio de sesion exitoso para {Email}", command.Email);

        UsuarioResumenDto usuarioDto = new(usuario!.Id, usuario.Nombre, usuario.Email, usuario.Rol.ToString());

        return new SesionDto(token, expiraEn, usuarioDto);
    }
}

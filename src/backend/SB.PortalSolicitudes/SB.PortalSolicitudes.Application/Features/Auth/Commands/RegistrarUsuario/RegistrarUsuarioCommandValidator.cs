using FluentValidation;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Auth.Commands.RegistrarUsuario;

public class RegistrarUsuarioCommandValidator : AbstractValidator<RegistrarUsuarioCommand>
{
    private const int LONGITUD_MINIMA_PASSWORD = 8;

    public RegistrarUsuarioCommandValidator()
    {
        RuleFor(comando => comando.Nombre).NotEmpty().MaximumLength(Usuario.MAX_LONGITUD_NOMBRE);
        RuleFor(comando => comando.Email).NotEmpty().EmailAddress().MaximumLength(Usuario.MAX_LONGITUD_EMAIL);
        RuleFor(comando => comando.Password).NotEmpty().MinimumLength(LONGITUD_MINIMA_PASSWORD);
    }
}

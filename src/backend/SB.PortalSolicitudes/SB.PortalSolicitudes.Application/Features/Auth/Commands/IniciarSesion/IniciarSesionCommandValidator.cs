using FluentValidation;

namespace SB.PortalSolicitudes.Application.Features.Auth.Commands.IniciarSesion;

public class IniciarSesionCommandValidator : AbstractValidator<IniciarSesionCommand>
{
    public IniciarSesionCommandValidator()
    {
        RuleFor(comando => comando.Email).NotEmpty().EmailAddress();
        RuleFor(comando => comando.Password).NotEmpty();
    }
}

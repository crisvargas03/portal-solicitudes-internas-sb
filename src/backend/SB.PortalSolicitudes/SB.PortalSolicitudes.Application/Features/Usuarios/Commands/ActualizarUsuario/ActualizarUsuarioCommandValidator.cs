using FluentValidation;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Usuarios.Commands.ActualizarUsuario;

public class ActualizarUsuarioCommandValidator : AbstractValidator<ActualizarUsuarioCommand>
{
    public ActualizarUsuarioCommandValidator()
    {
        RuleFor(comando => comando.Nombre)
            .NotEmpty().MaximumLength(Usuario.MAX_LONGITUD_NOMBRE)
            .When(comando => comando.Nombre is not null);

        RuleFor(comando => comando.Rol).IsInEnum().When(comando => comando.Rol is not null);
    }
}

using FluentValidation;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Usuarios.Commands.CrearUsuario;

public class CrearUsuarioCommandValidator : AbstractValidator<CrearUsuarioCommand>
{
    private const int LONGITUD_MINIMA_PASSWORD = 8;

    public CrearUsuarioCommandValidator()
    {
        RuleFor(comando => comando.Nombre).NotEmpty().MaximumLength(Usuario.MAX_LONGITUD_NOMBRE);
        RuleFor(comando => comando.Email).NotEmpty().EmailAddress().MaximumLength(Usuario.MAX_LONGITUD_EMAIL);
        RuleFor(comando => comando.Password).NotEmpty().MinimumLength(LONGITUD_MINIMA_PASSWORD);
        RuleFor(comando => comando.Rol).IsInEnum();
    }
}

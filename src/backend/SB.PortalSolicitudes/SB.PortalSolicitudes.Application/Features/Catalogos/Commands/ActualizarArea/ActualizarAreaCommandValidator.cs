using FluentValidation;
using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarArea;

public class ActualizarAreaCommandValidator : AbstractValidator<ActualizarAreaCommand>
{
    public ActualizarAreaCommandValidator()
    {
        RuleFor(comando => comando.Nombre)
            .NotEmpty().MaximumLength(CatalogoBase.MAX_LONGITUD_NOMBRE)
            .When(comando => comando.Nombre is not null);
    }
}

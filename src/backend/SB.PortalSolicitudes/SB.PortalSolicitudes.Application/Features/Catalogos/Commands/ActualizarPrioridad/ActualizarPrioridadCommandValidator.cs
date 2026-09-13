using FluentValidation;
using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarPrioridad;

public class ActualizarPrioridadCommandValidator : AbstractValidator<ActualizarPrioridadCommand>
{
    public ActualizarPrioridadCommandValidator()
    {
        RuleFor(comando => comando.Nombre)
            .NotEmpty().MaximumLength(CatalogoBase.MAX_LONGITUD_NOMBRE)
            .When(comando => comando.Nombre is not null);

        RuleFor(comando => comando.Nivel).GreaterThan(0).When(comando => comando.Nivel is not null);
    }
}

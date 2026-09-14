using FluentValidation;
using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearPrioridad;

public class CrearPrioridadCommandValidator : AbstractValidator<CrearPrioridadCommand>
{
    public CrearPrioridadCommandValidator()
    {
        RuleFor(comando => comando.Nombre).NotEmpty().MaximumLength(CatalogoBase.MAX_LONGITUD_NOMBRE);
        RuleFor(comando => comando.Nivel).GreaterThan(0);
    }
}

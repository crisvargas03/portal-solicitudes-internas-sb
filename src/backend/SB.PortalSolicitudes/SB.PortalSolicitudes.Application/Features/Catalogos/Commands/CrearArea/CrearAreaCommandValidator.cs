using FluentValidation;
using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearArea;

public class CrearAreaCommandValidator : AbstractValidator<CrearAreaCommand>
{
    public CrearAreaCommandValidator()
    {
        RuleFor(comando => comando.Nombre).NotEmpty().MaximumLength(CatalogoBase.MAX_LONGITUD_NOMBRE);
    }
}

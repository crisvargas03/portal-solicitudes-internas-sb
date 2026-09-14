using FluentValidation;
using SB.PortalSolicitudes.Domain.Common;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearTipoSolicitud;

public class CrearTipoSolicitudCommandValidator : AbstractValidator<CrearTipoSolicitudCommand>
{
    public CrearTipoSolicitudCommandValidator()
    {
        RuleFor(comando => comando.Nombre).NotEmpty().MaximumLength(CatalogoBase.MAX_LONGITUD_NOMBRE);

        RuleFor(comando => comando.Descripcion)
            .MaximumLength(TipoSolicitud.MAX_LONGITUD_DESCRIPCION)
            .When(comando => comando.Descripcion is not null);
    }
}

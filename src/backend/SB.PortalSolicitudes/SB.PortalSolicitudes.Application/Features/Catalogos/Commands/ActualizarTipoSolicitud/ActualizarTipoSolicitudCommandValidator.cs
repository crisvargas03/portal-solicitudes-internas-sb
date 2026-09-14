using FluentValidation;
using SB.PortalSolicitudes.Domain.Common;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarTipoSolicitud;

public class ActualizarTipoSolicitudCommandValidator : AbstractValidator<ActualizarTipoSolicitudCommand>
{
    public ActualizarTipoSolicitudCommandValidator()
    {
        RuleFor(comando => comando.Nombre)
            .NotEmpty().MaximumLength(CatalogoBase.MAX_LONGITUD_NOMBRE)
            .When(comando => comando.Nombre is not null);

        RuleFor(comando => comando.Descripcion)
            .MaximumLength(TipoSolicitud.MAX_LONGITUD_DESCRIPCION)
            .When(comando => comando.Descripcion is not null);
    }
}

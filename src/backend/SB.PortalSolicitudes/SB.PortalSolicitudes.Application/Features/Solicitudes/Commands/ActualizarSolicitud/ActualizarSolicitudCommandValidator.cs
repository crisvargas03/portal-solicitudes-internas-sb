using FluentValidation;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.ActualizarSolicitud;

public class ActualizarSolicitudCommandValidator : AbstractValidator<ActualizarSolicitudCommand>
{
    public ActualizarSolicitudCommandValidator()
    {
        RuleFor(comando => comando.Titulo)
            .NotEmpty().MaximumLength(Solicitud.MAX_LONGITUD_TITULO)
            .When(comando => comando.Titulo is not null);

        RuleFor(comando => comando.Descripcion)
            .NotEmpty().MaximumLength(Solicitud.MAX_LONGITUD_DESCRIPCION)
            .When(comando => comando.Descripcion is not null);

        RuleFor(comando => comando.TipoSolicitudId).GreaterThan(0).When(comando => comando.TipoSolicitudId is not null);
        RuleFor(comando => comando.PrioridadId).GreaterThan(0).When(comando => comando.PrioridadId is not null);
        RuleFor(comando => comando.AreaId).GreaterThan(0).When(comando => comando.AreaId is not null);
    }
}

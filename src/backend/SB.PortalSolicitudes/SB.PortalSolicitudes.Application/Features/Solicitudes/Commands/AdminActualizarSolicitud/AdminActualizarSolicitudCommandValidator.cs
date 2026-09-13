using FluentValidation;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.AdminActualizarSolicitud;

public class AdminActualizarSolicitudCommandValidator : AbstractValidator<AdminActualizarSolicitudCommand>
{
    public AdminActualizarSolicitudCommandValidator()
    {
        RuleFor(comando => comando.Titulo).NotEmpty().MaximumLength(Solicitud.MAX_LONGITUD_TITULO);
        RuleFor(comando => comando.Descripcion).NotEmpty().MaximumLength(Solicitud.MAX_LONGITUD_DESCRIPCION);
        RuleFor(comando => comando.TipoSolicitudId).GreaterThan(0);
        RuleFor(comando => comando.PrioridadId).GreaterThan(0);
        RuleFor(comando => comando.AreaId).GreaterThan(0);
    }
}

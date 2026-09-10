using FluentValidation;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearAdjunto;

public class CrearAdjuntoCommandValidator : AbstractValidator<CrearAdjuntoCommand>
{
    public CrearAdjuntoCommandValidator()
    {
        RuleFor(comando => comando.Descripcion).NotEmpty().MaximumLength(Adjunto.MAX_LONGITUD_DESCRIPCION);
        RuleFor(comando => comando.Url).NotEmpty().MaximumLength(Adjunto.MAX_LONGITUD_URL);
    }
}

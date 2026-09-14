using FluentValidation;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CambiarEstado;

public class CambiarEstadoCommandValidator : AbstractValidator<CambiarEstadoCommand>
{
    public CambiarEstadoCommandValidator()
    {
        RuleFor(comando => comando.EstadoDestinoId).GreaterThan(0);

        RuleFor(comando => comando.Comentario)
            .MaximumLength(HistorialEstado.MAX_LONGITUD_COMENTARIO)
            .When(comando => comando.Comentario is not null);
    }
}

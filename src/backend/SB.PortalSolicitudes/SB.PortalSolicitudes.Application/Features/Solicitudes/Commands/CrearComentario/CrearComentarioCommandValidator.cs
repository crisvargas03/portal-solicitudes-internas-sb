using FluentValidation;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearComentario;

public class CrearComentarioCommandValidator : AbstractValidator<CrearComentarioCommand>
{
    public CrearComentarioCommandValidator()
    {
        RuleFor(comando => comando.Texto).NotEmpty().MaximumLength(Comentario.MAX_LONGITUD_TEXTO);
    }
}

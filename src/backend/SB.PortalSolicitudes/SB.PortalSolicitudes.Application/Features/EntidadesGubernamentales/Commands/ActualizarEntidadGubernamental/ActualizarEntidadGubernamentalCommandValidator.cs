using FluentValidation;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Commands.ActualizarEntidadGubernamental;

public class ActualizarEntidadGubernamentalCommandValidator : AbstractValidator<ActualizarEntidadGubernamentalCommand>
{
    public ActualizarEntidadGubernamentalCommandValidator()
    {
        RuleFor(comando => comando.Nombre)
            .NotEmpty().MaximumLength(EntidadGubernamental.MAX_LONGITUD_NOMBRE).DebeSerCampoDeArchivoValido()
            .When(comando => comando.Nombre is not null);

        RuleFor(comando => comando.Categoria)
            .NotEmpty().MaximumLength(EntidadGubernamental.MAX_LONGITUD_CATEGORIA).DebeSerCampoDeArchivoValido()
            .When(comando => comando.Categoria is not null);

        RuleFor(comando => comando.PoderDelEstado)
            .NotEmpty().MaximumLength(EntidadGubernamental.MAX_LONGITUD_PODER_DEL_ESTADO).DebeSerCampoDeArchivoValido()
            .When(comando => comando.PoderDelEstado is not null);

        RuleFor(comando => comando.Sector)
            .NotEmpty().MaximumLength(EntidadGubernamental.MAX_LONGITUD_SECTOR).DebeSerCampoDeArchivoValido()
            .When(comando => comando.Sector is not null);
    }
}

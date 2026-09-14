using FluentValidation;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Commands.CrearEntidadGubernamental;

public class CrearEntidadGubernamentalCommandValidator : AbstractValidator<CrearEntidadGubernamentalCommand>
{
    public CrearEntidadGubernamentalCommandValidator()
    {
        RuleFor(comando => comando.Nombre)
            .NotEmpty().MaximumLength(EntidadGubernamental.MAX_LONGITUD_NOMBRE).DebeSerCampoDeArchivoValido();
        RuleFor(comando => comando.Categoria)
            .NotEmpty().MaximumLength(EntidadGubernamental.MAX_LONGITUD_CATEGORIA).DebeSerCampoDeArchivoValido();
        RuleFor(comando => comando.PoderDelEstado)
            .NotEmpty().MaximumLength(EntidadGubernamental.MAX_LONGITUD_PODER_DEL_ESTADO).DebeSerCampoDeArchivoValido();
        RuleFor(comando => comando.Sector)
            .NotEmpty().MaximumLength(EntidadGubernamental.MAX_LONGITUD_SECTOR).DebeSerCampoDeArchivoValido();
    }
}

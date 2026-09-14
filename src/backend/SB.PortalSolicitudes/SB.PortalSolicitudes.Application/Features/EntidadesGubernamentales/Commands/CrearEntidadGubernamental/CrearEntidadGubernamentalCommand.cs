using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Dtos;

namespace SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Commands.CrearEntidadGubernamental;

public sealed record CrearEntidadGubernamentalCommand(string Nombre, string Categoria, string PoderDelEstado, string Sector)
    : ICommand<Resultado<EntidadGubernamentalAdminDto>>;

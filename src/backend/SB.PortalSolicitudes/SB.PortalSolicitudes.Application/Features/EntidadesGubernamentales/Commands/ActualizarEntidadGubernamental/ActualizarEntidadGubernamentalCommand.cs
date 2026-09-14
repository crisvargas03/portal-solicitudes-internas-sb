using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Dtos;

namespace SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Commands.ActualizarEntidadGubernamental;

/// <summary>Edicion parcial: un campo en <c>null</c> significa "sin cambios".</summary>
public sealed record ActualizarEntidadGubernamentalCommand(
    int Id, string? Nombre, string? Categoria, string? PoderDelEstado, string? Sector, bool? Activo)
    : ICommand<Resultado<EntidadGubernamentalAdminDto>>;

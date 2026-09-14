using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarPrioridad;

/// <summary>Edicion parcial: un campo en <c>null</c> significa "sin cambios".</summary>
public sealed record ActualizarPrioridadCommand(int Id, string? Nombre, int? Nivel, bool? Activo)
    : ICommand<Resultado<PrioridadAdminDto>>;

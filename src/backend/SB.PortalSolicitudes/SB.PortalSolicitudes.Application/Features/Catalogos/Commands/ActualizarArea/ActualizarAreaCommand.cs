using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarArea;

/// <summary>Edicion parcial: un campo en <c>null</c> significa "sin cambios".</summary>
public sealed record ActualizarAreaCommand(int Id, string? Nombre, bool? Activo) : ICommand<Resultado<AreaAdminDto>>;

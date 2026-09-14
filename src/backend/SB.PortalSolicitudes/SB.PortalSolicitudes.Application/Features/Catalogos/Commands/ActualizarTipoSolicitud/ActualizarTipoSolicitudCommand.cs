using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarTipoSolicitud;

/// <summary>Edicion parcial: un campo en <c>null</c> significa "sin cambios".</summary>
public sealed record ActualizarTipoSolicitudCommand(int Id, string? Nombre, string? Descripcion, bool? Activo)
    : ICommand<Resultado<TipoSolicitudAdminDto>>;

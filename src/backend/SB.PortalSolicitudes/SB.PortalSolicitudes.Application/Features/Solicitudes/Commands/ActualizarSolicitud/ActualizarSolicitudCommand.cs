using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.ActualizarSolicitud;

/// <summary>
/// Edicion parcial: un campo en <c>null</c> significa "sin cambios", no "vaciar". Solo
/// aplica mientras la solicitud esta en <c>REGISTRADA</c> (ver el handler).
/// </summary>
public sealed record ActualizarSolicitudCommand(
    int Id,
    string? Titulo,
    string? Descripcion,
    int? TipoSolicitudId,
    int? PrioridadId,
    int? AreaId,
    DateTime? FechaCompromiso) : ICommand<Resultado<SolicitudResumenDto>>;

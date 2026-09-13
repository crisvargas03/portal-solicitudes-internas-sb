using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.AdminActualizarSolicitud;

/// <summary>
/// Edicion completa de Administrador, sin la restriccion de <c>REGISTRADA</c> que aplica a
/// <see cref="ActualizarSolicitud.ActualizarSolicitudCommand"/> (ver ADR-0020). No toca
/// <c>Estado</c> ni <c>UsuarioAsignadoId</c>: esos siguen gobernados por sus propios
/// endpoints (<c>PATCH .../estado</c>, <c>PATCH .../asignacion</c>).
/// </summary>
public sealed record AdminActualizarSolicitudCommand(
    int Id,
    string Titulo,
    string Descripcion,
    int TipoSolicitudId,
    int PrioridadId,
    int AreaId) : ICommand<Resultado<SolicitudResumenDto>>;

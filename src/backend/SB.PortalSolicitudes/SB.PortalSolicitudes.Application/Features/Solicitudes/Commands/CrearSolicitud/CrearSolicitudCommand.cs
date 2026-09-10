using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearSolicitud;

/// <summary>Sin <c>UsuarioSolicitanteId</c>: sale del token (ver <c>IUsuarioActual</c>), no del cliente.</summary>
public sealed record CrearSolicitudCommand(
    string Titulo,
    string Descripcion,
    int TipoSolicitudId,
    int PrioridadId,
    int AreaId,
    DateTime? FechaCompromiso) : ICommand<Resultado<SolicitudResumenDto>>;

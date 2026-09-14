using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CambiarAsignacion;

/// <summary><c>UsuarioAsignadoId</c> nulo desasigna. Ver el handler para las reglas de quien puede asignar a quien (ADR-0012).</summary>
public sealed record CambiarAsignacionCommand(int Id, int? UsuarioAsignadoId) : ICommand<Resultado<SolicitudResumenDto>>;

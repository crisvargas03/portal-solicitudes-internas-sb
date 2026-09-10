using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CambiarEstado;

public sealed record CambiarEstadoCommand(int Id, int EstadoDestinoId, string? Comentario)
    : ICommand<Resultado<SolicitudResumenDto>>;

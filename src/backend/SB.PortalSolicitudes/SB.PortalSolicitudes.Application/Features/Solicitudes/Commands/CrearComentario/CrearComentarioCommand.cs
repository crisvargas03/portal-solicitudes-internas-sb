using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearComentario;

public sealed record CrearComentarioCommand(int SolicitudId, string Texto, bool EsInterno)
    : ICommand<Resultado<ComentarioDto>>;

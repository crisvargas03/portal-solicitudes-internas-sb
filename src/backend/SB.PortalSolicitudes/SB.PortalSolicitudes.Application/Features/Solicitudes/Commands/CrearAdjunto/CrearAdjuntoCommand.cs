using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearAdjunto;

/// <summary>Solo texto y direccion (ver ADR-0006): no hay almacenamiento de archivos fisicos.</summary>
public sealed record CrearAdjuntoCommand(int SolicitudId, string Descripcion, string Url)
    : ICommand<Resultado<AdjuntoDto>>;

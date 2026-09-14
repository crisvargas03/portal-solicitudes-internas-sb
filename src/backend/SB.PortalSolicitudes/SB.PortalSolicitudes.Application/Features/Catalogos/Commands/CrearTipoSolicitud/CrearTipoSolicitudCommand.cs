using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearTipoSolicitud;

public sealed record CrearTipoSolicitudCommand(string Nombre, string? Descripcion) : ICommand<Resultado<TipoSolicitudAdminDto>>;

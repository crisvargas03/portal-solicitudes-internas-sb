using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearArea;

public sealed record CrearAreaCommand(string Nombre) : ICommand<Resultado<AreaAdminDto>>;

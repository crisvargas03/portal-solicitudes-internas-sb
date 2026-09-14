using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearPrioridad;

public sealed record CrearPrioridadCommand(string Nombre, int Nivel) : ICommand<Resultado<PrioridadAdminDto>>;

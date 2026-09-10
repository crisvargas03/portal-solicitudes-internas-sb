using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Features.Usuarios.Commands.CrearUsuario;

public sealed record CrearUsuarioCommand(string Nombre, string Email, string Password, RolUsuario Rol)
    : ICommand<Resultado<UsuarioResumenDto>>;

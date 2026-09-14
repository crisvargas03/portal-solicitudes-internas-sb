using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Features.Usuarios.Commands.ActualizarUsuario;

public sealed record ActualizarUsuarioCommand(int Id, string? Nombre, RolUsuario? Rol, bool? Activo)
    : ICommand<Resultado<UsuarioResumenDto>>;

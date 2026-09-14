using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Auth.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Auth.Commands.RegistrarUsuario;

/// <summary>
/// Auto-registro publico (<c>POST /api/auth/register</c>, sin autenticacion). A diferencia
/// de <c>CrearUsuarioCommand</c> (Administrador/Analista, Application/Features/Usuarios) no
/// lleva <c>Rol</c>: el handler siempre crea el usuario como <see cref="Domain.Enums.RolUsuario.Solicitante"/>,
/// para que un cliente anonimo no pueda auto-asignarse Analista o Administrador.
/// </summary>
public sealed record RegistrarUsuarioCommand(string Nombre, string Email, string Password)
    : ICommand<Resultado<SesionDto>>;

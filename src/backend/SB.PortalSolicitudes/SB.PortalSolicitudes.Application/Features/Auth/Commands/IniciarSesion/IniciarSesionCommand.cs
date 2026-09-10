using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Auth.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Auth.Commands.IniciarSesion;

/// <summary>Se une directamente al cuerpo de <c>POST /api/auth/login</c>: no necesita campos que el cliente no deba enviar.</summary>
public sealed record IniciarSesionCommand(string Email, string Password) : ICommand<Resultado<SesionDto>>;

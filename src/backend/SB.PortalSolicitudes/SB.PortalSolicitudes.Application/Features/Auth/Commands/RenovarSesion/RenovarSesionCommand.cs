using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Auth.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Auth.Commands.RenovarSesion;

/// <summary>
/// Respalda <c>POST /api/auth/refresh</c>: re-emision deslizante, sin cuerpo — el usuario
/// sale del token vigente (<see cref="Application.Abstractions.Autenticacion.IUsuarioActual"/>),
/// nunca de la peticion. Ver ADR-0019: solo renueva un token todavia valido, no sustituye
/// un refresh token real.
/// </summary>
public sealed record RenovarSesionCommand : ICommand<Resultado<SesionDto>>;

using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Seed.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Seed.Commands;

/// <summary>
/// Unico punto que toca el esquema y los datos de demostracion (ver ADR-0008, amendada
/// para exponerse como <c>GET /api/seed</c>). Idempotente: puede invocarse varias veces
/// sin duplicar usuarios ni solicitudes.
/// </summary>
public sealed record SeedCommand : ICommand<Resultado<SeedResultadoDto>>;

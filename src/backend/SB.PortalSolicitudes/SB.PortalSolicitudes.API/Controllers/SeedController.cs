using LiteBus.Commands.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.API.Common;
using SB.PortalSolicitudes.Application.Features.Seed.Commands;

namespace SB.PortalSolicitudes.API.Controllers;

/// <summary>Ver ADR-0008 (amendada): unico punto que toca el esquema y los datos de demostracion. Gated en el handler.</summary>
[ApiController]
[Route("api/seed")]
[AllowAnonymous]
public class SeedController : ControllerBase
{
    private readonly ICommandMediator _commandMediator;

    public SeedController(ICommandMediator commandMediator)
    {
        _commandMediator = commandMediator;
    }

    [HttpGet]
    public async Task<IActionResult> Ejecutar(CancellationToken cancellationToken)
    {
        var resultado = await _commandMediator.SendAsync(new SeedCommand(), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }
}

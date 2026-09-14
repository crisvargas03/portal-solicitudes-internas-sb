using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.API.Common;
using SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarArea;
using SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearArea;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerAreasActivas;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerAreasTodas;

namespace SB.PortalSolicitudes.API.Controllers;

[ApiController]
[Route("api/areas")]
[Authorize]
public class AreasController : ControllerBase
{
    private readonly ICommandMediator _commandMediator;
    private readonly IQueryMediator _queryMediator;

    public AreasController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    {
        _commandMediator = commandMediator;
        _queryMediator = queryMediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerActivas(CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(new ObtenerAreasActivasQuery(), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpGet("todas")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ObtenerTodas(CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(new ObtenerAreasTodasQuery(), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] CrearAreaCommand comando, CancellationToken cancellationToken)
    {
        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(
        int id, [FromBody] ActualizarAreaRequest cuerpo, CancellationToken cancellationToken)
    {
        ActualizarAreaCommand comando = new(id, cuerpo.Nombre, cuerpo.Activo);

        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }
}

/// <summary>Cuerpo de <c>PATCH /api/areas/{id}</c>: sin <c>Id</c>, que viene de la ruta.</summary>
public sealed record ActualizarAreaRequest(string? Nombre, bool? Activo);

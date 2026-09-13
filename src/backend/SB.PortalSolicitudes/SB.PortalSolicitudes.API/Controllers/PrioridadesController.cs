using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.API.Common;
using SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarPrioridad;
using SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearPrioridad;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerPrioridadesActivas;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerPrioridadesTodas;

namespace SB.PortalSolicitudes.API.Controllers;

[ApiController]
[Route("api/prioridades")]
[Authorize]
public class PrioridadesController : ControllerBase
{
    private readonly ICommandMediator _commandMediator;
    private readonly IQueryMediator _queryMediator;

    public PrioridadesController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    {
        _commandMediator = commandMediator;
        _queryMediator = queryMediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerActivas(CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(
            new ObtenerPrioridadesActivasQuery(), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpGet("todas")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ObtenerTodas(CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(
            new ObtenerPrioridadesTodasQuery(), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] CrearPrioridadCommand comando, CancellationToken cancellationToken)
    {
        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(
        int id, [FromBody] ActualizarPrioridadRequest cuerpo, CancellationToken cancellationToken)
    {
        ActualizarPrioridadCommand comando = new(id, cuerpo.Nombre, cuerpo.Nivel, cuerpo.Activo);

        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }
}

/// <summary>Cuerpo de <c>PATCH /api/prioridades/{id}</c>: sin <c>Id</c>, que viene de la ruta.</summary>
public sealed record ActualizarPrioridadRequest(string? Nombre, int? Nivel, bool? Activo);

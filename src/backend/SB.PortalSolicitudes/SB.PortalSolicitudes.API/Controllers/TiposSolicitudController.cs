using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.API.Common;
using SB.PortalSolicitudes.Application.Features.Catalogos.Commands.ActualizarTipoSolicitud;
using SB.PortalSolicitudes.Application.Features.Catalogos.Commands.CrearTipoSolicitud;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerTiposSolicitudActivos;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerTiposSolicitudTodos;

namespace SB.PortalSolicitudes.API.Controllers;

[ApiController]
[Route("api/tipos-solicitud")]
[Authorize]
public class TiposSolicitudController : ControllerBase
{
    private readonly ICommandMediator _commandMediator;
    private readonly IQueryMediator _queryMediator;

    public TiposSolicitudController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    {
        _commandMediator = commandMediator;
        _queryMediator = queryMediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerActivos(CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(
            new ObtenerTiposSolicitudActivosQuery(), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpGet("todos")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(
            new ObtenerTiposSolicitudTodosQuery(), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] CrearTipoSolicitudCommand comando, CancellationToken cancellationToken)
    {
        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(
        int id, [FromBody] ActualizarTipoSolicitudRequest cuerpo, CancellationToken cancellationToken)
    {
        ActualizarTipoSolicitudCommand comando = new(id, cuerpo.Nombre, cuerpo.Descripcion, cuerpo.Activo);

        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }
}

/// <summary>Cuerpo de <c>PATCH /api/tipos-solicitud/{id}</c>: sin <c>Id</c>, que viene de la ruta.</summary>
public sealed record ActualizarTipoSolicitudRequest(string? Nombre, string? Descripcion, bool? Activo);

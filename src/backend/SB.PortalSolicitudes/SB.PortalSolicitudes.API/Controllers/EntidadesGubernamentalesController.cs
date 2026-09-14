using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.API.Common;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Commands.ActualizarEntidadGubernamental;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Commands.CrearEntidadGubernamental;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Queries.ObtenerEntidadesGubernamentalesActivas;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Queries.ObtenerEntidadesGubernamentalesTodas;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Queries.ObtenerEntidadGubernamentalPorId;

namespace SB.PortalSolicitudes.API.Controllers;

[ApiController]
[Route("api/entidades-gubernamentales")]
[Authorize]
public class EntidadesGubernamentalesController : ControllerBase
{
    private readonly ICommandMediator _commandMediator;
    private readonly IQueryMediator _queryMediator;

    public EntidadesGubernamentalesController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    {
        _commandMediator = commandMediator;
        _queryMediator = queryMediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerActivas(CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(
            new ObtenerEntidadesGubernamentalesActivasQuery(), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpGet("todas")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ObtenerTodas(CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(
            new ObtenerEntidadesGubernamentalesTodasQuery(), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(
            new ObtenerEntidadGubernamentalPorIdQuery(id), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear(
        [FromBody] CrearEntidadGubernamentalCommand comando, CancellationToken cancellationToken)
    {
        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(
        int id, [FromBody] ActualizarEntidadGubernamentalRequest cuerpo, CancellationToken cancellationToken)
    {
        ActualizarEntidadGubernamentalCommand comando = new(
            id, cuerpo.Nombre, cuerpo.Categoria, cuerpo.PoderDelEstado, cuerpo.Sector, cuerpo.Activo);

        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }
}

/// <summary>Cuerpo de <c>PATCH /api/entidades-gubernamentales/{id}</c>: sin <c>Id</c>, que viene de la ruta.</summary>
public sealed record ActualizarEntidadGubernamentalRequest(
    string? Nombre, string? Categoria, string? PoderDelEstado, string? Sector, bool? Activo);
